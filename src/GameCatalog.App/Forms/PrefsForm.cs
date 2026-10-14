using GameCatalog.App.Services;

namespace GameCatalog.App;

/// <summary>Preferencje w stylu AMC options.pas — drzewo kategorii + panele.
/// Wszystkie parametry działają i zapisują się do prefs.json.</summary>
public sealed class PrefsForm : Form
{
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

    private readonly TreeView _tree = new() { Dock = DockStyle.Left, Width = 190, HideSelection = false };
    private readonly Panel _pages = new() { Dock = DockStyle.Fill, Padding = new Padding(10) };
    private readonly Dictionary<string, Panel> _panels = new();

    private ComboBox _cbTheme = null!;
    private CheckBox _cbAutoNum = null!;
    private CheckBox _cbNatural = null!;
    private CheckBox _cbForceRefresh = null!;
    private CheckBox _cbMenuIcons = null!;
    private CheckBox _cbReopen = null!;
    private CheckBox _cbBackup = null!;
    private CheckBox _cbFullRow = null!;
    private CheckBox _cbGrid = null!;
    private ComboBox _cbTitleFmt = null!;
    private CheckBox _cbGroupCount = null!;
    private CheckBox _cbExtraCat = null!;
    private CheckBox _cbCover = null!;
    private ComboBox _cbHtml = null!;
    private ComboBox _cbCsv = null!;
    private TextBox _tbPatterns = null!;
    private NumericUpDown _numCover = null!;
    private NumericUpDown _numScriptTimeout = null!;

    private static readonly (string Key, string Name, string Value)[] TitleFormats =
    {
        ("Both", "Oryginalny / Polski", "Both"),
        ("Original", "Tytuł oryginalny", "Original"),
        ("Localized", "Tytuł polski", "Localized"),
    };

    private static readonly (string Key, string Name)[] ListKeys =
    {
        ("MediaType", "Typ nośnika"), ("Store", "Sklep / źródło"), ("Platform", "Platforma"),
        ("GameMode", "Tryb gry"), ("Genre", "Gatunek"), ("Country", "Kraj"),
        ("Developer", "Developer"), ("Publisher", "Wydawca"), ("Engine", "Silnik"),
    };

    private readonly Dictionary<string, List<string>> _customLists;
    private readonly List<SearchSite> _sites;
    private ComboBox _cbListKeys = null!;
    private ListBox _lbVals = null!;
    private TextBox _tbNewVal = null!;
    private ListBox _lbSites = null!;
    private TextBox _tbSiteName = null!;
    private TextBox _tbSiteUrl = null!;

    public PrefsForm(PrefsStore prefs)
    {
        Theme = prefs.Theme;
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

        Text = "Preferencje";
        Width = 660; Height = 470;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; MinimizeBox = false;
        try
        {
            using var strip = AmcIcons.LoadStrip();
            var bmp = (Bitmap)strip.Images[(int)AmcIcon.Options];
            Icon = (Icon)Icon.FromHandle(bmp.GetHicon()).Clone();
        }
        catch { }

        BuildTree();
        BuildPages();
        Controls.Add(_pages);
        Controls.Add(_tree);

        var bar = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 44, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(8) };
        var help = new Button { Text = "Pomoc", Width = 80 };
        help.Click += (_, _) => { using var h = new HelpForm(); h.ShowDialog(this); };
        var cancel = new Button { Text = "Anuluj", DialogResult = DialogResult.Cancel, Width = 80 };
        var ok = new Button { Text = "Zapisz", DialogResult = DialogResult.OK, Width = 80 };
        ok.Click += (_, _) => SaveToProps();
        bar.Controls.Add(help); bar.Controls.Add(cancel); bar.Controls.Add(ok);
        Controls.Add(bar);
        AcceptButton = ok; CancelButton = cancel;

