using GameCatalog.App.Services;
using GameCatalog.Core.Fields;
using GameCatalog.Localization;
using GameCatalog.UI.Components;
using GameCatalog.UI.Icons;

namespace GameCatalog.App.Forms;

/// <summary>
/// Preferences in the style of AMC options.pas — a page tree plus one panel per page.
/// Every option is applied by the caller from the result properties and saved to prefs.json.
/// </summary>
public partial class PrefsForm : Form
{
    private sealed record ListItem(string Value, string Text)
    {
        public override string ToString() => Text;
    }

    public string Theme { get; private set; }
    public bool AutoNumbering { get; private set; }
    public bool ShowGroupCount { get; private set; }
    public string HtmlTemplate { get; private set; }
    public string TitleFormat { get; private set; }
    public bool NaturalSort { get; private set; }
    public bool ForceFullRefresh { get; private set; }
    public bool ReopenLastFile { get; private set; }
    public bool FullRowSelect { get; private set; }
    public bool ShowGridLines { get; private set; }
    public bool ShowMenuIcons { get; private set; }
    public bool CoverVisible { get; private set; }
    public bool ShowExtraCategory { get; private set; }
    public string CsvDelimiter { get; private set; }
    public bool BackupOnSave { get; private set; }
    public string MediaPatterns { get; private set; }
    public int MaxCoverWidth { get; private set; }
    public int ScriptTimeoutSec { get; private set; }
    public Dictionary<string, List<string>> CustomLists { get; private set; } = new();
    public List<SearchSite> CustomSearchSites { get; private set; } = new();

    /// <summary>Fields whose drop-down lists the user can extend.</summary>
    private static readonly string[] ListKeys =
        { "MediaType", "Store", "Platform", "GameMode", "Genre", "Country", "Developer", "Publisher", "Engine" };

    private readonly Dictionary<string, List<string>> _customLists;
    private readonly List<SearchSite> _sites;

    public PrefsForm(PrefsStore prefs)
    {
        InitializeComponent();
        Theme = IconThemeImageList.NormalizeThemeId(prefs.Theme);
        AutoNumbering = prefs.AutoNumbering;
        ShowGroupCount = prefs.ShowGroupCount;
        HtmlTemplate = prefs.HtmlTemplate;
        TitleFormat = prefs.TitleFormat;
        NaturalSort = prefs.NaturalSort;
        ForceFullRefresh = prefs.ForceFullRefresh;
        ReopenLastFile = prefs.ReopenLastFile;
        FullRowSelect = prefs.FullRowSelect;
        ShowGridLines = prefs.ShowGridLines;
        ShowMenuIcons = prefs.ShowMenuIcons;
        CoverVisible = prefs.CoverVisible;
        ShowExtraCategory = prefs.ShowExtraCategory;
        CsvDelimiter = prefs.CsvDelimiter;
        BackupOnSave = prefs.BackupOnSave;
        MediaPatterns = prefs.MediaPatterns;
        MaxCoverWidth = prefs.MaxCoverWidth;
        ScriptTimeoutSec = prefs.ScriptTimeoutSec;
        _customLists = prefs.CustomLists.ToDictionary(kv => kv.Key, kv => kv.Value.ToList());
        _sites = prefs.CustomSearchSites.Select(s => s with { }).ToList();
        if (_sites.Count == 0) _sites = PrefsStore.DefaultSearchSites();

        LoadWindowIcon();
        BindValues(AppServices.Languages);
        BuildTree(AppServices.Languages);
        WireListManager();
        WireSiteManager();

        btnHelp.Click += (_, _) => { using var h = new HelpForm(); h.ShowDialog(this); };
        btnSave.Click += (_, _) => SaveToProps();
        tvPages.AfterSelect += (_, e) =>
        {
            if (e.Node?.Tag is not Panel selected) return;
            foreach (Control p in pnlPages.Controls) p.Visible = p == selected;
        };
        tvPages.SelectedNode = tvPages.Nodes[0];
    }

    private void LoadWindowIcon()
    {
        try
        {
            using var strip = IconThemeImageList.LoadStrip(IconThemeImageList.DefaultThemeId, Path.Combine(AppContext.BaseDirectory, "Resources"));
            var bmp = (Bitmap)strip.Images[(int)AmcIcon.Options];
            Icon = (Icon)Icon.FromHandle(bmp.GetHicon()).Clone();
        }
        catch { }
    }

