using GameCatalog.App.Services;
using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;
using GameCatalog.Scripting;
using GameCatalog.UI.Controls;
using GameCatalog.UI.Icons;

namespace GameCatalog.App;

/// <summary>View menu: list, sorting, grouping, columns, thumbnails, HTML preview, find bar, bars.</summary>
public partial class MainForm
{
    /// <summary>Marks menu items generated at runtime so they can be replaced.</summary>
    private const string DynamicItem = "dynamic";

    /// <summary>The latest HTML preview render (printing waits for it).</summary>
    private Task _htmlRender = Task.CompletedTask;

    private void RefreshList()
    {
        var vm = _presenter.Build();
        if (_presenter.State.Thumbnails)
            thumbView.Display(vm.Rows.Select(r => (r.Game.Number, _presenter.FormatTitle(r.Game), r.Game.CoverImage)).ToList());
        else
            gameList.Display(vm.Rows.Select(ToItem).ToList(), vm.Groups.Select(g => new GameListGroup(g.Key, g.Caption)).ToList());
        stCount.Text = _presenter.State.GroupTag.Length == 0
            ? L.Get("Status.Count", vm.Rows.Count, vm.Total)
            : L.Get("Status.CountGroups", vm.Rows.Count, vm.Total, vm.Groups.Count);
    }

    private static GameListItem ToItem(GameRow row) =>
        new(row.Game.Number, row.Game.Checked, row.Game.ColorTag, row.Cells, row.GroupKey);

    private void UpdateRow(Game game) =>
        gameList.UpdateItem(new GameListItem(game.Number, game.Checked, game.ColorTag,
            _presenter.State.Columns.Select(c => _presenter.CellText(game, c.Tag)).ToList(), ""));

    private void SelectGame(int number)
    {
        if (_presenter.State.Thumbnails) thumbView.SelectNumber(number);
        else gameList.SelectNumber(number);
    }

    private void ApplyColumns() =>
        gameList.SetColumns(_presenter.State.Columns.Select(c => (c.Tag, c.Width)), CatalogLabels.Current.Field);

    /// <summary>Copies widths the user dragged in the list back into the view state.</summary>
    private void SyncColumnWidths()
    {
        foreach (var (tag, width) in gameList.ColumnWidths())
            if (_presenter.State.Columns.FirstOrDefault(c => c.Tag == tag) is { } column) column.Width = width;
    }

    private void ApplyPreferences()
    {
        gameList.FullRowSelect = _prefs.FullRowSelect;
        gameList.GridLines = _prefs.ShowGridLines;
        extrasFrame.ShowCategory = _prefs.ShowExtraCategory;
        if (iconThemes.Theme != _prefs.Theme) iconThemes.Theme = _prefs.Theme;
        iconThemes.SetUseThemeIcons(menuMain, _prefs.ShowMenuIcons);
        if (!_prefs.ShowMenuIcons) ClearImageLists(menuMain.Items);
        ScriptRunner.Timeout = TimeSpan.FromSeconds(Math.Max(5, _prefs.ScriptTimeoutSec));
        ApplyCoverVisibility();
        ApplyColumns();
        RefreshSuggestions();
    }

    private static void ClearImageLists(ToolStripItemCollection items)
    {
        foreach (ToolStripItem item in items)
        {
            if (item.Owner is not null) item.Owner.ImageList = null;
            if (item is ToolStripDropDownItem dd)
            {
                dd.DropDown.ImageList = null;
                ClearImageLists(dd.DropDownItems);
            }
        }
    }

    /// <summary>Editor drop-downs: values used in the catalog plus the user's custom lists (AMC lists).</summary>
    private void RefreshSuggestions()
    {
        foreach (var tag in new[] { "Genre", "Developer", "Publisher", "Store", "Platform", "Country", "Engine", "MediaType", "GameMode" })
        {
            var values = _session.Games.DistinctValues(g => GameFieldValues.Get(g, tag))
                .Concat(_prefs.CustomLists.GetValueOrDefault(tag) ?? new List<string>());
            gameFrame.SetSuggestions(tag, values);
        }
    }