        _tree.AfterSelect += (_, e) =>
        {
            if (e.Node?.Tag is string key && _panels.TryGetValue(key, out var p))
                foreach (var kv in _panels) kv.Value.Visible = kv.Key == key;
        };
        _tree.SelectedNode = _tree.Nodes[0];
    }

    private void SaveToProps()
    {
        Theme = _cbTheme.SelectedItem?.ToString() ?? Theme;
        AutoNumbering = _cbAutoNum.Checked;
        NaturalSort = _cbNatural.Checked;
        ForceFullRefresh = _cbForceRefresh.Checked;
        ShowMenuIcons = _cbMenuIcons.Checked;
        ReopenLastFile = _cbReopen.Checked;
        BackupOnSave = _cbBackup.Checked;
        FullRowSelect = _cbFullRow.Checked;
        ShowGridLines = _cbGrid.Checked;
        TitleFormat = _cbTitleFmt.SelectedItem is TitleFormatItem tf ? tf.Value : TitleFormat;
        ShowGroupCount = _cbGroupCount.Checked;
        ShowExtraCategory = _cbExtraCat.Checked;
        CoverVisible = _cbCover.Checked;
        HtmlTemplate = _cbHtml.SelectedItem?.ToString() ?? HtmlTemplate;
        CsvDelimiter = _cbCsv.SelectedItem is CsvItem ci ? ci.Value : CsvDelimiter;
        MediaPatterns = _tbPatterns.Text.Trim();
        if (string.IsNullOrWhiteSpace(MediaPatterns)) MediaPatterns = "*.exe";
        MaxCoverWidth = (int)_numCover.Value;
        ScriptTimeoutSec = Math.Max(5, (int)_numScriptTimeout.Value);
        CustomLists = _customLists.Where(kv => kv.Value.Count > 0)
            .ToDictionary(kv => kv.Key, kv => kv.Value.ToList());
        CustomSearchSites = _sites.Select(s => s with { }).ToList();
    }

    private sealed record TitleFormatItem(string Name, string Value)
    {
        public override string ToString() => Name;
    }

    private sealed record CsvItem(string Name, string Value)
    {
        public override string ToString() => Name;
    }

    private void AddPage(string key, string title)
    {
        // Sam panel — węzły drzewa buduje BuildTree (tytuł tylko dokumentacyjnie).
        var p = new Panel { Dock = DockStyle.Fill, Visible = false };
        _panels[key] = p;
        _pages.Controls.Add(p);
    }

    private static Label Section(Control parent, string text)
    {
        var l = new Label { Text = text, Dock = DockStyle.Top, Height = 24, Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold), Padding = new Padding(0, 6, 0, 0) };
        parent.Controls.Add(l);
        return l;
    }

    private static CheckBox Check(Control parent, string text, bool value)
    {
        var c = new CheckBox { Text = text, AutoSize = true, Dock = DockStyle.Top, Height = 24, Checked = value };
        parent.Controls.Add(c);
        return c;
    }

    private void BuildTree()
    {
        // Ikony węzłów z bieżącego motywu AMC.
        try
        {
            var strip = AmcIcons.LoadStrip(Theme);
            _tree.ImageList = strip;
        }
        catch { }

        TreeNode Node(string text, string key, AmcIcon icon)
        {
            var n = new TreeNode(text) { Tag = key };
            if (_tree.ImageList is not null && (int)icon < _tree.ImageList.Images.Count)
                n.ImageIndex = n.SelectedImageIndex = (int)icon;
            return n;
        }

        // Hierarchia jak w AMC: listy rozwijane/importy/szukaj pod „Film / Dodatkowe informacje”.
        _tree.Nodes.Add(Node("Opcje ogólne", "general", AmcIcon.Options));
        _tree.Nodes.Add(Node("Ikony Paska narzędzi", "icons", AmcIcon.GridMode));
        _tree.Nodes.Add(Node("Pliki", "files", AmcIcon.FileOpen));
        var list = Node("Lista gier", "list", AmcIcon.ViewList);
        list.Nodes.Add(Node("Formatowanie tytułu", "titlefmt", AmcIcon.FieldEdit));
        list.Nodes.Add(Node("Grupowanie", "grouping", AmcIcon.Group));
        _tree.Nodes.Add(list);
        _tree.Nodes.Add(Node("Lista dodatków", "extras", AmcIcon.MovieExtras));
        var movie = Node("Gra / Dodatkowe informacje", "movie", AmcIcon.MovieFind);
        movie.Nodes.Add(Node("Listy rozwijane", "dropdowns", AmcIcon.ViewDetail));
        movie.Nodes.Add(Node("Import plików multimedialnych", "importmedia", AmcIcon.MovieImportFiles));
        movie.Nodes.Add(Node("Import obrazków", "importpic", AmcIcon.PictureOpen));
        movie.Nodes.Add(Node("Szukaj w Internecie", "websearch", AmcIcon.MovieSearch));
        _tree.Nodes.Add(movie);
        _tree.Nodes.Add(Node("Skrypty", "scripts", AmcIcon.Scripting));
        _tree.Nodes.Add(Node("Eksport", "export", AmcIcon.FileExport));
        _tree.Nodes.Add(Node("Foldery", "folders", AmcIcon.Browse));
    }

    private void BuildPages()
    {
        // --- Opcje ogólne ---
        AddPage("general", "Opcje ogólne");
        var g = _panels["general"];
        Section(g, "Numerowanie");
        _cbAutoNum = Check(g, "Automatyczne numerowanie gier", AutoNumbering);
        g.Controls.Add(new Label { Text = "Gdy wyłączone, dodawanie gry pyta o numer („Wybór numeru gry” jak w AMC).", Dock = DockStyle.Top, Height = 30 });
        Section(g, "Opcje ogólne");
        _cbNatural = Check(g, "Użyj naturalnego porównania do sortowania tytułów (liczby po wartości)", NaturalSort);
        _cbForceRefresh = Check(g, "Wymuś odświeżanie całej listy gier po każdej zmianie wartości pola", ForceFullRefresh);

        // --- Ikony ---
        AddPage("icons", "Ikony Paska narzędzi");
        var ic = _panels["icons"];
        Section(ic, "Menu i Pasek narzędziowy");
        var t = new TableLayoutPanel { Dock = DockStyle.Top, Height = 60, ColumnCount = 2, AutoSize = true };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        t.Controls.Add(new Label { Text = "Zestaw ikon:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        _cbTheme = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        foreach (var th in AmcIcons.Themes) _cbTheme.Items.Add(th.Name);
        _cbTheme.SelectedItem = Theme;
        if (_cbTheme.SelectedIndex < 0) _cbTheme.SelectedIndex = 0;
        t.Controls.Add(_cbTheme, 1, 0);
        t.Controls.Add(new Label { Text = "Schemat kolorowania:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
        var cbScheme = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
        cbScheme.Items.AddRange(new object[] { "Automatyczny", "Jasny", "Ciemny" });
        cbScheme.SelectedIndex = 0;
        t.Controls.Add(cbScheme, 1, 1);
        ic.Controls.Add(t);
        _cbMenuIcons = Check(ic, "Ikony w menu", ShowMenuIcons);

        // --- Pliki ---
        AddPage("files", "Pliki");
        var fi = _panels["files"];
        Section(fi, "Katalog");
        _cbReopen = Check(fi, "Otwieraj ostatni katalog przy starcie", ReopenLastFile);
        _cbBackup = Check(fi, "Twórz kopię zapasową (.bak) przy zapisie", BackupOnSave);

        // --- Lista gier ---
        AddPage("list", "Lista gier");
        var li = _panels["list"];
        Section(li, "Wygląd listy");
        _cbFullRow = Check(li, "Zaznaczanie pełnych wierszy", FullRowSelect);
        _cbGrid = Check(li, "Linie siatki", ShowGridLines);

        // --- Formatowanie tytułu ---
        AddPage("titlefmt", "Formatowanie tytułu");
        var tf = _panels["titlefmt"];
        Section(tf, "Tytuł na liście gier");
        _cbTitleFmt = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList, Height = 24 };
        foreach (var f in TitleFormats) _cbTitleFmt.Items.Add(new TitleFormatItem(f.Name, f.Value));
        _cbTitleFmt.SelectedIndex = Math.Max(0, TitleFormats.ToList().FindIndex(f => f.Value == TitleFormat));
        tf.Controls.Add(_cbTitleFmt);

        // --- Grupowanie ---
        AddPage("grouping", "Grupowanie");
        var gr = _panels["grouping"];
        _cbGroupCount = Check(gr, "Pokazuj liczebność grup na liście", ShowGroupCount);

        // --- Lista dodatków ---
        AddPage("extras", "Lista dodatków");
        var ex2 = _panels["extras"];
        _cbExtraCat = Check(ex2, "Pokazuj kolumnę kategorii dodatków", ShowExtraCategory);

        // --- Gra / Dodatkowe informacje ---
        AddPage("movie", "Gra / Dodatkowe informacje");
        var mo = _panels["movie"];
        _cbCover = Check(mo, "Pokazuj panel okładki", CoverVisible);

        // --- Listy rozwijane ---
        AddPage("dropdowns", "Listy rozwijane");
        BuildListManager(_panels["dropdowns"]);

        // --- Import plików multimedialnych ---
        AddPage("importmedia", "Import plików multimedialnych");
        var im = _panels["importmedia"];
        Section(im, "Wzorce plików wykonywalnych");
        _tbPatterns = new TextBox { Dock = DockStyle.Top, Text = MediaPatterns };
        im.Controls.Add(_tbPatterns);
        im.Controls.Add(new Label { Text = "Rozdzielone średnikiem, np. *.exe;*.lnk", Dock = DockStyle.Top, Height = 22, ForeColor = SystemColors.GrayText });

        // --- Import obrazków ---
        AddPage("importpic", "Import obrazków");
        var ip = _panels["importpic"];
        Section(ip, "Wczytywanie okładek");
        var tip = new TableLayoutPanel { Dock = DockStyle.Top, Height = 28, ColumnCount = 2, AutoSize = true };
        tip.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        tip.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        tip.Controls.Add(new Label { Text = "Maks. szerokość okładki (px, 0 = bez zmian):", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        _numCover = new NumericUpDown { Maximum = 4000, Minimum = 0, Value = Math.Clamp(MaxCoverWidth, 0, 4000), Dock = DockStyle.Fill };
        tip.Controls.Add(_numCover, 1, 0);
        ip.Controls.Add(tip);

        // --- Szukaj w Internecie ---
        AddPage("websearch", "Szukaj w Internecie");
        BuildSiteManager(_panels["websearch"]);

        // --- Skrypty ---
        AddPage("scripts", "Skrypty");
        var sc = _panels["scripts"];
        Section(sc, "Wykonywanie");
        var tsc = new TableLayoutPanel { Dock = DockStyle.Top, Height = 28, ColumnCount = 2, AutoSize = true };
        tsc.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        tsc.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        tsc.Controls.Add(new Label { Text = "Limit czasu skryptu (s):", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        _numScriptTimeout = new NumericUpDown { Maximum = 600, Minimum = 5, Value = Math.Clamp(ScriptTimeoutSec, 5, 600), Dock = DockStyle.Fill };
        tsc.Controls.Add(_numScriptTimeout, 1, 0);
        sc.Controls.Add(tsc);

        // --- Eksport ---
        AddPage("export", "Eksport");
        var ex = _panels["export"];
        Section(ex, "Szablon HTML");
        _cbHtml = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList, Height = 24 };
        foreach (var n in HtmlTemplates.Names) _cbHtml.Items.Add(n);
        _cbHtml.SelectedItem = HtmlTemplate;
        if (_cbHtml.SelectedIndex < 0) _cbHtml.SelectedIndex = 0;
        ex.Controls.Add(_cbHtml);
        Section(ex, "Plik CSV");
        _cbCsv = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList, Height = 24 };
        _cbCsv.Items.Add(new CsvItem("Średnik (;)", ";"));
        _cbCsv.Items.Add(new CsvItem("Przecinek (,)", ","));
        _cbCsv.Items.Add(new CsvItem("Tabulator", "TAB"));
        _cbCsv.SelectedIndex = CsvDelimiter == "," ? 1 : CsvDelimiter == "TAB" ? 2 : 0;
        ex.Controls.Add(_cbCsv);

        // --- Foldery (informacyjnie) ---
        AddPage("folders", "Foldery");
        var fo = _panels["folders"];
        Section(fo, "Lokalizacje");
        fo.Controls.Add(new Label { Text = "Katalog danych:", Dock = DockStyle.Top, Height = 20 });
        fo.Controls.Add(new TextBox { Dock = DockStyle.Top, ReadOnly = true, Text = Path.GetDirectoryName(PrefsStore.PrefsPath()) ?? "" });
        fo.Controls.Add(new Label { Text = "Katalog skryptów:", Dock = DockStyle.Top, Height = 20 });
        fo.Controls.Add(new TextBox { Dock = DockStyle.Top, ReadOnly = true, Text = GameCatalog.Scripting.GameScript.ScriptsDir() });

        foreach (var p in _panels.Values) ControlLayout.FixDockOrder(p);
    }

    private void BuildListManager(Panel page)
    {
        Section(page, "Wartości list rozwijanych");
        _cbListKeys = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList, Height = 24 };
        foreach (var k in ListKeys) _cbListKeys.Items.Add(k.Name);
        _cbListKeys.SelectedIndex = 0;
        page.Controls.Add(_cbListKeys);
        _lbVals = new ListBox { Dock = DockStyle.Fill };
        page.Controls.Add(_lbVals);
        var bar = new Panel { Dock = DockStyle.Bottom, Height = 30 };
        _tbNewVal = new TextBox { Dock = DockStyle.Fill };
        bar.Controls.Add(_tbNewVal);
        var bDel = new Button { Text = "Usuń", Width = 70, Dock = DockStyle.Right };
        bDel.Click += (_, _) =>
        {
            if (_lbVals.SelectedItem is string s && _cbListKeys.SelectedIndex >= 0)
            {
                CurrentList().Remove(s);
                RefreshVals();
            }
        };
        bar.Controls.Add(bDel);
        var bAdd = new Button { Text = "Dodaj", Width = 70, Dock = DockStyle.Right };
        bAdd.Click += (_, _) =>
        {
            var v = _tbNewVal.Text.Trim();
            if (v.Length == 0 || _cbListKeys.SelectedIndex < 0) return;
            var list = CurrentList();
            if (!list.Contains(v)) list.Add(v);
            _tbNewVal.Clear();
            RefreshVals();
        };
        bar.Controls.Add(bAdd);
        page.Controls.Add(bar);
        _cbListKeys.SelectedIndexChanged += (_, _) => RefreshVals();
        RefreshVals();

        List<string> CurrentList()
        {
            string key = ListKeys[_cbListKeys.SelectedIndex].Key;
            if (!_customLists.TryGetValue(key, out var list))
            {
                list = new List<string>();
                _customLists[key] = list;
            }
            return list;
        }

        void RefreshVals()
        {
            _lbVals.Items.Clear();
            if (_cbListKeys.SelectedIndex < 0) return;
            foreach (var v in CurrentList()) _lbVals.Items.Add(v);
        }
    }

    private void BuildSiteManager(Panel page)
    {
        Section(page, "Wyszukiwarki (adres z {q} jako zapytaniem)");
        _lbSites = new ListBox { Dock = DockStyle.Fill };
        page.Controls.Add(_lbSites);
        var form = new TableLayoutPanel { Dock = DockStyle.Bottom, Height = 84, ColumnCount = 2 };
        form.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        form.Controls.Add(new Label { Text = "Nazwa:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        _tbSiteName = new TextBox { Dock = DockStyle.Fill };
        form.Controls.Add(_tbSiteName, 1, 0);
        form.Controls.Add(new Label { Text = "Adres URL:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
        _tbSiteUrl = new TextBox { Dock = DockStyle.Fill };
        form.Controls.Add(_tbSiteUrl, 1, 1);
        page.Controls.Add(form);
        var bar = new Panel { Dock = DockStyle.Bottom, Height = 30 };
        var bDef = new Button { Text = "Domyślne", Width = 80, Dock = DockStyle.Right };
        bDef.Click += (_, _) => { _sites.Clear(); _sites.AddRange(PrefsStore.DefaultSearchSites()); RefreshSites(); };
        bar.Controls.Add(bDef);
        var bDel = new Button { Text = "Usuń", Width = 70, Dock = DockStyle.Right };
        bDel.Click += (_, _) => { if (_lbSites.SelectedIndex >= 0) { _sites.RemoveAt(_lbSites.SelectedIndex); RefreshSites(); } };
        bar.Controls.Add(bDel);
        var bAdd = new Button { Text = "Dodaj", Width = 70, Dock = DockStyle.Right };
        bAdd.Click += (_, _) =>
        {
            var name = _tbSiteName.Text.Trim();
            var url = _tbSiteUrl.Text.Trim();
            if (name.Length == 0 || url.Length == 0) return;
            if (!url.Contains("{q}")) { MessageBox.Show(this, "Adres musi zawierać {q} jako miejsce na zapytanie.", "Wyszukiwarka", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            _sites.RemoveAll(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            _sites.Add(new SearchSite(name, url));
            _tbSiteName.Clear(); _tbSiteUrl.Clear();
            RefreshSites();
        };
        bar.Controls.Add(bAdd);
        page.Controls.Add(bar);
        RefreshSites();

        void RefreshSites()
        {
            _lbSites.Items.Clear();
            foreach (var s in _sites) _lbSites.Items.Add(s.Name);
        }
    }
}
