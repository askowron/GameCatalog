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

    // --- Import / export (built-in formats; plugin formats are merged in Task 15) ---

    private IReadOnlyList<ExchangeEntry> ImportEntries() => new[]
    {
        new ExchangeEntry(L.Get("Exchange.ImportFolder"), AmcIcon.MovieImportFiles, ImportFromFolder),
        new ExchangeEntry(L.Get("Exchange.ImportCsv"), AmcIcon.FileImport, ImportFromCsv),
        new ExchangeEntry(L.Get("Exchange.ImportAmc"), AmcIcon.MovieImport, ImportFromAmc),
    };

    private IReadOnlyList<ExchangeEntry> ExportEntries() => new[]
    {
        new ExchangeEntry(L.Get("Exchange.ExportCsv"), AmcIcon.FileExport, ExportToCsv),
        new ExchangeEntry(L.Get("Exchange.ExportHtml"), AmcIcon.Html, ExportToHtml),
    };

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

    private void ImportFromFolder()
    {
        using var dialog = new FolderBrowserDialog { Description = L.Get("Folder.SelectGames"), UseDescriptionForTitle = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        AddImported(() => CatalogTransfer.ImportFromFolder(dialog.SelectedPath, _session.Games.NextNumber(), _prefs.MediaPatterns));
    }

    private void ImportFromCsv()
    {
        using var dialog = new OpenFileDialog { Filter = L.Get("Filter.Csv") };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        AddImported(() => CatalogTransfer.ImportCsv(dialog.FileName, _session.Games.NextNumber(), _prefs.CsvSeparator()));
    }

    private void ImportFromAmc()
    {
        using var dialog = new OpenFileDialog { Filter = L.Get("Filter.AmcXml") };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        AddImported(() => AmcXmlImporter.Import(dialog.FileName, _session.Games.NextNumber()));
    }

    /// <summary>Adds imported games as one undoable step.</summary>
    private void AddImported(Func<List<Game>> import)
    {
        List<Game> games;
        try
        {
            games = import();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Xml.XmlException or FormatException)
        {
            AppLog.Error(ex, "Import");
            MessageBox.Show(this, L.Get("Msg.ImportError", ex.Message), L.Get("Common.Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
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

    private void ExportToCsv() => ExportTo(L.Get("Filter.Csv"), L.Get("File.DefaultCsv"),
        path => CatalogTransfer.ExportCsv(_presenter.CurrentView(), path, _prefs.CsvSeparator()));

    private void ExportToHtml() => ExportTo(L.Get("Filter.Html"), L.Get("File.DefaultHtml"),
        path => CatalogTransfer.ExportHtml(_presenter.CurrentView(), path, L.Get("Export.HtmlTitle")));

    private void ExportTo(string filter, string fileName, Action<string> export)
    {
        using var dialog = new SaveFileDialog { Filter = filter, FileName = fileName };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            export(dialog.FileName);
            SetHint(L.Get("Status.Exported", dialog.FileName));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            AppLog.Error(ex, "Export");
            MessageBox.Show(this, L.Get("Msg.ExportError", ex.Message), L.Get("Common.Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