    // --- Sorting and grouping ---

    private void SetSort(string tag)
    {
        _presenter.State.SortTag = tag;
        RefreshList();
    }

    private void SortByColumn(string tag)
    {
        var state = _presenter.State;
        if (tag == state.SortTag) state.Ascending = !state.Ascending;
        else
        {
            state.SortTag = tag;
            state.Ascending = true;
        }
        RefreshList();
    }

    private void actSortAscending_Execute(object? sender, EventArgs e) => SetOrder(true);
    private void actSortDescending_Execute(object? sender, EventArgs e) => SetOrder(false);
    private void actSortAscending_Update(object? sender, EventArgs e) => actSortAscending.Checked = _presenter.State.Ascending;
    private void actSortDescending_Update(object? sender, EventArgs e) => actSortDescending.Checked = !_presenter.State.Ascending;

    private void SetOrder(bool ascending)
    {
        _presenter.State.Ascending = ascending;
        RefreshList();
    }

    private void SetGroup(string tag)
    {
        _presenter.State.GroupTag = tag;
        RefreshList();
    }

    private void actGroupCount_Execute(object? sender, EventArgs e)
    {
        _prefs.ShowGroupCount = !_prefs.ShowGroupCount;
        _prefs.Save();
        RefreshList();
    }

    private void actGroupCount_Update(object? sender, EventArgs e) => actGroupCount.Checked = _prefs.ShowGroupCount;

    private void actStretchList_Execute(object? sender, EventArgs e) => gameList.StretchFirstColumn();

    private void actFlatTable_Execute(object? sender, EventArgs e)
    {
        if (_presenter.State.Thumbnails) SetThumbnails(false);
        SetGroup("");
    }

    private void actFlatTable_Update(object? sender, EventArgs e) =>
        actFlatTable.Checked = _presenter.State.GroupTag.Length == 0 && !_presenter.State.Thumbnails;

    private void actThumbnails_Execute(object? sender, EventArgs e) => SetThumbnails(!_presenter.State.Thumbnails);
    private void actThumbnails_Update(object? sender, EventArgs e) => actThumbnails.Checked = _presenter.State.Thumbnails;

    private void SetThumbnails(bool on)
    {
        int? number = _current?.Number;
        _presenter.State.Thumbnails = on;
        thumbView.Visible = on;
        gameList.Visible = !on;
        RefreshList();
        if (number is int n) SelectGame(n);
    }

    // --- HTML preview ---

    private void actShowHtml_Execute(object? sender, EventArgs e) => tabsGame.SelectedTab = tabHtml;
    private void actShowHtml_Update(object? sender, EventArgs e) => actShowHtml.Checked = tabsGame.SelectedTab == tabHtml;
    private void actHtmlRefresh_Execute(object? sender, EventArgs e) => _ = RefreshHtmlAsync();

    private void actHtmlCopy_Execute(object? sender, EventArgs e)
    {
        if (_current is null) return;
        Clipboard.SetText(BuildHtml(_current));
        SetHint(L.Get("Status.HtmlCopied"));
    }

    private string BuildHtml(Game game) =>
        HtmlTemplates.Expand(HtmlTemplates.Load(_prefs.HtmlTemplate), game, CoverImages.ToDataUri(game.CoverImage));

    /// <summary>Renders the preview only while its tab is visible (WebView2 work is not free).</summary>
    private Task RefreshHtmlAsync()
    {
        if (tabsGame.SelectedTab != tabHtml) return Task.CompletedTask;
        return _htmlRender = htmlPreview.ShowHtmlAsync(_current is null ? "<html><body></body></html>" : BuildHtml(_current));
    }