    /// <summary>Fills the editors from the current values; items computed from data and translations live here.</summary>
    private void BindValues(ITranslator l)
    {
        chkAutoNumbering.Checked = AutoNumbering;
        chkNaturalSort.Checked = NaturalSort;
        chkForceFullRefresh.Checked = ForceFullRefresh;

        foreach (var th in IconThemeImageList.Themes) cbxTheme.Items.Add(new ListItem(th.Id, l.Get(th.NameKey)));
        cbxTheme.SelectedItem = cbxTheme.Items.Cast<ListItem>().FirstOrDefault(i => i.Value == Theme);
        if (cbxTheme.SelectedIndex < 0) cbxTheme.SelectedIndex = 0;
        // Colour schemes are not implemented yet; the combo mirrors the AMC page and stays disabled.
        cbxColorScheme.Items.AddRange(new object[]
            { l.Get("Prefs.ColorScheme.Auto"), l.Get("Prefs.ColorScheme.Light"), l.Get("Prefs.ColorScheme.Dark") });
        cbxColorScheme.SelectedIndex = 0;
        cbxColorScheme.Enabled = false;
        chkMenuIcons.Checked = ShowMenuIcons;

        chkReopenLastFile.Checked = ReopenLastFile;
        chkBackupOnSave.Checked = BackupOnSave;
        chkFullRowSelect.Checked = FullRowSelect;
        chkShowGridLines.Checked = ShowGridLines;

        foreach (var value in new[] { "Both", "Original", "Localized" })
            cbxTitleFormat.Items.Add(new ListItem(value, l.Get("Prefs.TitleFormat." + value)));
        cbxTitleFormat.SelectedIndex = Math.Max(0, cbxTitleFormat.Items.Cast<ListItem>().ToList().FindIndex(i => i.Value == TitleFormat));

        chkShowGroupCount.Checked = ShowGroupCount;
        chkShowExtraCategory.Checked = ShowExtraCategory;
        chkCoverVisible.Checked = CoverVisible;
        txtMediaPatterns.Text = MediaPatterns;
        numMaxCoverWidth.Value = Math.Clamp(MaxCoverWidth, 0, 4000);
        numScriptTimeout.Value = Math.Clamp(ScriptTimeoutSec, 5, 600);

        foreach (var n in HtmlTemplates.Names) cbxHtmlTemplate.Items.Add(new ListItem(n, HtmlTemplates.DisplayName(n)));
        cbxHtmlTemplate.SelectedItem = cbxHtmlTemplate.Items.Cast<ListItem>().FirstOrDefault(i => i.Value == HtmlTemplate);
        if (cbxHtmlTemplate.SelectedIndex < 0) cbxHtmlTemplate.SelectedIndex = 0;
        cbxCsvDelimiter.Items.Add(new ListItem(";", l.Get("Prefs.Delimiter.Semicolon")));
        cbxCsvDelimiter.Items.Add(new ListItem(",", l.Get("Prefs.Delimiter.Comma")));
        cbxCsvDelimiter.Items.Add(new ListItem("TAB", l.Get("Prefs.Delimiter.Tab")));
        cbxCsvDelimiter.SelectedIndex = CsvDelimiter == "," ? 1 : CsvDelimiter == "TAB" ? 2 : 0;

        txtDataFolder.Text = Path.GetDirectoryName(PrefsStore.PrefsPath()) ?? "";
        txtScriptsFolder.Text = GameCatalog.Scripting.GameScript.ScriptsDir();
    }

    /// <summary>Page tree with the AMC hierarchy; node texts are translated here (tree nodes are not controls).</summary>
    private void BuildTree(ITranslator l)
    {
        try
        {
            var strip = IconThemeImageList.LoadStrip(Theme, Path.Combine(AppContext.BaseDirectory, "Resources"));
            components.Add(strip);
            tvPages.ImageList = strip;
        }
        catch { }

        TreeNode Node(string page, AmcIcon icon, Panel panel, params TreeNode[] children)
        {
            var n = new TreeNode(l.Get("Prefs.Page." + page)) { Tag = panel };
            if (tvPages.ImageList is not null && (int)icon < tvPages.ImageList.Images.Count)
                n.ImageIndex = n.SelectedImageIndex = (int)icon;
            n.Nodes.AddRange(children);
            return n;
        }

        tvPages.Nodes.AddRange(new[]
        {
            Node("General", AmcIcon.Options, pnlPageGeneral),
            Node("Icons", AmcIcon.GridMode, pnlPageIcons),
            Node("Files", AmcIcon.FileOpen, pnlPageFiles),
            Node("List", AmcIcon.ViewList, pnlPageList,
                Node("TitleFormat", AmcIcon.FieldEdit, pnlPageTitleFormat),
                Node("Grouping", AmcIcon.Group, pnlPageGrouping)),
            Node("Extras", AmcIcon.MovieExtras, pnlPageExtras),
            Node("Game", AmcIcon.MovieFind, pnlPageGame,
                Node("Lists", AmcIcon.ViewDetail, pnlPageLists),
                Node("ImportMedia", AmcIcon.MovieImportFiles, pnlPageImportMedia),
                Node("ImportPictures", AmcIcon.PictureOpen, pnlPageImportPictures),
                Node("Search", AmcIcon.MovieSearch, pnlPageSearch)),
            Node("Scripts", AmcIcon.Scripting, pnlPageScripts),
            Node("Export", AmcIcon.FileExport, pnlPageExport),
            Node("Folders", AmcIcon.Browse, pnlPageFolders),
        });
    }

