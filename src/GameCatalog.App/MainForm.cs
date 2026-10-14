using GameCatalog.App.Services;
using GameCatalog.Core.Models;
using GameCatalog.Localization;
using GameCatalog.UI.Components;
using GameCatalog.UI.Controls;
using GameCatalog.UI.Icons;

namespace GameCatalog.App;

/// <summary>
/// Main window — port of AMC main.pas/main.dfm: menu and dockable toolbars with the original
/// AMC icons, grouped game list with the cover panel, game info tabs (fields, custom fields,
/// all fields, extras, HTML), find bar and status bar. Every command is a UIAction; catalog
/// logic lives in CatalogSession, NumberingService and GameListPresenter.
/// Split into partial files by menu: File, Game, View, Tools.
/// </summary>
public partial class MainForm : Form
{
    private readonly PrefsStore _prefs;
    private readonly CatalogSession _session;
    private readonly NumberingService _numbering;
    private readonly GameListPresenter _presenter;
    private Game? _current;

    private static ITranslator L => AppServices.Languages;

    public MainForm()
    {
        InitializeComponent();
        _prefs = AppServices.Prefs;
        _session = new CatalogSession(_prefs);
        _numbering = new NumberingService(_session);
        _presenter = new GameListPresenter(_session.Games, _prefs);

        iconThemes.Theme = _prefs.Theme;
        iconThemes.SetUseThemeIcons(pictureWindow.Toolbar, true);
        iconThemes.SetUseThemeIcons(extrasFrame.Toolbar, true);
        BindPanelButtons();
        WireEvents();
        ApplyPreferences();
        SetWindowIcon();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        var defaultPath = CatalogSession.DefaultPath();
        var path = _prefs.ReopenLastFile && _prefs.RecentFiles.FirstOrDefault() is { } last && File.Exists(last)
            ? last
            : defaultPath;
        OpenCatalog(path, addSamplesIfNew: path == defaultPath);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!e.Cancel && !ConfirmDiscard("Msg.SaveBeforeClose")) e.Cancel = true;
        base.OnFormClosing(e);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        L.LanguageChanged -= OnLanguageChanged;
        _prefs.Save();
        base.OnFormClosed(e);
    }

    private void BindPanelButtons()
    {
        actionList.SetAction(pictureWindow.LoadButton, actPicLoad);
        actionList.SetAction(pictureWindow.SaveAsButton, actPicSaveAs);
        actionList.SetAction(pictureWindow.CopyButton, actPicCopy);
        actionList.SetAction(pictureWindow.PasteButton, actPicPaste);
        actionList.SetAction(pictureWindow.DeleteButton, actPicDelete);
        actionList.SetAction(extrasFrame.AddButton, actExtraAdd);
        actionList.SetAction(extrasFrame.EditButton, actExtraEdit);
        actionList.SetAction(extrasFrame.DeleteButton, actExtraDelete);
    }

    private void WireEvents()
    {
        _session.Opened += (_, _) => OnCatalogOpened();
        _session.Changed += (_, _) => RefreshList();
        _session.DirtyChanged += (_, _) => UpdateTitle();
        _session.Saving += (_, _) => CommitEditors();

        gameList.SelectedGameChanged += (_, _) => OnSelectionChanged(gameList.SelectedNumber);
        thumbView.SelectedGameChanged += (_, _) => OnSelectionChanged(thumbView.SelectedNumber);
        gameList.GameCheckedChanged += OnGameChecked;
        gameList.SortRequested += (_, e) => SortByColumn(e.Tag);
        gameList.DoubleClick += (_, _) =>
        {
            tabsGame.SelectedTab = tabGameFields;
            gameFrame.FocusTitle();
        };
        gameFrame.Modified += (_, e) => OnGameModified(e.Tag);
        customFrame.Modified += (_, e) => OnGameModified(e.Tag);
        gameFrame.OpenUrlRequested += (_, _) => actUrlOpen.PerformExecute();
        extrasFrame.ExtraActivated += (_, _) => actExtraEdit.PerformExecute();
        tabsGame.SelectedIndexChanged += (_, _) => OnTabChanged();
        findBar.CriteriaChanged += (_, _) => ApplyFind();
        findBar.FindNextRequested += (_, _) => FindNext();
        findBar.CloseRequested += (_, _) => ShowFindBar(false);
        HookDynamicMenus();
        L.LanguageChanged += OnLanguageChanged;
    }

    /// <summary>Asks to save pending changes; false when the user cancelled.</summary>
    private bool ConfirmDiscard(string questionKey)
    {
        CommitEditors();
        if (!_session.IsDirty) return true;
        var answer = MessageBox.Show(this, L.Get(questionKey), L.Get("Common.AppName"),
            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
        return answer switch
        {
            DialogResult.Cancel => false,
            DialogResult.Yes => SaveCatalog(),
            _ => true,
        };
    }

    private void OnCatalogOpened()
    {
        _presenter.State.FindOnly = null;
        customFrame.SetDefinitions(_session.Games.CustomFieldDefs);
        RefreshSuggestions();
        RebuildRecentMenu();
        UpdateTitle();
        RebindCurrent();
    }

    /// <summary>The user selected another game: commit the editors (still bound to the old game), then rebind.</summary>
    private void OnSelectionChanged(int? number)
    {
        CommitEditors();
        _current = number is int n ? _session.Games.Find(n) : null;
        BindFrames();
    }

    /// <summary>Rebinds the frames to the selected game without committing editor values (after undo, paste, apply…).</summary>
    private void RebindCurrent()
    {
        var number = _presenter.State.Thumbnails ? thumbView.SelectedNumber : gameList.SelectedNumber;
        _current = number is int n ? _session.Games.Find(n) : null;
        BindFrames();
    }

    private void BindFrames()
    {
        gameFrame.Bind(_current);
        customFrame.Bind(_current);
        allFieldsFrame.Bind(_current, _session.Games.CustomFieldDefs);
        extrasFrame.Bind(_current);
        pictureWindow.ShowCover(_current?.CoverImage);
        stSelected.Text = _current is null ? "" : $"#{_current.Number} {_current.Title}";
        _ = RefreshHtmlAsync();
    }

    private void OnGameModified(string tag)
    {
        if (_current is null) return;
        _session.MarkModified();
        if (_prefs.ForceFullRefresh) RefreshList(); else UpdateRow(_current);
        allFieldsFrame.Bind(_current, _session.Games.CustomFieldDefs);
        if (tag is "Title" or "LocalizedTitle")
        {
            stSelected.Text = $"#{_current.Number} {_current.Title}";
            _ = RefreshHtmlAsync();
        }
    }

    private void OnGameChecked(object? sender, GameCheckedEventArgs e)
    {
        var game = _session.Games.Find(e.Number);
        if (game is null || game.Checked == e.Checked) return;
        game.Checked = e.Checked;
        _session.MarkModified();
        if (ReferenceEquals(game, _current)) gameFrame.Bind(game);
    }

    private void CommitEditors()
    {
        gameFrame.Commit();
        customFrame.Commit();
    }

    private void UpdateTitle() =>
        Text = L.Get("Main.Caption", Path.GetFileName(_session.FilePath)) + (_session.IsDirty ? " *" : "");

    private void SetHint(string text) => stHint.Text = text;

    private bool RequireCurrent()
    {
        if (_current is not null) return true;
        SetHint(L.Get("Status.NoGameSelected"));
        return false;
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        CommitEditors();
        gameFrame.RefreshCaptions();
        extrasFrame.RefreshCaptions();
        ApplyColumns();
        RefreshList();
        UpdateTitle();
        BindFrames();
    }

    private void SetWindowIcon()
    {
        if (iconThemes.ImageList.Images.Count <= (int)AmcIcon.GridMode) return;
        using var bitmap = new Bitmap(iconThemes.ImageList.Images[(int)AmcIcon.GridMode]);
        Icon = Icon.FromHandle(bitmap.GetHicon());
    }

    // --- Update handlers shared by several actions ---
    private void actRequiresGame_Update(object? sender, EventArgs e) => ((UIAction)sender!).Enabled = _current is not null;
    private void actRequiresCover_Update(object? sender, EventArgs e) => ((UIAction)sender!).Enabled = _current?.HasCover == true;
    private void actRequiresExtra_Update(object? sender, EventArgs e) => ((UIAction)sender!).Enabled = extrasFrame.SelectedExtra is not null;
}