    private void OnTabChanged() => _ = RefreshHtmlAsync();

    // --- Bars ---

    private void actViewMainToolbar_Execute(object? sender, EventArgs e) => tsMain.Visible = !tsMain.Visible;
    private void actViewMainToolbar_Update(object? sender, EventArgs e) => actViewMainToolbar.Checked = tsMain.Visible;
    private void actViewStatusBar_Execute(object? sender, EventArgs e) => statusMain.Visible = !statusMain.Visible;
    private void actViewStatusBar_Update(object? sender, EventArgs e) => actViewStatusBar.Checked = statusMain.Visible;

    // --- Find bar (AMC ToolbarFind) ---

    private void ShowFindBar(bool show)
    {
        findBar.Visible = show;
        actGameFind.Checked = show;
        if (show)
        {
            findBar.SetFields(GameFields.Searchable.Select(f => (f.Tag, CatalogLabels.Current.Field(f.Tag))), findBar.FieldTag);
            findBar.FocusText();
        }
        else
        {
            _presenter.State.FindOnly = null;
            RefreshList();
        }
    }

    private FindCriteria CurrentCriteria() =>
        new(findBar.FieldTag, findBar.SearchText, findBar.WholeField, findBar.Reverse);

    private void ApplyFind()
    {
        var criteria = CurrentCriteria();
        if (criteria.Text.Length == 0 || findBar.ShowResults)
        {
            _presenter.State.FindOnly = criteria.Text.Length == 0 ? null : _presenter.FindAll(criteria);
            RefreshList();
            return;
        }
        _presenter.State.FindOnly = null;
        RefreshList();
        var match = _presenter.FindFirst(criteria);
        if (match is not null) SelectGame(match.Number);
        SetHint(match is null ? L.Get("Status.NotFound") : L.Get("Status.Found", match.Number, match.Title));
    }

    private void FindNext()
    {
        var criteria = CurrentCriteria();
        if (criteria.Text.Length == 0) return;
        var next = _presenter.FindNext(criteria, _current?.Number);
        if (next is null) SetHint(L.Get("Status.NotFound"));
        else SelectGame(next.Number);
    }

    // --- Menus built at runtime (rebuilt on every opening, so they follow language and state) ---

    private void HookDynamicMenus()
    {
        mnuViewSort.DropDownOpening += (_, _) => ReplaceDynamicItems(mnuViewSort, SortItems());
        mnuViewGroup.DropDownOpening += (_, _) => ReplaceDynamicItems(mnuViewGroup, GroupItems());
        tbGroup.DropDownOpening += (_, _) => ReplaceDynamicItems(tbGroup, GroupItems());
        mnuViewGridFields.DropDownOpening += (_, _) => ReplaceDynamicItems(mnuViewGridFields, ColumnItems());
        tbGridFields.DropDownOpening += (_, _) => ReplaceDynamicItems(tbGridFields, ColumnItems());
        mnuViewSelectHtml.DropDownOpening += (_, _) => ReplaceDynamicItems(mnuViewSelectHtml, HtmlTemplateItems());
        mnuGameWeb.DropDownOpening += (_, _) => ReplaceDynamicItems(mnuGameWeb, WebSearchItems());
        mnuCtxColorTag.DropDownOpening += (_, _) => ReplaceDynamicItems(mnuCtxColorTag, ColorTagItems());
        tbImport.DropDownOpening += (_, _) => FillExchangeMenu(tbImport, ImportEntries());
        tbExport.DropDownOpening += (_, _) => FillExchangeMenu(tbExport, ExportEntries());
        mnuGameGetInfo.DropDownOpening += (_, _) => ReplaceDynamicItems(mnuGameGetInfo, InfoProviderItems(), append: true);
        // Placeholders so the drop-down arrows open before the first rebuild.
        foreach (var menu in new ToolStripDropDownItem[] { mnuViewGroup, tbGroup, mnuViewGridFields, tbGridFields, mnuViewSelectHtml, mnuGameWeb, mnuCtxColorTag, tbImport, tbExport })
            if (menu.DropDownItems.Count == 0) menu.DropDownItems.Add(new ToolStripMenuItem("…") { Tag = DynamicItem });
    }

