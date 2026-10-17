using GameCatalog.App.Diagnostics;
using GameCatalog.App.Forms;
using GameCatalog.App.Services;
using GameCatalog.Core.Models;
using GameCatalog.Storage;
using GameCatalog.UI.Icons;
using Microsoft.Data.Sqlite;

namespace GameCatalog.App;

/// <summary>File menu: catalog files, printing, import/export.</summary>
public partial class MainForm
{
    /// <summary>One import or export format offered in the menus and the format dialog.</summary>
    private sealed record ExchangeEntry(string Caption, AmcIcon Icon, Action Run);

    private void actFileNew_Execute(object? sender, EventArgs e)
    {
        if (!ConfirmDiscard("Msg.SaveBeforeNew")) return;
        using var dialog = new SaveFileDialog { Filter = L.Get("Filter.Catalog"), FileName = L.Get("File.DefaultCatalog") };
        if (dialog.ShowDialog(this) == DialogResult.OK)
            RunGuarded("Msg.OpenFailed", () => _session.New(dialog.FileName));
    }

    private void actFileOpen_Execute(object? sender, EventArgs e)
    {
        if (!ConfirmDiscard("Msg.SaveBeforeOpen")) return;
        using var dialog = new OpenFileDialog { Filter = L.Get("Filter.CatalogAll") };
        if (dialog.ShowDialog(this) == DialogResult.OK) OpenCatalog(dialog.FileName);
    }

    private void actFileSave_Execute(object? sender, EventArgs e) => SaveCatalog();

    private void actFileSaveAs_Execute(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog { Filter = L.Get("Filter.Catalog"), FileName = Path.GetFileName(_session.FilePath) };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        if (!RunGuarded("Msg.SaveFailed", () => _session.SaveAs(dialog.FileName))) return;
        RebuildRecentMenu();
        UpdateTitle();
        SetHint(L.Get("Status.Saved", _session.Games.Games.Count));
    }

    private async void actFilePrint_Execute(object? sender, EventArgs e)
    {
        if (!RequireCurrent()) return;
        if (tabsGame.SelectedTab != tabHtml)
        {
            tabsGame.SelectedTab = tabHtml;   // OnTabChanged starts the render; wait for it instead of rendering twice
            await _htmlRender;
        }
        else
        {
            await RefreshHtmlAsync();
        }
        htmlPreview.Print();
    }

    private void actFileImport_Execute(object? sender, EventArgs e) =>
        ChooseExchange(L.Get("Exchange.ImportTitle"), ImportEntries());

    private void actFileExport_Execute(object? sender, EventArgs e) =>
        ChooseExchange(L.Get("Exchange.ExportTitle"), ExportEntries());

    private void actFileProperties_Execute(object? sender, EventArgs e)
    {
        var games = _session.Games.Games;
        using var dialog = new CatalogPropertiesDialog(_session.FilePath, games.Count,
            games.Sum(g => g.Extras.Count), games.Sum(g => g.PlaytimeHours), _session.Loans.Count(l => !l.IsReturned));
        dialog.ShowDialog(this);
    }

    private void actFileExit_Execute(object? sender, EventArgs e) => Close();

    private void OpenCatalog(string path, bool addSamplesIfNew = false) =>
        RunGuarded("Msg.OpenFailed", () => _session.Open(path, addSamplesIfNew));

    private bool SaveCatalog()
    {
        if (!RunGuarded("Msg.SaveFailed", _session.Save)) return false;
        SetHint(L.Get("Status.Saved", _session.Games.Games.Count));
        return true;
    }

    /// <summary>Runs a file operation; I/O and database errors are logged and shown instead of crashing.</summary>
    private bool RunGuarded(string messageKey, Action action)
    {
        try
        {
            action();
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SqliteException or InvalidOperationException)
        {
            AppLog.Error(ex, messageKey);
            MessageBox.Show(this, L.Get(messageKey, ex.Message), L.Get("Common.Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    private void RebuildRecentMenu()
    {
        mnuFileRecent.DropDownItems.Clear();
        var files = _prefs.RecentFiles.Where(File.Exists).ToList();
        if (files.Count == 0)
        {
            mnuFileRecent.DropDownItems.Add(new ToolStripMenuItem(L.Get("Common.None")) { Enabled = false });
            return;
        }
        for (int i = 0; i < files.Count; i++)
        {
            string path = files[i];
            var item = new ToolStripMenuItem($"&{i + 1}: {path}");
            item.Click += (_, _) =>
            {
                if (ConfirmDiscard("Msg.SaveBeforeOpen")) OpenCatalog(path);
            };
            mnuFileRecent.DropDownItems.Add(item);
        }
    }

    // --- Import / export (formats come from the extension registry, see MainForm.Plugins.cs) ---

    private void ChooseExchange(string title, IReadOnlyList<ExchangeEntry> entries)
    {
        using var dialog = new ChoiceDialog(title, L.Get("Exchange.ChooseFormat"), entries.Select(e => e.Caption).ToList());
        if (dialog.ShowDialog(this) == DialogResult.OK && dialog.SelectedIndex >= 0) entries[dialog.SelectedIndex].Run();
    }

    private static void FillExchangeMenu(ToolStripDropDownItem menu, IReadOnlyList<ExchangeEntry> entries)
    {
        menu.DropDownItems.Clear();
        foreach (var entry in entries)
        {
            var item = new ToolStripMenuItem(entry.Caption) { ImageIndex = (int)entry.Icon };
            item.Click += (_, _) => entry.Run();
            menu.DropDownItems.Add(item);
        }
    }

    /// <summary>Adds imported games as one undoable step.</summary>
    private void AddImported(IReadOnlyList<Game> games)
    {
        foreach (var g in games) _session.Games.Add(g);
        _session.Undo.Push(() =>
        {
            foreach (var g in games) _session.Games.Remove(g);
            _session.MarkDirty();
        });
        _session.MarkDirty();
        RefreshSuggestions();
        MessageBox.Show(this, L.Get("Msg.ImportedCount", games.Count), L.Get("Exchange.ImportTitle"));
    }
}
