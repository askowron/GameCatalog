using GameCatalog.App.Diagnostics;
using GameCatalog.App.Forms;
using GameCatalog.App.Services;
using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;
using GameCatalog.Plugins.Abstractions;
using GameCatalog.Scripting;
using GameCatalog.UI.Components;

namespace GameCatalog.App;

/// <summary>Tools and Help menus; getting information from scripts.</summary>
public partial class MainForm
{
    private void actManageFields_Execute(object? sender, EventArgs e)
    {
        CommitEditors();
        using var form = new CustomFieldsForm(_session.Games.CustomFieldDefs);
        form.ShowDialog(this);
        _session.MarkModified();
        customFrame.SetDefinitions(_session.Games.CustomFieldDefs);
        RebindCurrent();
    }

    private void actManagePictures_Execute(object? sender, EventArgs e)
    {
        CommitEditors();
        using var form = new PicturesForm(_session.Games);
        if (form.ShowDialog(this) != DialogResult.OK) return;
        _session.MarkDirty();
        RebindCurrent();
    }

    private void actScripts_Execute(object? sender, EventArgs e)
    {
        if (_current is not { } game) return;
        using var form = new ScriptsForm(game, script => RunScriptAsync(script, selectAfter: false));
        form.ShowDialog(this);
    }

    private void actGetInfoScript_Execute(object? sender, EventArgs e)
    {
        if (!RequireCurrent()) return;
        var scripts = GameScript.LoadAll();
        if (scripts.Count == 0)
        {
            MessageBox.Show(this, L.Get("Msg.NoScripts"), L.Get("Common.AppName"));
            return;
        }
        using var dialog = new ChoiceDialog(L.Get("Script.SelectTitle"), L.Get("Script.SelectPrompt"), scripts.Select(s => s.ToString()).ToList());
        if (dialog.ShowDialog(this) == DialogResult.OK && dialog.SelectedIndex >= 0)
            _ = RunScriptAsync(scripts[dialog.SelectedIndex], selectAfter: true);
    }

    /// <summary>Runs a script for the selected game; never throws (callers fire and forget).</summary>
    private async Task RunScriptAsync(GameScript script, bool selectAfter)
    {
        try
        {
            if (_current is not { } game) return;
            CommitEditors();
            UseWaitCursor = true;
            GameInfoResult result;
            try
            {
                result = await ScriptRunner.RunAsync(script, game.Clone(), PickFromList);
            }
            finally { UseWaitCursor = false; }
            if (IsDisposed) return;
            ApplyInfoResult(game, result, script.Name);
            if (selectAfter && IsInCatalog(game)) SelectGame(game.Number);
        }
        catch (Exception ex)
        {
            ErrorReporter.Show(ex, this);
        }
    }

    /// <summary>Script/provider callback: let the user pick from a list (AMC PickList).</summary>
    private int PickFromList(string title, IList<string> options)
    {
        if (InvokeRequired) return Invoke(() => PickFromList(title, options));
        using var form = new PickForm(title, options);
        return form.ShowDialog(this) == DialogResult.OK ? form.SelectedIndex : -1;
    }