    /// <summary>Replaces the items built at runtime; they go before the static items, or after them with <paramref name="append"/>.</summary>
    private static void ReplaceDynamicItems(ToolStripDropDownItem menu, IEnumerable<ToolStripItem> items, bool append = false)
    {
        foreach (var old in menu.DropDownItems.Cast<ToolStripItem>().Where(i => Equals(i.Tag, DynamicItem)).ToList())
        {
            menu.DropDownItems.Remove(old);
            old.Dispose();
        }
        int index = append ? menu.DropDownItems.Count : 0;
        foreach (var item in items)
        {
            item.Tag = DynamicItem;
            menu.DropDownItems.Insert(index++, item);
        }
    }

    private static ToolStripMenuItem MenuItem(string text, bool isChecked, Action onClick, AmcIcon icon = AmcIcon.None)
    {
        var item = new ToolStripMenuItem(text) { Checked = isChecked };
        if (icon != AmcIcon.None) item.ImageIndex = (int)icon;
        item.Click += (_, _) => onClick();
        return item;
    }

    private IEnumerable<ToolStripItem> SortItems() =>
        GameFields.SortableTags.Select(tag =>
            (ToolStripItem)MenuItem(CatalogLabels.Current.Field(tag), _presenter.State.SortTag == tag, () => SetSort(tag)));

    private IEnumerable<ToolStripItem> GroupItems()
    {
        yield return MenuItem(L.Get("Common.None"), _presenter.State.GroupTag.Length == 0, () => SetGroup(""), AmcIcon.Group);
        yield return new ToolStripSeparator();
        foreach (var tag in GameFields.GroupableTags)
            yield return MenuItem(CatalogLabels.Current.Field(tag), _presenter.State.GroupTag == tag, () => SetGroup(tag), AmcIcon.Group);
    }

    private IEnumerable<ToolStripItem> ColumnItems()
    {
        var visible = _presenter.State.Columns.Select(c => c.Tag).ToHashSet();
        foreach (var (tag, _) in GameListPresenter.AvailableColumns)
        {
            var item = new ToolStripMenuItem(CatalogLabels.Current.Field(tag)) { Checked = visible.Contains(tag), CheckOnClick = true };
            item.CheckedChanged += (_, _) =>
            {
                SyncColumnWidths();
                _presenter.ToggleColumn(tag, item.Checked);
                ApplyColumns();
                RefreshList();
            };
            yield return item;
        }
    }

    private IEnumerable<ToolStripItem> HtmlTemplateItems() =>
        HtmlTemplates.Names.Select(name => (ToolStripItem)MenuItem(name, _prefs.HtmlTemplate == name, () =>
        {
            _prefs.HtmlTemplate = name;
            _prefs.Save();
            _ = RefreshHtmlAsync();
        }));

    private IEnumerable<ToolStripItem> WebSearchItems()
    {
        var sites = _prefs.CustomSearchSites.Count > 0 ? _prefs.CustomSearchSites : PrefsStore.DefaultSearchSites();
        return sites.Select(site => (ToolStripItem)MenuItem(site.Name, false, () => WebSearch(site), AmcIcon.MovieSearch));
    }

    private IEnumerable<ToolStripItem> ColorTagItems()
    {
        for (int i = 0; i < ColorTags.Count; i++)
        {
            int tag = i;
            var item = MenuItem(tag == 0 ? L.Get("Common.None") : L.Get("Menu.ColorTag", tag), _current?.ColorTag == tag, () => SetColorTag(tag));
            if (tag > 0) item.Image = ColorTags.Swatch(tag);
            yield return item;
        }
    }
}