    private void SaveToProps()
    {
        Theme = (cbxTheme.SelectedItem as ListItem)?.Value ?? Theme;
        AutoNumbering = chkAutoNumbering.Checked;
        NaturalSort = chkNaturalSort.Checked;
        ForceFullRefresh = chkForceFullRefresh.Checked;
        ShowMenuIcons = chkMenuIcons.Checked;
        ReopenLastFile = chkReopenLastFile.Checked;
        BackupOnSave = chkBackupOnSave.Checked;
        FullRowSelect = chkFullRowSelect.Checked;
        ShowGridLines = chkShowGridLines.Checked;
        TitleFormat = (cbxTitleFormat.SelectedItem as ListItem)?.Value ?? TitleFormat;
        ShowGroupCount = chkShowGroupCount.Checked;
        ShowExtraCategory = chkShowExtraCategory.Checked;
        CoverVisible = chkCoverVisible.Checked;
        HtmlTemplate = (cbxHtmlTemplate.SelectedItem as ListItem)?.Value ?? HtmlTemplate;
        CsvDelimiter = (cbxCsvDelimiter.SelectedItem as ListItem)?.Value ?? CsvDelimiter;
        MediaPatterns = txtMediaPatterns.Text.Trim();
        if (string.IsNullOrWhiteSpace(MediaPatterns)) MediaPatterns = "*.exe";
        MaxCoverWidth = (int)numMaxCoverWidth.Value;
        ScriptTimeoutSec = Math.Max(5, (int)numScriptTimeout.Value);
        CustomLists = _customLists.Where(kv => kv.Value.Count > 0)
            .ToDictionary(kv => kv.Key, kv => kv.Value.ToList());
        CustomSearchSites = _sites.Select(s => s with { }).ToList();
    }

    private void WireListManager()
    {
        foreach (var key in ListKeys) cbxListKeys.Items.Add(CatalogLabels.Current.Field(key));
        cbxListKeys.SelectedIndex = 0;
        cbxListKeys.SelectedIndexChanged += (_, _) => RefreshValues();
        btnListDelete.Click += (_, _) =>
        {
            if (lstListValues.SelectedItem is string s && cbxListKeys.SelectedIndex >= 0)
            {
                CurrentList().Remove(s);
                RefreshValues();
            }
        };
        btnListAdd.Click += (_, _) =>
        {
            var v = txtNewValue.Text.Trim();
            if (v.Length == 0 || cbxListKeys.SelectedIndex < 0) return;
            var list = CurrentList();
            if (!list.Contains(v)) list.Add(v);
            txtNewValue.Clear();
            RefreshValues();
        };
        RefreshValues();

        List<string> CurrentList()
        {
            string key = ListKeys[cbxListKeys.SelectedIndex];
            if (!_customLists.TryGetValue(key, out var list))
            {
                list = new List<string>();
                _customLists[key] = list;
            }
            return list;
        }

        void RefreshValues()
        {
            lstListValues.Items.Clear();
            if (cbxListKeys.SelectedIndex < 0) return;
            foreach (var v in CurrentList()) lstListValues.Items.Add(v);
        }
    }

    private void WireSiteManager()
    {
        btnSiteDefaults.Click += (_, _) =>
        {
            _sites.Clear();
            _sites.AddRange(PrefsStore.DefaultSearchSites());
            RefreshSites();
        };
        btnSiteDelete.Click += (_, _) =>
        {
            if (lstSites.SelectedIndex < 0) return;
            _sites.RemoveAt(lstSites.SelectedIndex);
            RefreshSites();
        };
        btnSiteAdd.Click += (_, _) =>
        {
            var name = txtSiteName.Text.Trim();
            var url = txtSiteUrl.Text.Trim();
            if (name.Length == 0 || url.Length == 0) return;
            if (!url.Contains("{q}"))
            {
                var l = AppServices.Languages;
                MessageBox.Show(this, l.Get("Prefs.SearchUrlInvalid"), l.Get("Prefs.SearchTitle"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _sites.RemoveAll(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            _sites.Add(new SearchSite(name, url));
            txtSiteName.Clear();
            txtSiteUrl.Clear();
            RefreshSites();
        };
        RefreshSites();

        void RefreshSites()
        {
            lstSites.Items.Clear();
            foreach (var s in _sites) lstSites.Items.Add(s.Name);
        }
    }
}