    /// <summary>Shows fetched data, applies what the user ticks as one undoable step (AMC getscript_results).</summary>
    private void ApplyInfoResult(Game game, GameInfoResult result, string sourceName)
    {
        if (!IsInCatalog(game))
        {
            // Deleted, or another catalog was opened, while the script was running.
            SetHint(L.Get("Status.GameRemoved"));
            return;
        }
        if (!result.Success)
        {
            MessageBox.Show(this, result.Error + "\n\n" + string.Join("\n", result.Log), L.Get("Msg.ScriptFailed"),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        using var form = new ScriptResultForm(game, result);
        if (form.ShowDialog(this) != DialogResult.OK || !form.HasSelection) return;
        var backup = game.Clone();
        foreach (var (tag, value) in form.CheckedFields) GameFieldWriter.Set(game, tag, value);
        foreach (var extra in result.Extras)
        {
            extra.GameNumber = game.Number;
            game.Extras.Add(extra);
        }
        if (form.IncludeCover) game.CoverImage = result.Cover;
        _session.Undo.Push(() =>
        {
            game.CopyFrom(backup);
            _session.MarkDirty();
            RebindCurrent();
        });
        _session.MarkDirty();
        RefreshSuggestions();
        RebindCurrent();
        SetHint(L.Get("Status.ScriptApplied", sourceName));
    }

    private void actRenumber_Execute(object? sender, EventArgs e)
    {
        CommitEditors();
        var game = _current;
        var view = _presenter.CurrentView();
        if (view.Count == 0) return;
        if (MessageBox.Show(this, L.Get("Msg.ConfirmRenumber", view.Count), L.Get("Msg.Confirm"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        _numbering.RenumberSequential(view);
        // The list re-selected the old number, which now belongs to another game: follow the instance.
        if (game is not null && IsInCatalog(game)) SelectGame(game.Number);
        RebindCurrent();
    }

    private void actLoans_Execute(object? sender, EventArgs e)
    {
        CommitEditors();
        using var form = new LoanForm(_session.Games, _session.Loans);
        if (form.ShowDialog(this) != DialogResult.OK) return;
        _session.MarkDirty();
        RebindCurrent();
    }

    private void actStats_Execute(object? sender, EventArgs e)
    {
        using var form = new StatsForm(_session.Games, _presenter.CurrentView().Select(g => g.Number).ToHashSet(),
            _current?.Number, _session.FilePath);
        form.ShowDialog(this);
    }

    private void actHtmlEditor_Execute(object? sender, EventArgs e)
    {
        using var form = new HtmlEditorForm(_prefs.HtmlTemplate);
        if (form.ShowDialog(this) == DialogResult.OK) _ = RefreshHtmlAsync();
    }

    private void actPreferences_Execute(object? sender, EventArgs e)
    {
        CommitEditors();
        using var form = new PrefsForm(_prefs);
        if (form.ShowDialog(this) != DialogResult.OK) return;
        _prefs.AutoNumbering = form.AutoNumbering;
        _prefs.TitleFormat = form.TitleFormat;
        _prefs.NaturalSort = form.NaturalSort;
        _prefs.ForceFullRefresh = form.ForceFullRefresh;
        _prefs.ReopenLastFile = form.ReopenLastFile;
        _prefs.FullRowSelect = form.FullRowSelect;
        _prefs.ShowGridLines = form.ShowGridLines;
        _prefs.CoverVisible = form.CoverVisible;
        _prefs.ShowExtraCategory = form.ShowExtraCategory;
        _prefs.CsvDelimiter = form.CsvDelimiter;
        _prefs.BackupOnSave = form.BackupOnSave;
        _prefs.MediaPatterns = form.MediaPatterns;
        _prefs.MaxCoverWidth = form.MaxCoverWidth;
        _prefs.ScriptTimeoutSec = Math.Max(5, form.ScriptTimeoutSec);
        _prefs.CustomLists = form.CustomLists;
        _prefs.CustomSearchSites = form.CustomSearchSites;
        _prefs.HtmlTemplate = form.HtmlTemplate;
        _prefs.ShowGroupCount = form.ShowGroupCount;
        _prefs.Theme = form.Theme;
        _prefs.ShowMenuIcons = form.ShowMenuIcons;
        _prefs.Save();
        ApplyPreferences();
        RefreshList();
        BindFrames();
    }

    private void actHelpIndex_Execute(object? sender, EventArgs e)
    {
        using var form = new HelpForm();
        form.ShowDialog(this);
    }

    private void actHelpAbout_Execute(object? sender, EventArgs e)
    {
        using var form = new AboutForm();
        form.ShowDialog(this);
    }

    private void actHelpVersions_Execute(object? sender, EventArgs e)
    {
        using var form = new VersionsForm();
        form.ShowDialog(this);
    }

    private void actLanguage_Execute(object? sender, EventArgs e)
    {
        using var form = new LanguageSelectForm(AppServices.Languages.Available, AppServices.Languages.CurrentLanguage);
        if (form.ShowDialog(this) != DialogResult.OK || form.SelectedCode is not { } code) return;
        AppServices.Languages.SetLanguage(code);   // every open window re-translates itself
        _prefs.Language = code;
        _prefs.Save();
    }

    private void actHelpExportTranslation_Execute(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog
        {
            Filter = L.Get("Filter.Json"),
            FileName = AppServices.Languages.CurrentLanguage + ".json",
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            File.WriteAllText(dialog.FileName, AppServices.Languages.ExportTemplate(Localizer.CollectAllLiveKeys()));
            SetHint(L.Get("Status.Exported", dialog.FileName));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            AppLog.Error(ex, "Export translation template");
            MessageBox.Show(this, L.Get("Msg.ExportError", ex.Message), L.Get("Common.Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
