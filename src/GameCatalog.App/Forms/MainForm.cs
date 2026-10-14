using System.ComponentModel;
using System.Text;
using System.Text.Json;
using GameCatalog.Core.Catalog;
using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;
using GameCatalog.Core.Services;
using GameCatalog.Plugins.Abstractions;
using GameCatalog.Storage;

namespace GameCatalog.App;

/// <summary>
/// Główne okno — wierny port Ant Movie Catalog main.pas/main.dfm:
/// pasek menu + pasek narzędzi z oryginalnymi ikonami AMC, lista z grupami
/// (jak ElTree), okładka pod listą z mini-paskiem, edycja pól inline
/// (jak TMovieFrame), panel dodatków, pasek szukania i pasek statusu.
/// Pola filmowe zastąpiono growymi (Developer, Gatunek, Platforma, Silnik…).
/// </summary>
public sealed class MainForm : Form
{
    private const string ClipboardHeader = "GAMECATALOG1:";

    // Paleta etykiet koloru z AMC (movieclass.pas DefaultColorsTag, TColor = $BBGGRR)
    private static readonly int[] ColorTagValues =
        { 0xFFFFFF, 0x241CED, 0x277FFF, 0x00F2FF, 0x1DE6B5, 0x4CB122, 0xEEEE1C,
          0xFFB040, 0xFF80A0, 0xE060FF, 0xE00ED0, 0x5786B9, 0x999999 };

    private static Color ColorTagToColor(int tag)
    {
        if (tag <= 0 || tag >= ColorTagValues.Length) return Color.Empty;
        int c = ColorTagValues[tag];
        return Color.FromArgb(c & 0xFF, (c >> 8) & 0xFF, (c >> 16) & 0xFF);
    }

    private readonly GameList _model = new();
    private readonly List<LoanRecord> _loans = new();
    private SqliteGameCatalogStore _store = null!;
    private string _dbPath = "";
    private bool _dirty;
    private bool _refreshing;
    private readonly Stack<Action> _undo = new();

    // Stan widoku (jak opcje sort/group w AMC)
    private string _sortTag = "Title";
    private bool _ascending = true;
    private string _groupTag = "";
    private bool _thumbs;
    private List<(string Tag, int Width)> _columns = new()
    {
        ("Number", 40), ("Title", 210),
    };

    // Stan szukania (pasek Find jak ToolbarFind)
    private HashSet<int>? _findOnly;
    private string _findField = "Title";

    private ImageList _icons = null!;
    private PrefsStore _prefs = new();
    private readonly MenuStrip _menu = new();
    private readonly ToolStrip _tools = new();
    private readonly StatusStrip _status = new();
    private readonly ToolStripStatusLabel _stCount = new() { Width = 300, TextAlign = ContentAlignment.MiddleLeft };
    private readonly ToolStripStatusLabel _stSelected = new() { Width = 350, TextAlign = ContentAlignment.MiddleLeft };
    private readonly ToolStripStatusLabel _stHint = new() { Spring = true, TextAlign = ContentAlignment.MiddleLeft };

    private readonly Panel _panelLeft = new() { Dock = DockStyle.Left, Width = 300 };
    private readonly ListView _list = new() { Dock = DockStyle.Fill };
    private readonly Panel _picPanel = new() { Dock = DockStyle.Bottom, Height = 210 };
    private readonly Splitter _picSplitter = new() { Dock = DockStyle.Bottom, Height = 3 };
    private readonly ToolStrip _picTools = new();
    private readonly PictureBox _picture = new() { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle };

    private readonly Panel _panelExtras = new() { Dock = DockStyle.Right, Width = 230, Visible = false };
    private readonly ToolStrip _extrasTools = new();
    private readonly ListView _extrasList = new() { Dock = DockStyle.Fill };
    private readonly Label _extrasTitle = new() { Dock = DockStyle.Top, Height = 22, TextAlign = ContentAlignment.MiddleLeft, Text = "Dodatki" };

    private readonly Panel _panelCenter = new() { Dock = DockStyle.Fill };
    private readonly TabControl _tabs = new() { Dock = DockStyle.Fill };
    private readonly GameFieldsControl _fields = new();
    private readonly Panel _customScroll = new() { Dock = DockStyle.Fill, AutoScroll = true, BackColor = SystemColors.Window, Padding = new Padding(8) };
    private TabPage _pageFields = null!, _pageCustom = null!, _pageAll = null!, _pageExtras = null!, _pageHtml = null!;
    private readonly DataGridView _allFieldsGrid = new() { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
    private readonly WebBrowser _htmlView = new() { Dock = DockStyle.Fill };
    private readonly ToolStrip _htmlTools = new();

    private readonly Panel _findBar = new() { Dock = DockStyle.Bottom, Height = 58, Visible = false };
    private readonly ComboBox _findFieldBox = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
    private readonly TextBox _findText = new() { Width = 400 };
    private readonly CheckBox _findWhole = new() { Text = "Całe pole", AutoSize = true };
    private readonly CheckBox _findShow = new() { Text = "Pokaż wyniki", AutoSize = true };
    private readonly CheckBox _findReverse = new() { Text = "Odwróć wyniki", AutoSize = true };

    private Dictionary<string, Control> _editors => _fields.Editors;
    private readonly Dictionary<string, Control> _sectionBodies = new();
    private Game? _current;

    private ToolStripMenuItem _mPicShow = null!, _mExtrasShow = null!, _mThumbs = null!,
        _mSortAsc = null!, _mSortDesc = null!;
    private ToolStripMenuItem? _mWeb;
    private ToolStripMenuItem _mFlatTable = null!, _mShowHtml = null!, _mGroupCount = null!;
    private ToolStripMenuItem _mruMenu = null!, _fieldsMenu = null!, _htmlMenu = null!;
    private ToolStripDropDownButton _tbColumns = null!;
    private readonly Dictionary<string, ToolStripMenuItem> _sortMenuItems = new();
    private readonly Dictionary<string, ToolStripMenuItem> _groupMenuItems = new();
    private ToolStripDropDownButton _tbGroup = null!;

    private static Bitmap? _appIconBitmap;

    public MainForm()
    {
        Text = "GameCatalog";
        Width = 1180; Height = 740;
        StartPosition = FormStartPosition.CenterScreen;

        _prefs = PrefsStore.Load();
        _icons = AmcIcons.LoadStrip(_prefs.Theme);
        try
        {
            _appIconBitmap?.Dispose();
            _appIconBitmap = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(_appIconBitmap))
                g.DrawImage(_icons.Images[(int)AmcIcon.GridMode], 0, 0);
            Icon = Icon.FromHandle(_appIconBitmap.GetHicon());
        }
        catch { /* brak ikony okna — nie krytyczne */ }

        ApplyIconLists();

        BuildMenu();
        BuildToolbar();
        BuildLeft();
        BuildCenter();
        BuildExtras();
        BuildFindBar();

        _status.Items.AddRange(new ToolStripItem[] { _stCount, _stSelected, _stHint });
        var splitL = new Splitter { Dock = DockStyle.Left, Width = 3 };
        Controls.Add(_panelCenter);
        Controls.Add(splitL);
        Controls.Add(_panelLeft);
        Controls.Add(_findBar);
        Controls.Add(_tools);
        Controls.Add(_menu);
        Controls.Add(_status);
        MainMenuStrip = _menu;

        _dbPath = DefaultDbPath();
        if (_prefs.ReopenLastFile && _prefs.RecentFiles.FirstOrDefault() is string last && File.Exists(last))
            _dbPath = last;

        // Preferencje wyglądu/zachowania (jak opcje AMC).
        _list.FullRowSelect = _prefs.FullRowSelect;
        _list.GridLines = _prefs.ShowGridLines;
        _picPanel.Visible = _picSplitter.Visible = _prefs.CoverVisible;
        _mPicShow.Checked = _prefs.CoverVisible;
        GameCatalog.Scripting.ScriptRunner.Timeout = TimeSpan.FromSeconds(Math.Max(5, _prefs.ScriptTimeoutSec));
        RebuildWebMenu();
        ApplyCustomLists();
        OpenDatabase(_dbPath);
    }

    private static string DefaultDbPath()
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GameCatalog");
        Directory.CreateDirectory(dir);
        var gc = Path.Combine(dir, "catalog.gc");
        // Migracja ze starego rozszerzenia .db
        var legacy = Path.Combine(dir, "catalog.db");
        if (!File.Exists(gc) && File.Exists(legacy))
        {
            try { File.Copy(legacy, gc); }
            catch { /* przy błędzie otwarty zostanie stary plik */ }
        }
        return gc;
    }

    private void ApplyIconLists()
    {
        _tools.ImageList = _icons;
        _menu.ImageList = _prefs.ShowMenuIcons ? _icons : null;
        _picTools.ImageList = _icons;
        _extrasTools.ImageList = _icons;
        _htmlTools.ImageList = _icons;
        AssignIconLists(_menu.Items);
        AssignIconLists(_tools.Items);
    }

    private void ApplyTheme()
    {
        var old = _icons;
        _icons = AmcIcons.LoadStrip(_prefs.Theme);
        ApplyIconLists();
        old.Dispose();
        _list.LargeImageList = null;
        RefreshList();
    }

    private static ToolStripMenuItem MI(string text, EventHandler onClick, AmcIcon? icon = null, Keys shortcut = Keys.None)
    {
        var m = new ToolStripMenuItem(text);
        if (icon.HasValue) m.ImageIndex = (int)icon.Value;
        if (shortcut != Keys.None) m.ShortcutKeys = shortcut;
        m.Click += onClick;
        return m;
    }

    private ToolStripButton TB(string text, AmcIcon icon, EventHandler onClick, string? tip = null)
    {
        var b = new ToolStripButton
        {
            ImageIndex = (int)icon,
            DisplayStyle = ToolStripItemDisplayStyle.Image,
            ToolTipText = tip ?? text,
            Text = text,
        };
        b.Click += onClick;
        return b;
    }

    private ToolStripButton TBtn(AmcIcon icon, EventHandler onClick, string tip)
    {
        var b = new ToolStripButton
        {
            ImageIndex = (int)icon,
            DisplayStyle = ToolStripItemDisplayStyle.Image,
            ToolTipText = tip,
        };
        b.Click += onClick;
        return b;
    }

    private void AssignIconLists(ToolStripItemCollection items)
    {
        foreach (ToolStripItem item in items)
        {
            if (item is ToolStripMenuItem mi && mi.HasDropDownItems)
            {
                mi.DropDown.ImageList = _icons;
                AssignIconLists(mi.DropDownItems);
            }
            else if (item is ToolStripDropDownButton dd)
            {
                dd.DropDown.ImageList = _icons;
                AssignIconLists(dd.DropDownItems);
            }
        }
    }

    // ================= MENU (jak ToolbarMenu: Plik/Gra/Widok/Narzędzia/Pomoc) =================
    private void BuildMenu()
    {
        var mFile = new ToolStripMenuItem("&Plik");
        mFile.DropDownItems.Add(MI("&Nowy", (_, _) => NewCatalog(), AmcIcon.FileNew, Keys.Control | Keys.N));
        mFile.DropDownItems.Add(MI("&Otwórz…", (_, _) => OpenDatabaseDialog(), AmcIcon.FileOpen, Keys.Control | Keys.O));
        mFile.DropDownItems.Add(MI("&Zapisz", (_, _) => Save(), AmcIcon.FileSave, Keys.Control | Keys.S));
        mFile.DropDownItems.Add(MI("Zapisz &jako…", (_, _) => SaveAs(), AmcIcon.FileSaveAs, Keys.Control | Keys.Shift | Keys.S));
        mFile.DropDownItems.Add(new ToolStripSeparator());
        mFile.DropDownItems.Add(MI("&Drukuj…", (_, _) => PrintCurrent(), AmcIcon.FilePrint, Keys.Control | Keys.P));
        mFile.DropDownItems.Add(MI("&Importuj…", (_, _) => ImportDialog(), AmcIcon.FileImport, Keys.Control | Keys.I));
        mFile.DropDownItems.Add(MI("&Eksportuj…", (_, _) => ExportDialog(), AmcIcon.FileExport, Keys.Control | Keys.E));
        mFile.DropDownItems.Add(MI("&Właściwości…", (_, _) => ShowProperties(), AmcIcon.FileProperties, Keys.Alt | Keys.Enter));
        mFile.DropDownItems.Add(new ToolStripSeparator());
        _mruMenu = new ToolStripMenuItem("Ostatnie pliki");
        mFile.DropDownItems.Add(_mruMenu);
        mFile.DropDownItems.Add(new ToolStripSeparator());
        mFile.DropDownItems.Add(MI("Za&mnij", (_, _) => Close(), AmcIcon.Exit, Keys.Alt | Keys.F4));

        var mGame = new ToolStripMenuItem("&Gra");
        mGame.DropDownItems.Add(MI("&Dodaj…", (_, _) => AddGame(), AmcIcon.MovieAdd, Keys.Alt | Keys.Insert));
        mGame.DropDownItems.Add(MI("&Usuń", (_, _) => DeleteGame(), AmcIcon.MovieDelete, Keys.Alt | Keys.Delete));
        mGame.DropDownItems.Add(MI("&Znajdź…", (_, _) => ToggleFindBar(), AmcIcon.MovieFind, Keys.Control | Keys.F));
        mGame.DropDownItems.Add(MI("&Losowy", (_, _) => RandomGame(), AmcIcon.Random, Keys.F8));
        mGame.DropDownItems.Add(new ToolStripSeparator());
        mGame.DropDownItems.Add(MI("Zmień &numer…", (_, _) => ChangeNumber(), AmcIcon.MovieNumber, Keys.F2));
        var mPic = new ToolStripMenuItem("&Obrazki") { ImageIndex = (int)AmcIcon.MoviePicture };
        _mPicShow = MI("Po&każ panel obrazków", (_, _) => TogglePicturePanel(), AmcIcon.MoviePicture);
        _mPicShow.CheckOnClick = true; _mPicShow.Checked = true;
        mPic.DropDownItems.Add(_mPicShow);
        mPic.DropDownItems.Add(new ToolStripSeparator());
        mPic.DropDownItems.Add(MI("&Wczytaj…", (_, _) => PictureLoad(), AmcIcon.PictureOpen));
        mPic.DropDownItems.Add(MI("Zapisz &jako…", (_, _) => PictureSaveAs(), AmcIcon.PictureSave));
        mPic.DropDownItems.Add(MI("&Kopiuj", (_, _) => PictureCopy(), AmcIcon.PictureCopy));
        mPic.DropDownItems.Add(MI("W&klej", (_, _) => PicturePaste(), AmcIcon.MoviePaste));
        mPic.DropDownItems.Add(MI("&Usuń", (_, _) => PictureDelete(), AmcIcon.PictureDelete));
        mGame.DropDownItems.Add(mPic);
        _mExtrasShow = MI("&Dodatki", (_, _) => ShowExtrasTab(), AmcIcon.MovieExtras,
            Keys.Control | Keys.Shift | Keys.F12);
        mGame.DropDownItems.Add(_mExtrasShow);
        var mUrl = new ToolStripMenuItem("&URL") { ImageIndex = (int)AmcIcon.MovieUrl };
        mUrl.DropDownItems.Add(MI("&Otwórz", (_, _) => OpenUrl(), AmcIcon.MovieUrl));
        mUrl.DropDownItems.Add(MI("&Kopiuj", (_, _) => CopyUrl(), AmcIcon.MovieCopy));
        mGame.DropDownItems.Add(mUrl);
        var mPath = new ToolStripMenuItem("Ście&żka pliku") { ImageIndex = (int)AmcIcon.Browse };
        mPath.DropDownItems.Add(MI("Otwórz w &eksploratorze", (_, _) => OpenPath(), AmcIcon.Browse));
        mPath.DropDownItems.Add(MI("&Kopiuj", (_, _) => CopyPath(), AmcIcon.MovieCopy));
        mGame.DropDownItems.Add(mPath);
        var mGet = new ToolStripMenuItem("Pobierz &informacje") { ImageIndex = (int)AmcIcon.MovieImport };
        mGet.DropDownItems.Add(MI("Z &plików…", (_, _) => ImportFolder(), AmcIcon.MovieImportFiles));
        mGet.DropDownItems.Add(MI("Ze &skryptu…", (_, _) => RunScriptDialog(), AmcIcon.MovieImportScript, Keys.F6));
        mGame.DropDownItems.Add(mGet);
        var mWeb = new ToolStripMenuItem("Szukaj w &Internecie") { ImageIndex = (int)AmcIcon.MovieSearch };
        _mWeb = mWeb;
        mGame.DropDownItems.Add(mWeb);
        mGame.DropDownItems.Add(new ToolStripSeparator());
        mGame.DropDownItems.Add(MI("&Kopiuj", (_, _) => CopyGame(), AmcIcon.MovieCopy, Keys.Control | Keys.Shift | Keys.C));
        mGame.DropDownItems.Add(MI("W&klej", (_, _) => PasteGame(), AmcIcon.MoviePaste, Keys.Control | Keys.Shift | Keys.V));
        mGame.DropDownItems.Add(MI("C&ofnij zmiany", (_, _) => Undo(), AmcIcon.MovieUndo, Keys.Control | Keys.Shift | Keys.Z));

        var mView = new ToolStripMenuItem("&Wyświetlaj");
        var mSort = new ToolStripMenuItem("Sortuj &wg") { ImageIndex = (int)AmcIcon.SortAscend };
        foreach (var tag in new[] { "Number", "Title", "Genre", "Platform", "Developer", "Publisher", "ReleaseYear", "UserRating", "CriticRating", "PlaytimeHours", "CompletionStatus", "DateAdded", "LastPlayed", "Store" })
        {
            var item = new ToolStripMenuItem(CatalogLabels.Current.Field(tag)) { CheckOnClick = false, Tag = tag };
            item.Click += (_, _) => SetSort((string)item.Tag!);
            _sortMenuItems[tag] = item;
            mSort.DropDownItems.Add(item);
        }
        mSort.DropDownItems.Add(new ToolStripSeparator());
        _mSortAsc = MI("Rosnąc&o", (_, _) => SetOrder(true), AmcIcon.SortAscend);
        _mSortDesc = MI("Malejąc&o", (_, _) => SetOrder(false), AmcIcon.SortDescend);
        mSort.DropDownItems.Add(_mSortAsc);
        mSort.DropDownItems.Add(_mSortDesc);
        mView.DropDownItems.Add(mSort);
        var mGroup = new ToolStripMenuItem("&Grupuj wg") { ImageIndex = (int)AmcIcon.Group };
        var gNone = new ToolStripMenuItem("(brak)") { Tag = "" };
        gNone.Click += (_, _) => SetGroup("");
        _groupMenuItems[""] = gNone;
        mGroup.DropDownItems.Add(gNone);
        mGroup.DropDownItems.Add(new ToolStripSeparator());
        foreach (var tag in GameFields.GroupableTags)
        {
            var item = new ToolStripMenuItem(CatalogLabels.Current.Field(tag)) { Tag = tag };
            item.Click += (_, _) => SetGroup((string)item.Tag!);
            _groupMenuItems[tag] = item;
            mGroup.DropDownItems.Add(item);
        }
        mView.DropDownItems.Add(mGroup);
        var mGpf = new ToolStripMenuItem("&Formatowanie grup") { ImageIndex = (int)AmcIcon.GroupsFormat };
        _mGroupCount = MI("Pokaż &liczbę gier", (_, _) => ToggleGroupCount(), AmcIcon.GroupsFormat);
        _mGroupCount.CheckOnClick = true;
        _mGroupCount.Checked = _prefs.ShowGroupCount;
        mGpf.DropDownItems.Add(_mGroupCount);
        mView.DropDownItems.Add(mGpf);
        mView.DropDownItems.Add(MI("Rozciągnij &listę", (_, _) => StretchList(), AmcIcon.StretchList, Keys.F11));
        _mFlatTable = MI("Pokaż w &formie tabeli", (_, _) => ShowFlatTable(), AmcIcon.GridMode, Keys.Shift | Keys.F4);
        mView.DropDownItems.Add(_mFlatTable);
        _fieldsMenu = new ToolStripMenuItem("Pola &tabeli") { ImageIndex = (int)AmcIcon.Field };
        _fieldsMenu.DropDownOpening += (_, _) => RebuildFieldsMenu();
        mView.DropDownItems.Add(_fieldsMenu);
        _mThumbs = MI("Pokaż jako &miniatury", (_, _) => ToggleThumbs(), AmcIcon.ViewThumbnails, Keys.F4);
        _mThumbs.CheckOnClick = true;
        mView.DropDownItems.Add(_mThumbs);
        _mShowHtml = MI("Pokaż &HTML", (_, _) => ShowHtmlTab(), AmcIcon.Html, Keys.F12);
        mView.DropDownItems.Add(_mShowHtml);
        _htmlMenu = new ToolStripMenuItem("&Wybierz HTML") { ImageIndex = (int)AmcIcon.Html };
        _htmlMenu.DropDownOpening += (_, _) => RebuildHtmlMenu();
        mView.DropDownItems.Add(_htmlMenu);

        var mTools = new ToolStripMenuItem("Na&rzędzia");
        mTools.DropDownItems.Add(MI("&Zarządzanie polami…", (_, _) => ManageFields(), AmcIcon.FieldEdit));
        mTools.DropDownItems.Add(MI("Zarządzanie &obrazkami…", (_, _) => ManagePictures(), AmcIcon.MoviePicture));
        mTools.DropDownItems.Add(MI("&Skrypty…", (_, _) => OpenScriptsManager(), AmcIcon.Scripting, Keys.Shift | Keys.F6));
        mTools.DropDownItems.Add(new ToolStripSeparator());
        mTools.DropDownItems.Add(MI("Zmień &numerację gier…", (_, _) => Renumber(), AmcIcon.Renumber));
        mTools.DropDownItems.Add(MI("&Pożyczone…", (_, _) => ShowLoans(), AmcIcon.Loans, Keys.F9));
        mTools.DropDownItems.Add(MI("&Statystyki…", (_, _) => ShowStats(), AmcIcon.Statistics));
        mTools.DropDownItems.Add(MI("&Edytor HTML…", (_, _) => EditHtml(), AmcIcon.Html, Keys.Control | Keys.F12));
        mTools.DropDownItems.Add(new ToolStripSeparator());
        mTools.DropDownItems.Add(MI("&Preferencje…", (_, _) => EditPrefs(), AmcIcon.Options, Keys.F10));

        var mHelp = new ToolStripMenuItem("P&omoc");
        mHelp.DropDownItems.Add(MI("&Indeks pomocy", (_, _) => new HelpForm().ShowDialog(this), AmcIcon.Help, Keys.F1));
        mHelp.DropDownItems.Add(MI("&O programie…", (_, _) => new AboutForm().ShowDialog(this), AmcIcon.About));
        mHelp.DropDownItems.Add(MI("&Wersje…", (_, _) => new VersionsForm().ShowDialog(this), AmcIcon.Help));

        _menu.Items.AddRange(new ToolStripItem[] { mFile, mGame, mView, mTools, mHelp });
        AssignIconLists(_menu.Items);
        RefreshSortGroupChecks();
    }

    // ================= TOOLBAR (jak ToolbarMain) =================
    private void BuildToolbar()
    {
        _tools.Items.Add(TB("Nowy", AmcIcon.FileNew, (_, _) => NewCatalog(), "Nowy katalog"));
        _tools.Items.Add(TB("Otwórz", AmcIcon.FileOpen, (_, _) => OpenDatabaseDialog(), "Otwórz bazę"));
        _tools.Items.Add(TB("Zapisz", AmcIcon.FileSave, (_, _) => Save(), "Zapisz"));
        _tools.Items.Add(TB("Zapisz jako", AmcIcon.FileSaveAs, (_, _) => SaveAs(), "Zapisz jako"));
        _tools.Items.Add(TB("Drukuj", AmcIcon.FilePrint, (_, _) => PrintCurrent(), "Drukuj kartę gry"));
        var ddImp = new ToolStripDropDownButton("Import") { ImageIndex = (int)AmcIcon.FileImport, DisplayStyle = ToolStripItemDisplayStyle.Image, ToolTipText = "Import" };
        ddImp.DropDownItems.Add(MI("Z folderu…", (_, _) => ImportFolder(), AmcIcon.MovieImportFiles));
        ddImp.DropDownItems.Add(MI("Z CSV…", (_, _) => ImportCsv(), AmcIcon.FileImport));
        ddImp.DropDownItems.Add(MI("Z AMC XML…", (_, _) => ImportAmc(), AmcIcon.MovieImport));
        _tools.Items.Add(ddImp);
        var ddExp = new ToolStripDropDownButton("Eksport") { ImageIndex = (int)AmcIcon.FileExport, DisplayStyle = ToolStripItemDisplayStyle.Image, ToolTipText = "Eksport" };
        ddExp.DropDownItems.Add(MI("Do CSV…", (_, _) => ExportCsv(), AmcIcon.FileExport));
        ddExp.DropDownItems.Add(MI("Do HTML…", (_, _) => ExportHtml(), AmcIcon.Html));
        _tools.Items.Add(ddExp);
        _tools.Items.Add(TB("Właściwości", AmcIcon.FileProperties, (_, _) => ShowProperties(), "Właściwości katalogu"));
        _tools.Items.Add(new ToolStripSeparator());
        _tools.Items.Add(TB("Dodaj", AmcIcon.MovieAdd, (_, _) => AddGame(), "Dodaj grę"));
        _tools.Items.Add(TB("Numer", AmcIcon.MovieNumber, (_, _) => GoToNumber(), "Idź do numeru"));
        _tools.Items.Add(TB("Usuń", AmcIcon.MovieDelete, (_, _) => DeleteGame(), "Usuń grę"));
        _tools.Items.Add(TB("Znajdź", AmcIcon.MovieFind, (_, _) => ToggleFindBar(), "Znajdź"));
        _tools.Items.Add(TB("Losuj", AmcIcon.Random, (_, _) => RandomGame(), "Losowa gra"));
        var ddPic = new ToolStripDropDownButton("Okładka") { ImageIndex = (int)AmcIcon.MoviePicture, DisplayStyle = ToolStripItemDisplayStyle.Image, ToolTipText = "Okładka" };
        ddPic.DropDownItems.Add(MI("Wczytaj…", (_, _) => PictureLoad(), AmcIcon.PictureOpen));
        ddPic.DropDownItems.Add(MI("Zapisz jako…", (_, _) => PictureSaveAs(), AmcIcon.PictureSave));
        ddPic.DropDownItems.Add(MI("Kopiuj", (_, _) => PictureCopy(), AmcIcon.PictureCopy));
        ddPic.DropDownItems.Add(MI("Wklej", (_, _) => PicturePaste(), AmcIcon.MoviePaste));
        ddPic.DropDownItems.Add(MI("Usuń", (_, _) => PictureDelete(), AmcIcon.PictureDelete));
        _tools.Items.Add(ddPic);
        _tools.Items.Add(TB("Dodatki", AmcIcon.MovieExtras, (_, _) => ShowExtrasTab(), "Dodatki"));
        var ddUrl = new ToolStripDropDownButton("URL") { ImageIndex = (int)AmcIcon.MovieUrl, DisplayStyle = ToolStripItemDisplayStyle.Image, ToolTipText = "URL" };
        ddUrl.DropDownItems.Add(MI("Otwórz", (_, _) => OpenUrl(), AmcIcon.MovieUrl));
        ddUrl.DropDownItems.Add(MI("Kopiuj", (_, _) => CopyUrl(), AmcIcon.MovieCopy));
        _tools.Items.Add(ddUrl);
        _tools.Items.Add(new ToolStripSeparator());
        _tools.Items.Add(TB("Kopiuj", AmcIcon.MovieCopy, (_, _) => CopyGame(), "Kopiuj grę"));
        _tools.Items.Add(TB("Wklej", AmcIcon.MoviePaste, (_, _) => PasteGame(), "Wklej grę"));
        _tools.Items.Add(TB("Cofnij", AmcIcon.MovieUndo, (_, _) => Undo(), "Cofnij"));
        _tools.Items.Add(new ToolStripSeparator());
        _tools.Items.Add(TB("Sortuj rosnąco", AmcIcon.SortAscend, (_, _) => SetOrder(true), "Sortuj rosnąco"));
        _tools.Items.Add(TB("Sortuj malejąco", AmcIcon.SortDescend, (_, _) => SetOrder(false), "Sortuj malejąco"));
        _tbGroup = new ToolStripDropDownButton("Grupuj") { ImageIndex = (int)AmcIcon.Group, DisplayStyle = ToolStripItemDisplayStyle.Image, ToolTipText = "Grupuj wg" };
        RefreshGroupMenu();
        _tools.Items.Add(_tbGroup);
        _tools.Items.Add(TB("Rozciągnij", AmcIcon.StretchList, (_, _) => StretchList(), "Rozciągnij listę"));
        _tbColumns = new ToolStripDropDownButton("Pola tabeli") { ImageIndex = (int)AmcIcon.Field, DisplayStyle = ToolStripItemDisplayStyle.Image, ToolTipText = "Pola tabeli" };
        _tbColumns.DropDownOpening += (_, _) => RebuildColumnsMenu(_tbColumns.DropDownItems);
        _tools.Items.Add(_tbColumns);
        _tools.Items.Add(TB("Miniatury", AmcIcon.ViewThumbnails, (_, _) => ToggleThumbs(), "Pokaż jako miniatury"));
        _tools.Items.Add(TB("HTML", AmcIcon.Html, (_, _) => ShowHtmlTab(), "Podgląd HTML"));
        _tools.Items.Add(new ToolStripSeparator());
        _tools.Items.Add(TB("Pożyczone", AmcIcon.Loans, (_, _) => ShowLoans(), "Pożyczone"));
        _tools.Items.Add(TB("Statystyki", AmcIcon.Statistics, (_, _) => ShowStats(), "Statystyki"));
        _tools.Items.Add(TB("Przenumeruj", AmcIcon.Renumber, (_, _) => Renumber(), "Zmień numerację gier"));
        _tools.Items.Add(TB("Skrypty", AmcIcon.Scripting, (_, _) => OpenScriptsManager(), "Skrypty"));
        _tools.Items.Add(TB("Edytor HTML", AmcIcon.Html, (_, _) => EditHtml(), "Edytor HTML"));
        _tools.Items.Add(TB("Preferencje", AmcIcon.Options, (_, _) => EditPrefs(), "Preferencje"));
        _tools.Items.Add(new ToolStripSeparator());
        _tools.Items.Add(TB("O programie", AmcIcon.About, (_, _) => new AboutForm().ShowDialog(this), "O programie"));
        AssignIconLists(_tools.Items);
    }

    private void RefreshGroupMenu()
    {
        _tbGroup.DropDownItems.Clear();
        var none = MI("(brak)", (_, _) => SetGroup(""), AmcIcon.Group);
        none.Checked = _groupTag == "";
        _tbGroup.DropDownItems.Add(none);
        _tbGroup.DropDownItems.Add(new ToolStripSeparator());
        foreach (var tag in GameFields.GroupableTags)
        {
            string t = tag;
            var item = MI(CatalogLabels.Current.Field(t), (_, _) => SetGroup(t), AmcIcon.Group);
            item.Checked = _groupTag == t;
            _tbGroup.DropDownItems.Add(item);
        }
    }

    private void RefreshSortGroupChecks()
    {
        foreach (var kv in _sortMenuItems) kv.Value.Checked = kv.Key == _sortTag;
        foreach (var kv in _groupMenuItems) kv.Value.Checked = kv.Key == _groupTag;
        _mSortAsc.Checked = _ascending;
        _mSortDesc.Checked = !_ascending;
        _mFlatTable.Checked = _groupTag == "" && !_thumbs;
        _mShowHtml.Checked = _tabs.SelectedTab == _pageHtml;
        _mThumbs.Checked = _thumbs;
    }

    // ================= LEWA STRONA: lista + okładka (jak PanelLeft) =================
    private void BuildLeft()
    {
        _list.View = View.Details;
        _list.FullRowSelect = true;
        _list.MultiSelect = false;
        _list.CheckBoxes = true;
        _list.HideSelection = false;
        _list.ShowGroups = true;
        _list.ItemChecked += (_, e) =>
        {
            if (_refreshing || e.Item.Tag is not int n) return;
            var g = _model.Find(n);
            if (g is not null && g.Checked != e.Item.Checked) { g.Checked = e.Item.Checked; _dirty = true; }
        };
        _list.SelectedIndexChanged += (_, _) => SelectFromList();
        _list.ColumnClick += (_, e) =>
        {
            if (e.Column < _columns.Count)
            {
                var tag = _columns[e.Column].Tag;
                if (tag == _sortTag) SetOrder(!_ascending);
                else { _sortTag = tag; _ascending = true; RefreshSortGroupChecks(); RefreshList(); }
            }
        };
        _list.DoubleClick += (_, _) => _fields.FocusTitle();
        _list.ContextMenuStrip = BuildListPopup();
        _panelLeft.Controls.Add(_list);

        var splitter = _picSplitter;
        _panelLeft.Controls.Add(splitter);

        _picPanel.Controls.Add(_picture);
        _picTools.Items.Add(TBtn(AmcIcon.PictureOpen, (_, _) => PictureLoad(), "Wczytaj okładkę"));
        _picTools.Items.Add(TBtn(AmcIcon.PictureSave, (_, _) => PictureSaveAs(), "Zapisz okładkę jako…"));
        _picTools.Items.Add(TBtn(AmcIcon.PictureCopy, (_, _) => PictureCopy(), "Kopiuj okładkę"));
        _picTools.Items.Add(TBtn(AmcIcon.MoviePaste, (_, _) => PicturePaste(), "Wklej okładkę"));
        _picTools.Items.Add(TBtn(AmcIcon.PictureDelete, (_, _) => PictureDelete(), "Usuń okładkę"));
        var picBar = new Panel { Dock = DockStyle.Top, Height = 25 };
        _picTools.Dock = DockStyle.Fill;
        picBar.Controls.Add(_picTools);
        _picPanel.Controls.Add(picBar);
        _panelLeft.Controls.Add(_picPanel);
        RebuildColumns();
    }

    private ContextMenuStrip BuildListPopup()
    {
        var p = new ContextMenuStrip { ImageList = _icons };
        p.Items.Add(MI("Dodaj", (_, _) => AddGame(), AmcIcon.MovieAdd));
        p.Items.Add(MI("Usuń", (_, _) => DeleteGame(), AmcIcon.MovieDelete));
        p.Items.Add(MI("Numer…", (_, _) => GoToNumber(), AmcIcon.MovieNumber));
        p.Items.Add(new ToolStripSeparator());
        p.Items.Add(MI("Kopiuj", (_, _) => CopyGame(), AmcIcon.MovieCopy));
        p.Items.Add(MI("Wklej", (_, _) => PasteGame(), AmcIcon.MoviePaste));
        p.Items.Add(new ToolStripSeparator());
        p.Items.Add(MI("Zaznacz wszystkie", (_, _) => SetAllChecked(true), AmcIcon.MovieFind));
        p.Items.Add(MI("Odznacz wszystkie", (_, _) => SetAllChecked(false), AmcIcon.MovieFind));
        var mCol = new ToolStripMenuItem("Etykieta koloru");
        for (int i = 0; i < ColorTagValues.Length; i++)
        {
            int idx = i;
            var item = new ToolStripMenuItem(idx == 0 ? "(brak)" : $"Kolor {idx}");
            var c = ColorTagToColor(idx);
            if (idx != 0) { item.Image = ColorSwatch(c); }
            item.Click += (_, _) => SetColorTag(idx);
            mCol.DropDownItems.Add(item);
        }
        p.Items.Add(mCol);
        return p;
    }

    private static Bitmap ColorSwatch(Color c)
    {
        var b = new Bitmap(16, 16);
        using var g = Graphics.FromImage(b);
        g.Clear(c);
        g.DrawRectangle(Pens.Black, 0, 0, 15, 15);
        return b;
    }

    private void RebuildColumns()
    {
        _list.Columns.Clear();
        foreach (var (tag, width) in _columns)
            _list.Columns.Add(tag == "Title" ? "Tytuł" : CatalogLabels.Current.Field(tag), width);
    }

    // ================= ŚRODEK: zakładki jak w AMC (Pola gry / Pola niestandardowe / Wszystkie pola / Dodatki + HTML) =================
    private void BuildCenter()
    {
        _pageFields = new TabPage("Pola gry");
        _pageFields.Controls.Add(_fields);
        _fields.FieldCommitted += CommitField;
        _fields.OpenUrlRequested = OpenUrl;
        _fields.Build();
        _pageCustom = new TabPage("Pola niestandardowe");
        _pageCustom.Controls.Add(_customScroll);
        _pageAll = new TabPage("Wszystkie pola");
        _allFieldsGrid.Columns.Add("Pole", "Pole");
        _allFieldsGrid.Columns.Add("Wartość", "Wartość");
        _pageAll.Controls.Add(_allFieldsGrid);
        _pageExtras = new TabPage("Dodatki");
        _panelExtras.Dock = DockStyle.Fill;
        _panelExtras.Visible = true;
        _panelExtras.BorderStyle = BorderStyle.None;
        _pageExtras.Controls.Add(_panelExtras);
        _pageHtml = new TabPage("HTML");
        _htmlView.ScriptErrorsSuppressed = true;
        _htmlView.DocumentCompleted += (_, _) =>
        {
            if (_pendingPrint) { _pendingPrint = false; _htmlView.ShowPrintDialog(); }
        };
        _htmlTools.Items.Add(TBtn(AmcIcon.Refresh, (_, _) => RefreshHtml(), "Odśwież"));
        _htmlTools.Items.Add(TBtn(AmcIcon.FilePrint, (_, _) => PrintCurrent(), "Drukuj"));
        _htmlTools.Items.Add(TBtn(AmcIcon.MovieCopy, (_, _) => CopyHtml(), "Kopiuj HTML"));
        _htmlTools.Dock = DockStyle.Top;
        var htmlPanel = new Panel { Dock = DockStyle.Fill };
        htmlPanel.Controls.Add(_htmlView);
        htmlPanel.Controls.Add(_htmlTools);
        _pageHtml.Controls.Add(htmlPanel);
        _tabs.TabPages.AddRange(new TabPage[] { _pageFields, _pageCustom, _pageAll, _pageExtras, _pageHtml });
        _tabs.SelectedIndexChanged += (_, _) => RefreshSortGroupChecks();
        _panelCenter.Controls.Add(_tabs);
        _panelCenter.BorderStyle = BorderStyle.FixedSingle;
    }

    private Panel Section(string title, bool collapsible, bool startCollapsed, Panel? host = null)
    {
        host ??= _customScroll;
        var body = new Panel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(6, 0, 6, 6) };
        var grid = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2 };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        body.Controls.Add(grid);
        body.Tag = grid;
        Panel? wrapper = null;
        if (collapsible)
        {
            wrapper = new Panel { Dock = DockStyle.Top, AutoSize = true };
            var header = new Button
            {
                Text = (startCollapsed ? "[+] " : "[-] ") + title,
                Dock = DockStyle.Top, Height = 24, FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft, BackColor = SystemColors.Control,
            };
            header.FlatAppearance.BorderSize = 0;
            header.Click += (_, _) =>
            {
                body.Visible = !body.Visible;
                header.Text = (body.Visible ? "[-] " : "[+] ") + title;
            };
            wrapper.Controls.Add(body);
            wrapper.Controls.Add(header);
            body.Visible = !startCollapsed;
            host.Controls.Add(wrapper);
            wrapper.BringToFront();
        }
        else
        {
            var header = new Label { Text = title, Dock = DockStyle.Top, Height = 22, Font = new Font(Font, FontStyle.Bold), BackColor = SystemColors.Control, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(4, 3, 0, 0) };
            var w = new Panel { Dock = DockStyle.Top, AutoSize = true };
            w.Controls.Add(body);
            w.Controls.Add(header);
            host.Controls.Add(w);
            w.BringToFront();
        }
        _sectionBodies[title] = body;
        return body;
    }

    private void AddRow(Panel section, string tag, string label, Control editor)
    {
        var grid = (TableLayoutPanel)section.Tag!;
        int r = grid.RowCount++;
        grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var l = new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Padding = new Padding(0, 5, 0, 0) };
        grid.Controls.Add(l, 0, r);
        editor.Dock = DockStyle.Fill;
        editor.Margin = new Padding(0, 2, 0, 2);
        grid.Controls.Add(editor, 1, r);
        _editors[tag] = editor;
        editor.Leave += (_, _) => CommitField(tag);
    }


    // ================= PRAWA STRONA: dodatki (jak ToolbarExtrasWindow) =================
    private void BuildExtras()
    {
        _extrasTools.Items.Add(TBtn(AmcIcon.ExtrasAdd, (_, _) => ExtraAdd(), "Dodaj dodatek"));
        _extrasTools.Items.Add(TBtn(AmcIcon.ExtrasEdit, (_, _) => ExtraEdit(), "Edytuj dodatek"));
        _extrasTools.Items.Add(TBtn(AmcIcon.ExtrasDel, (_, _) => ExtraDelete(), "Usuń dodatek"));
        _extrasTools.Dock = DockStyle.Top;
        _extrasList.View = View.Details;
        _extrasList.FullRowSelect = true;
        _extrasList.Columns.Add("Tytuł", 130);
        _extrasList.Columns.Add("Kategoria", 90);
        _extrasList.DoubleClick += (_, _) => ExtraEdit();
        _panelExtras.Controls.Add(_extrasList);
        _panelExtras.Controls.Add(_extrasTools);
        _panelExtras.Controls.Add(_extrasTitle);
        _panelExtras.BorderStyle = BorderStyle.FixedSingle;
    }

    // ================= PASEK SZUKANIA (jak ToolbarFind) =================
    private void BuildFindBar()
    {
        var y = 6;
        var l1 = new Label { Text = "Szukaj w &polu:", AutoSize = true, Left = 8, Top = y + 3 };
        _findFieldBox.Left = 100; _findFieldBox.Top = y;
        _findFieldBox.Items.AddRange(FieldValues.SearchableFields().Select(f => (object)f.Name).ToArray());
        _findFieldBox.SelectedIndex = FieldValues.SearchableFields().ToList().FindIndex(f => f.Tag == "Title");
        _findFieldBox.SelectedIndexChanged += (_, _) =>
        {
            var fields = FieldValues.SearchableFields().ToList();
            if (_findFieldBox.SelectedIndex >= 0) _findField = fields[_findFieldBox.SelectedIndex].Tag;
        };
        var l2 = new Label { Text = "Szukana &wartość:", AutoSize = true, Left = 8, Top = y + 27 };
        _findText.Left = 100; _findText.Top = y + 24; _findText.Width = 420;
        _findText.TextChanged += (_, _) => ApplyFind();
        _findText.KeyDown += (_, e) => { if (e.KeyCode == Keys.Escape) ToggleFindBar(false); };
        _findWhole.Left = 530; _findWhole.Top = y + 2;
        _findWhole.CheckedChanged += (_, _) => ApplyFind();
        _findShow.Left = 640; _findShow.Top = y + 2;
        _findShow.CheckedChanged += (_, _) => ApplyFind();
        _findReverse.Left = 760; _findReverse.Top = y + 2;
        _findReverse.CheckedChanged += (_, _) => ApplyFind();
        var btnNext = new Button { Text = "Znajdź następny", Left = 530, Top = y + 24, Width = 130 };
        btnNext.Click += (_, _) => FindNext();
        var btnHide = new Button { Text = "Ukryj", Left = 670, Top = y + 24, Width = 80 };
        btnHide.Click += (_, _) => ToggleFindBar(false);
        _findBar.Controls.AddRange(new Control[] { l1, _findFieldBox, l2, _findText, _findWhole, _findShow, _findReverse, btnNext, btnHide });
        _findBar.BorderStyle = BorderStyle.FixedSingle;
    }

    // ================= DANE =================
    private void OpenDatabase(string path)
    {
        _store = new SqliteGameCatalogStore(path);
        _dbPath = path;
        var data = _store.LoadAll();
        _model.ReplaceAll(data.Games);
        _model.CustomFieldDefs.Clear();
        _model.CustomFieldDefs.AddRange(data.CustomFieldDefs);
        _loans.Clear();
        _loans.AddRange(data.Loans);
        if (_model.Games.Count == 0)
        {
            foreach (var g in SampleDataFactory.CreateSamples()) _model.Add(g);
            _dirty = true;
            Save();
        }
        RefreshCatalogCombos();
        ApplyCustomLists();
        BuildCustomSection();
        RefreshList();
        UpdateTitle();
        PushRecent(path);
    }

    private void UpdateTitle() =>
        Text = $"GameCatalog - [{Path.GetFileName(_dbPath)}]{(_dirty ? " *" : "")}";

    private void MarkDirty()
    {
        _dirty = true;
        UpdateTitle();
        RefreshCatalogCombos();
        RefreshList();
    }

    private void PushUndo(Action undo)
    {
        _undo.Push(undo);
        while (_undo.Count > 50)
        {
            var tmp = _undo.ToArray();
            _undo.Clear();
            foreach (var a in tmp.Take(50).Reverse()) _undo.Push(a);
        }
    }

    private List<Game> CurrentView()
    {
        IEnumerable<Game> q = _model.Games;
        if (_findOnly is not null)
            q = q.Where(g => _findOnly.Contains(g.Number));
        return GameList.Sort(q, _sortTag, _ascending, _prefs.NaturalSort).ToList();
    }

    private void RefreshCatalogCombos()
    {
        // Wartości z katalogu do pól ComboBox (jak listy AMC)
        SetComboItems("Genre", _model.Games.Select(g => g.Genre));
        SetComboItems("Developer", _model.Games.Select(g => g.Developer));
        SetComboItems("Publisher", _model.Games.Select(g => g.Publisher));
        SetComboItems("Store", _model.Games.Select(g => g.Store));
        SetComboItems("Platform", _model.Games.Select(g => g.Platform));
        SetComboItems("Country", _model.Games.Select(g => g.Country));
        SetComboItems("Engine", _model.Games.Select(g => g.Engine));
        SetComboItems("MediaType", _model.Games.Select(g => g.MediaType));
        SetComboItems("GameMode", _model.Games.Select(g => g.GameMode));
    }

    private void SetComboItems(string tag, IEnumerable<string> values)
    {
        if (!_editors.TryGetValue(tag, out var c) || c is not ComboBox cb) return;
        var cur = cb.Text;
        var preset = cb.Items.Cast<object>().Select(o => o.ToString()!).ToList();
        var vals = values.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().OrderBy(s => s);
        cb.Items.Clear();
        foreach (var v in preset.Concat(vals).Distinct()) cb.Items.Add(v);
        cb.Text = cur;
    }

    /// <summary>Własne wartości list rozwijanych z preferencji (jak listy AMC) — doklejane do ComboBoxów.</summary>
    private void ApplyCustomLists()
    {
        foreach (var kv in _prefs.CustomLists)
        {
            if (!_editors.TryGetValue(kv.Key, out var c) || c is not ComboBox cb) continue;
            var cur = cb.Text;
            foreach (var v in kv.Value.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).Distinct())
                if (!cb.Items.Cast<object>().Any(o => o.ToString() == v)) cb.Items.Add(v);
            cb.Text = cur;
        }
    }

    private ImageList? _thumbImages;
    private readonly Dictionary<int, int> _thumbIndex = new();

    /// <summary>Tytuł na liście wg preferencji (Formatowanie tytułu jak w AMC).</summary>
    private string FormattedTitle(Game g) => _prefs.TitleFormat switch
    {
        "Original" => g.Title,
        "Localized" => string.IsNullOrWhiteSpace(g.LocalizedTitle) ? g.Title : g.LocalizedTitle,
        _ => g.DisplayTitle,
    };

    private void RefreshList()
    {
        _refreshing = true;
        try
        {
            int? sel = SelectedGame()?.Number;
            _list.BeginUpdate();
            _list.Groups.Clear();
            _list.Items.Clear();
            var view = CurrentView();
            if (_thumbs)
            {
                _thumbImages = new ImageList { ImageSize = new Size(64, 64), ColorDepth = ColorDepth.Depth32Bit };
                _thumbIndex.Clear();
                _thumbImages.Images.Add("def", _icons.Images[(int)AmcIcon.MoviePicture]);
                foreach (var g in view)
                {
                    int idx = 0;
                    if (g.HasCover)
                    {
                        try
                        {
                            using var ms = new MemoryStream(g.CoverImage!);
                            using var img = Image.FromStream(ms);
                            _thumbImages.Images.Add(g.Number.ToString(), img);
                            idx = _thumbImages.Images.Count - 1;
                        }
                        catch { }
                    }
                    _thumbIndex[g.Number] = idx;
                }
                _list.LargeImageList = _thumbImages;
        _list.View = View.LargeIcon;
            }
            else
            {
                _list.View = View.Details;
                _list.LargeImageList = null;
            }

            Dictionary<string, ListViewGroup> groups = new();
            if (!string.IsNullOrEmpty(_groupTag))
            {
                foreach (var grp in GameList.GroupBy(view, _groupTag))
                {
                    string caption = GameList.GroupCaption(_groupTag, grp.Key, CatalogLabels.Current);
                    string name = _prefs.ShowGroupCount ? $"{caption} ({grp.Count()})" : caption;
                    var lg = new ListViewGroup(name, HorizontalAlignment.Left);
                    _list.Groups.Add(lg);
                    groups[grp.Key] = lg;
                }
            }

            foreach (var g in view)
            {
                var item = new ListViewItem(g.Number.ToString()) { Tag = g.Number, Checked = g.Checked };
                var bg = ColorTagToColor(g.ColorTag);
                if (bg != Color.Empty) item.BackColor = bg;
                if (_thumbs) item.ImageIndex = _thumbIndex.GetValueOrDefault(g.Number, 0);
                foreach (var (tag, _) in _columns.Skip(1))
                    item.SubItems.Add(tag == "Title" ? FormattedTitle(g) : FieldValues.Get(g, tag));
                if (groups.TryGetValue(GameList.GroupKeyOf(g, _groupTag), out var lg)) item.Group = lg;
                _list.Items.Add(item);
            }
            _list.EndUpdate();

            if (sel.HasValue)
                foreach (ListViewItem it in _list.Items)
                    if (it.Tag is int n && n == sel.Value) { it.Selected = true; it.EnsureVisible(); break; }

            _stCount.Text = _groupTag == ""
                ? $"Gier: {view.Count} / {_model.Games.Count}"
                : $"Gier: {view.Count} / {_model.Games.Count}  (grupy: {_list.Groups.Count})";
        }
        finally { _refreshing = false; }
        SelectFromList();
    }

    private Game? SelectedGame()
    {
        if (_list.SelectedItems.Count == 0) return null;
        return _list.SelectedItems[0].Tag is int n ? _model.Find(n) : null;
    }

    private void SelectFromList()
    {
        if (_refreshing) return;
        _current = SelectedGame();
        LoadGameToFrame(_current);
        RefreshExtras();
        RefreshAllFields();
        RefreshHtml();
        if (_current is null)
        {
            _stSelected.Text = "";
            _picture.Image?.Dispose();
            _picture.Image = null;
        }
        else
        {
            _stSelected.Text = $"#{_current.Number} {_current.Title}";
            ShowCover(_current);
        }
    }

    // ================= RAMKA PÓL (jak TMovieFrame LoadFromObject/SaveToObject) =================
    private void LoadGameToFrame(Game? g)
    {
        _refreshing = true;
        try
        {
            _fields.Enabled = g is not null;
            _customScroll.Enabled = g is not null;
            if (g is null) return;
            SetEd("MediaLabel", g.MediaLabel); SetEd("MediaType", g.MediaType); SetEd("Store", g.Store);
            SetDate("DateAdded", g.DateAdded); SetEd("Borrower", g.Borrower); SetDate("DateFinished", g.DateFinished);
            SetEd("Title", g.Title); SetEd("LocalizedTitle", g.LocalizedTitle);
            SetEd("Developer", g.Developer); SetEd("Publisher", g.Publisher);
            SetEd("Designer", g.Designer); SetEd("Composer", g.Composer);
            SetEd("VoiceActors", g.VoiceActors); SetEd("Country", g.Country);
            SetNum("ReleaseYear", g.ReleaseYear); SetNum("PlaytimeHours", (decimal)g.PlaytimeHours);
            SetEd("Genre", g.Genre); SetEd("Pegi", g.Pegi); SetEd("URL", g.Url);
            SetRating("UserRating", g.UserRating); SetRating("CriticRating", g.CriticRating);
            SetEd("InstallPath", g.InstallPath); SetEd("Engine", g.Engine); SetEd("Version", g.Version);
            SetEd("GameMode", g.GameMode); SetEd("Platform", g.Platform);
            SetEd("Resolution", g.Resolution); SetEd("Framerate", g.Framerate);
            SetEd("AudioLanguages", g.AudioLanguages); SetEd("Subtitles", g.Subtitles);
            SetEd("SizeGb", g.SizeGb); SetNum("DiskCount", g.DiskCount);
            SetDate("LastPlayed", g.LastPlayed);
            if (_editors.TryGetValue("CompletionStatus", out var st) && st is ComboBox cb)
                cb.SelectedIndex = (int)g.CompletionStatus;
            SetEd("Description", g.Description); SetEd("Comments", g.Comments);
            if (_editors.TryGetValue("Checked", out var ch) && ch is CheckBox ckb) ckb.Checked = g.Checked;
            foreach (var def in _model.CustomFieldDefs)
            {
                string key = "custom:" + def.Tag;
                if (!_editors.TryGetValue(key, out var ec)) continue;
                string v = g.CustomValues.TryGetValue(def.Tag, out var cv) ? cv : def.DefaultValue;
                switch (ec)
                {
                    case TextBox t: t.Text = v; break;
                    case ComboBox ccb: ccb.Text = v; break;
                    case NumericUpDown n: n.Value = Math.Clamp(decimal.TryParse(v, out var dv) ? dv : 0, n.Minimum, n.Maximum); break;
                    case DateTimePicker p:
                        if (DateTime.TryParse(v, out var dd)) { p.Value = dd; p.Checked = true; }
                        else { p.Value = DateTime.Today; p.Checked = false; }
                        break;
                    case CheckBox c: c.Checked = YesNo.Parse(v); break;
                }
            }
        }
        finally { _refreshing = false; }
    }

    private void SetEd(string tag, string v)
    {
        if (_editors.TryGetValue(tag, out var c))
        {
            if (c is TextBox t) t.Text = v;
            else if (c is ComboBox cb) cb.Text = v;
        }
    }

    private void SetNum(string tag, decimal v)
    {
        if (_editors.TryGetValue(tag, out var c) && c is NumericUpDown n)
            n.Value = Math.Clamp(v, n.Minimum, n.Maximum);
    }

    private void SetDate(string tag, DateTime? d)
    {
        if (_editors.TryGetValue(tag, out var c) && c is DateTimePicker p)
        {
            if (d.HasValue) { p.Value = d.Value; p.Checked = true; }
            else { p.Value = DateTime.Today; p.Checked = false; }
        }
    }

    private void SetRating(string tag, int rating)
    {
        if (!_editors.TryGetValue(tag, out var c)) return;
        var num = c.Controls.Count > 0 ? c.Controls.OfType<NumericUpDown>().FirstOrDefault() : c as NumericUpDown;
        var chk = _editors.TryGetValue(tag + "None", out var h) ? h as CheckBox : null;
        if (num is null) return;
        if (rating < 0) { if (chk is not null) chk.Checked = true; num.Value = 0; }
        else { if (chk is not null) chk.Checked = false; num.Value = Math.Clamp(rating / 10m, 0, 10); }
    }

    private string GetEd(string tag)
    {
        if (!_editors.TryGetValue(tag, out var c)) return "";
        return c switch
        {
            TextBox t => t.Text,
            ComboBox cb => cb.Text,
            _ => "",
        };
    }

    private void CommitField(string tag)
    {
        if (_refreshing || _current is null) return;
        var g = _current;
        bool changed = false;
        void Set<T>(T old, T val, Action<T> apply) { if (!Equals(old, val)) { apply(val); changed = true; } }
        switch (tag)
        {
            case "MediaLabel": Set(g.MediaLabel, GetEd(tag), v => g.MediaLabel = v); break;
            case "MediaType": Set(g.MediaType, GetEd(tag), v => g.MediaType = v); break;
            case "Store": Set(g.Store, GetEd(tag), v => g.Store = v); break;
            case "DateAdded": Set(g.DateAdded, GetDate(tag), v => g.DateAdded = v); break;
            case "Borrower": Set(g.Borrower, GetEd(tag), v => g.Borrower = v); break;
            case "DateFinished": Set(g.DateFinished, GetDate(tag), v => g.DateFinished = v); break;
            case "Title": Set(g.Title, GetEd(tag), v => g.Title = v); break;
            case "LocalizedTitle": Set(g.LocalizedTitle, GetEd(tag), v => g.LocalizedTitle = v); break;
            case "Developer": Set(g.Developer, GetEd(tag), v => g.Developer = v); break;
            case "Publisher": Set(g.Publisher, GetEd(tag), v => g.Publisher = v); break;
            case "Designer": Set(g.Designer, GetEd(tag), v => g.Designer = v); break;
            case "Composer": Set(g.Composer, GetEd(tag), v => g.Composer = v); break;
            case "VoiceActors": Set(g.VoiceActors, GetEd(tag), v => g.VoiceActors = v); break;
            case "Country": Set(g.Country, GetEd(tag), v => g.Country = v); break;
            case "ReleaseYear": Set(g.ReleaseYear, (int)GetNum(tag), v => g.ReleaseYear = v); break;
            case "PlaytimeHours": Set(g.PlaytimeHours, (double)GetNum(tag), v => g.PlaytimeHours = v); break;
            case "Genre": Set(g.Genre, GetEd(tag), v => g.Genre = v); break;
            case "Pegi": Set(g.Pegi, GetEd(tag), v => g.Pegi = v); break;
            case "URL": Set(g.Url, GetEd(tag), v => g.Url = v); break;
            case "UserRating": Set(g.UserRating, GetRating(tag), v => g.UserRating = v); break;
            case "CriticRating": Set(g.CriticRating, GetRating(tag), v => g.CriticRating = v); break;
            case "InstallPath": Set(g.InstallPath, GetEd(tag), v => g.InstallPath = v); break;
            case "Engine": Set(g.Engine, GetEd(tag), v => g.Engine = v); break;
            case "Version": Set(g.Version, GetEd(tag), v => g.Version = v); break;
            case "GameMode": Set(g.GameMode, GetEd(tag), v => g.GameMode = v); break;
            case "Platform": Set(g.Platform, GetEd(tag), v => g.Platform = v); break;
            case "Resolution": Set(g.Resolution, GetEd(tag), v => g.Resolution = v); break;
            case "Framerate": Set(g.Framerate, GetEd(tag), v => g.Framerate = v); break;
            case "AudioLanguages": Set(g.AudioLanguages, GetEd(tag), v => g.AudioLanguages = v); break;
            case "Subtitles": Set(g.Subtitles, GetEd(tag), v => g.Subtitles = v); break;
            case "SizeGb": Set(g.SizeGb, GetEd(tag), v => g.SizeGb = v); break;
            case "DiskCount": Set(g.DiskCount, (int)GetNum(tag), v => g.DiskCount = v); break;
            case "LastPlayed": Set(g.LastPlayed, GetDate(tag), v => g.LastPlayed = v); break;
            case "CompletionStatus":
                if (_editors.TryGetValue(tag, out var st) && st is ComboBox cb && cb.SelectedIndex >= 0)
                    Set(g.CompletionStatus, (CompletionStatus)cb.SelectedIndex, v => g.CompletionStatus = v);
                break;
            case "Description": Set(g.Description, GetEd(tag), v => g.Description = v); break;
            case "Comments": Set(g.Comments, GetEd(tag), v => g.Comments = v); break;
            case "Checked":
                if (_editors.TryGetValue(tag, out var ch) && ch is CheckBox ckb)
                    Set(g.Checked, ckb.Checked, v => g.Checked = v);
                break;
            case string t when t.StartsWith("custom:", StringComparison.Ordinal):
                if (CommitCustomField(g, t["custom:".Length..])) changed = true;
                break;
        }
        if (changed)
        {
            _dirty = true;
            UpdateTitle();
            if (_prefs.ForceFullRefresh) RefreshList();
            else RefreshRow(g);
            RefreshAllFields();
            if (tag is "Title" or "LocalizedTitle") RefreshHtml();
        }
    }

    private bool CommitCustomField(Game g, string tag)
    {
        if (!_editors.TryGetValue("custom:" + tag, out var ec)) return false;
        string v = ec switch
        {
            TextBox t => t.Text,
            ComboBox cb => cb.Text,
            NumericUpDown n => n.Value.ToString(),
            DateTimePicker p => p.Checked ? p.Value.ToString("yyyy-MM-dd") : "",
            CheckBox c => c.Checked ? YesNo.StoredTrue : "",
            _ => "",
        };
        string old = g.CustomValues.TryGetValue(tag, out var cv) ? cv : "";
        if (old == v) return false;
        if (v == "") g.CustomValues.Remove(tag);
        else g.CustomValues[tag] = v;
        return true;
    }

    private DateTime? GetDate(string tag) =>
        _editors.TryGetValue(tag, out var c) && c is DateTimePicker p && p.Checked ? p.Value.Date : null;

    private decimal GetNum(string tag) =>
        _editors.TryGetValue(tag, out var c) && c is NumericUpDown n ? n.Value : 0;

    private int GetRating(string tag)
    {
        var c = _editors.TryGetValue(tag, out var ctl) ? ctl : null;
        var num = c?.Controls.OfType<NumericUpDown>().FirstOrDefault();
        var chk = _editors.TryGetValue(tag + "None", out var h) ? h as CheckBox : null;
        if (chk is not null && chk.Checked) return -1;
        return num is null ? -1 : (int)Math.Round(num.Value * 10);
    }

    private void RefreshRow(Game g)
    {
        _refreshing = true;
        try
        {
            foreach (ListViewItem it in _list.Items)
            {
                if (it.Tag is int n && n == g.Number)
                {
                    for (int i = 1; i < _columns.Count; i++)
                    {
                        var tag = _columns[i].Tag;
                        it.SubItems[i].Text = tag == "Title" ? FormattedTitle(g) : FieldValues.Get(g, tag);
                    }
                    if (it.Checked != g.Checked) it.Checked = g.Checked;
                    var bg = ColorTagToColor(g.ColorTag);
                    it.BackColor = bg == Color.Empty ? _list.BackColor : bg;
                    break;
                }
            }
        }
        finally { _refreshing = false; }
    }

    private void RefreshAllFields()
    {
        _allFieldsGrid.Rows.Clear();
        if (_current is null) return;
        foreach (var f in GameFields.All)
            _allFieldsGrid.Rows.Add(CatalogLabels.Current.Field(f.Tag), FieldValues.Get(_current, f.Tag));
        foreach (var d in _model.CustomFieldDefs)
            _allFieldsGrid.Rows.Add(d.Name, _current.CustomValues.TryGetValue(d.Tag, out var v) ? v : "");
    }

    // ================= OKŁADKA (jak PanelPicture + ToolbarPicture) =================
    private void ShowCover(Game g)
    {
        _picture.Image?.Dispose();
        _picture.Image = null;
        if (!g.HasCover) return;
        try { using var ms = new MemoryStream(g.CoverImage!); _picture.Image = Image.FromStream(ms); }
        catch { }
    }

    private void TogglePicturePanel()
    {
        _picPanel.Visible = !_picPanel.Visible;
        _picSplitter.Visible = _picPanel.Visible;
        _mPicShow.Checked = _picPanel.Visible;
        _prefs.CoverVisible = _picPanel.Visible;
        _prefs.Save();
    }

    private void PictureLoad()
    {
        if (_current is null) return;
        using var o = new OpenFileDialog { Filter = "Obrazy (*.jpg;*.jpeg;*.png;*.gif;*.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp" };
        if (o.ShowDialog(this) != DialogResult.OK) return;
        var old = _current.CoverImage;
        _current.CoverImage = DownscaleCover(File.ReadAllBytes(o.FileName), _prefs.MaxCoverWidth);
        PushUndo(() => { _current.CoverImage = old; ShowCover(_current); _dirty = true; });
        ShowCover(_current);
        _dirty = true; UpdateTitle();
        _stHint.Text = "Wczytano okładkę.";
    }

    /// <summary>Skalowanie okładki do maks. szerokości (preferencje Import obrazków).</summary>
    private static byte[] DownscaleCover(byte[] bytes, int maxWidth)
    {
        if (maxWidth <= 0) return bytes;
        try
        {
            using var ms = new MemoryStream(bytes);
            using var img = Image.FromStream(ms);
            if (img.Width <= maxWidth) return bytes;
            int h = (int)Math.Round(img.Height * (maxWidth / (double)img.Width));
            using var bmp = new Bitmap(img, new Size(maxWidth, h));
            using var out_ms = new MemoryStream();
            bmp.Save(out_ms, System.Drawing.Imaging.ImageFormat.Png);
            return out_ms.ToArray();
        }
        catch { return bytes; }
    }

    private void PictureSaveAs()
    {
        if (_current?.HasCover != true) return;
        using var s = new SaveFileDialog { Filter = "PNG (*.png)|*.png|JPEG (*.jpg)|*.jpg", FileName = $"okladka_{_current.Number}.png" };
        if (s.ShowDialog(this) == DialogResult.OK) File.WriteAllBytes(s.FileName, _current.CoverImage!);
    }

    private void PictureCopy()
    {
        if (_picture.Image is not null) Clipboard.SetImage(_picture.Image);
    }

    private void PicturePaste()
    {
        if (_current is null || !Clipboard.ContainsImage()) return;
        using var img = Clipboard.GetImage();
        using var ms = new MemoryStream();
        img!.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        var old = _current.CoverImage;
        _current.CoverImage = ms.ToArray();
        PushUndo(() => { _current.CoverImage = old; ShowCover(_current); _dirty = true; });
        ShowCover(_current);
        _dirty = true; UpdateTitle();
    }

    private void PictureDelete()
    {
        if (_current?.HasCover != true) return;
        var old = _current.CoverImage;
        _current.CoverImage = null;
        PushUndo(() => { _current.CoverImage = old; ShowCover(_current); _dirty = true; });
        ShowCover(_current);
        _dirty = true; UpdateTitle();
    }

    // ================= DODATKI (zakładka jak w AMC) =================
    private void ShowExtrasTab()
    {
        _tabs.SelectedTab = _pageExtras;
        RefreshExtras();
    }

    private void RefreshExtras()
    {
        _extrasList.Items.Clear();
        if (_extrasList.Columns.Count > 1)
            _extrasList.Columns[1].Width = _prefs.ShowExtraCategory ? 90 : 0;
        var g = _current;
        _extrasTitle.Text = g is null ? "Dodatki" : $"Dodatki ({g.Extras.Count})";
        if (g is null) return;
        foreach (var e in g.Extras)
        {
            var it = new ListViewItem(e.Title) { Tag = e };
            it.SubItems.Add(e.Category);
            _extrasList.Items.Add(it);
        }
    }

    private void ExtraAdd()
    {
        if (_current is null) return;
        var e = new GameExtra { GameNumber = _current.Number, Category = "DLC fabularne" };
        using var f = new ExtraEditForm(e);
        if (f.ShowDialog(this) != DialogResult.OK) return;
        _current.Extras.Add(e);
        PushUndo(() => { _current.Extras.Remove(e); RefreshExtras(); _dirty = true; });
        RefreshExtras();
        _dirty = true; UpdateTitle();
    }

    private void ExtraEdit()
    {
        if (_current is null || _extrasList.SelectedItems.Count == 0) return;
        var e = (GameExtra)_extrasList.SelectedItems[0].Tag!;
        var backup = e.Clone();
        using var f = new ExtraEditForm(e);
        if (f.ShowDialog(this) != DialogResult.OK) return;
        PushUndo(() =>
        {
            e.Title = backup.Title; e.Category = backup.Category; e.Tag = backup.Tag;
            e.CreatedBy = backup.CreatedBy; e.Url = backup.Url;
            e.Description = backup.Description; e.Comments = backup.Comments;
            RefreshExtras(); _dirty = true;
        });
        RefreshExtras();
        _dirty = true; UpdateTitle();
    }

    private void ExtraDelete()
    {
        if (_current is null || _extrasList.SelectedItems.Count == 0) return;
        var e = (GameExtra)_extrasList.SelectedItems[0].Tag!;
        int idx = _current.Extras.IndexOf(e);
        _current.Extras.Remove(e);
        PushUndo(() => { _current.Extras.Insert(Math.Max(0, idx), e); RefreshExtras(); _dirty = true; });
        RefreshExtras();
        _dirty = true; UpdateTitle();
    }

    // ================= HTML (jak HTMLViewer + szablony) =================
    private string? _htmlCoverTemp;
    private bool _pendingPrint;

    private string CoverFileUri(Game g)
    {
        if (!g.HasCover) return "";
        try
        {
            var tmp = Path.Combine(Path.GetTempPath(), $"gamecatalog_cover_{g.Number}.png");
            File.WriteAllBytes(tmp, g.CoverImage!);
            if (_htmlCoverTemp is not null && _htmlCoverTemp != tmp && File.Exists(_htmlCoverTemp))
                File.Delete(_htmlCoverTemp);
            _htmlCoverTemp = tmp;
            return "file:///" + tmp.Replace('\\', '/');
        }
        catch { return ""; }
    }

    private string BuildGameHtml(Game g)
    {
        var uri = CoverFileUri(g);
        return HtmlTemplates.Expand(HtmlTemplates.Load(_prefs.HtmlTemplate), g,
            string.IsNullOrEmpty(uri) ? null : uri);
    }

    private void RefreshHtml()
    {
        if (_current is null) { _htmlView.DocumentText = "<html><body></body></html>"; return; }
        _htmlView.DocumentText = BuildGameHtml(_current);
    }

    private void ShowHtmlTab()
    {
        _tabs.SelectedTab = _pageHtml;
        RefreshSortGroupChecks();
    }

    private void CopyHtml()
    {
        if (_current is null) return;
        Clipboard.SetText(BuildGameHtml(_current));
        _stHint.Text = "Skopiowano HTML karty gry.";
    }

    private void PrintCurrent()
    {
        if (_current is null) return;
        ShowHtmlTab();
        RefreshHtml();
        _pendingPrint = true;
    }

    // ================= AKCJE PLIK =================
    private void Save()
    {
        CommitAllFields();
        if (_prefs.BackupOnSave && File.Exists(_dbPath))
        {
            try { File.Copy(_dbPath, _dbPath + ".bak", true); }
            catch { }
        }
        _store.SaveAll(_model, _loans);
        _dirty = false;
        UpdateTitle();
        _stHint.Text = $"Zapisano. Gier: {_model.Games.Count}";
    }

    private void CommitAllFields()
    {
        if (_current is null) return;
        foreach (var tag in _editors.Keys.ToList())
        {
            if (!tag.EndsWith("None", StringComparison.Ordinal))
                CommitField(tag);
        }
    }

    private void SaveAs()
    {
        using var s = new SaveFileDialog { Filter = "Katalog GameCatalog (*.gc)|*.gc", FileName = Path.GetFileName(_dbPath) };
        if (s.ShowDialog(this) != DialogResult.OK) return;
        _store = new SqliteGameCatalogStore(s.FileName);
        _dbPath = s.FileName;
        Save();
    }

    private void NewCatalog()
    {
        if (_dirty)
        {
            var r = MessageBox.Show(this, "Zapisać zmiany przed utworzeniem nowego katalogu?", "GameCatalog",
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (r == DialogResult.Yes) Save();
            else if (r == DialogResult.Cancel) return;
        }
        using var s = new SaveFileDialog { Filter = "Katalog GameCatalog (*.gc)|*.gc", FileName = "catalog.gc" };
        if (s.ShowDialog(this) != DialogResult.OK) return;
        _model.ReplaceAll(Enumerable.Empty<Game>());
        _loans.Clear();
        _undo.Clear();
        _findOnly = null;
        _store = new SqliteGameCatalogStore(s.FileName);
        _dbPath = s.FileName;
        _store.Initialize();
        _store.SaveAll(_model, _loans);
        _dirty = false;
        RefreshCatalogCombos();
        RefreshList();
        UpdateTitle();
    }

    private void OpenDatabaseDialog()
    {
        using var o = new OpenFileDialog { Filter = "Katalog GameCatalog (*.gc)|*.gc|Wszystkie pliki (*.*)|*.*" };
        if (o.ShowDialog(this) == DialogResult.OK) OpenDatabase(o.FileName);
    }

    private void ShowProperties()
    {
        new CatalogPropertiesDialog(_dbPath, _model.Games.Count,
            _model.Games.Sum(g => g.Extras.Count), _model.Games.Sum(g => g.PlaytimeHours),
            _loans.Count(l => !l.IsReturned)).ShowDialog(this);
    }

    private void ExportCsv()
    {
        using var s = new SaveFileDialog { Filter = "CSV (*.csv)|*.csv", FileName = "katalog.csv" };
        if (s.ShowDialog(this) != DialogResult.OK) return;
        CatalogTransfer.ExportCsv(CurrentView(), s.FileName, _prefs.CsvSeparator());
        _stHint.Text = "Wyeksportowano CSV.";
    }

    private void ExportHtml()
    {
        using var s = new SaveFileDialog { Filter = "HTML (*.html)|*.html", FileName = "katalog.html" };
        if (s.ShowDialog(this) != DialogResult.OK) return;
        CatalogTransfer.ExportHtml(CurrentView(), s.FileName);
        _stHint.Text = "Wyeksportowano HTML.";
    }

    private void ImportFolder()
    {
        using var f = new FolderBrowserDialog { Description = "Wybierz folder z grami (podfolder = gra)" };
        if (f.ShowDialog(this) != DialogResult.OK) return;
        var imported = CatalogTransfer.ImportFromFolder(f.SelectedPath, _model.NextNumber(), _prefs.MediaPatterns);
        foreach (var g in imported) _model.Add(g);
        PushUndo(() => { foreach (var g in imported) _model.Remove(g.Number); MarkDirty(); });
        MarkDirty();
        MessageBox.Show(this, $"Zaimportowano {imported.Count} gier z folderu.", "Import");
    }

    private void ImportCsv()
    {
        using var o = new OpenFileDialog { Filter = "CSV (*.csv)|*.csv" };
        if (o.ShowDialog(this) != DialogResult.OK) return;
        var imported = CatalogTransfer.ImportCsv(o.FileName, _model.NextNumber(), _prefs.CsvSeparator());
        foreach (var g in imported) _model.Add(g);
        PushUndo(() => { foreach (var g in imported) _model.Remove(g.Number); MarkDirty(); });
        MarkDirty();
        MessageBox.Show(this, $"Zaimportowano {imported.Count} gier z CSV.", "Import");
    }

    private void ImportAmc()
    {
        using var o = new OpenFileDialog { Filter = "Ant Movie Catalog XML (*.xml)|*.xml" };
        if (o.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            var imported = AmcXmlImporter.Import(o.FileName, _model.NextNumber());
            foreach (var g in imported) _model.Add(g);
            PushUndo(() => { foreach (var g in imported) _model.Remove(g.Number); MarkDirty(); });
            MarkDirty();
            MessageBox.Show(this, $"Zaimportowano {imported.Count} pozycji z AMC (pola filmowe→growe).", "Import AMC");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Błąd importu AMC: " + ex.Message, "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ================= AKCJE GRA =================
    private void AddGame()
    {
        var g = new Game { Platform = "PC", MediaType = "Cyfrowa", Title = "Nowa gra", DateAdded = DateTime.Today };
        if (!_prefs.AutoNumbering)
        {
            // Tryb AMC: zapytaj o numer (okno „Wybór numeru gry”).
            int proposed = _model.NextNumber();
            using var dlg = new NumberConflictDialog(proposed, Math.Max(proposed, _model.NextNumber()), n => _model.Find(n) is not null, n => _model.Find(n)?.DisplayTitle);
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            if (dlg.DontAskAgain) { _prefs.AutoNumbering = true; _prefs.Save(); }
            int? resolved = ResolveNewNumber(dlg.ChosenNumber, dlg.Resolution);
            if (resolved is null) return;
            g.Number = resolved.Value;
        }
        _model.Add(g);
        PushUndo(() => { _model.Remove(g.Number); MarkDirty(); });
        MarkDirty();
        SelectNumber(g.Number);
        _tabs.SelectedIndex = 0;
        _fields.FocusTitle();
    }

    /// <summary>Rozstrzyga numer nowej gry wg wyboru z dialogu AMC. Zwraca null, gdy anulowano.
    /// W razie RenumberExisting przenumerowuje kolidującą grę (z undo).</summary>
    private int? ResolveNewNumber(int desired, NumberConflictResolution choice)
    {
        var existing = _model.Find(desired);
        if (existing is null) return desired;
        return choice switch
        {
            NumberConflictResolution.AcceptDuplicate => desired,
            NumberConflictResolution.ShiftNew => ShiftUntilFree(desired),
            NumberConflictResolution.RenumberExisting => RenumberExistingForNew(desired),
            _ => desired,
        };
    }

    private static int ShiftUntilFreeStatic(Func<int, bool> exists, int start)
    {
        int n = start;
        while (exists(n)) n++;
        return n;
    }

    private int ShiftUntilFree(int start) => ShiftUntilFreeStatic(n => _model.Find(n) is not null, start);

    private int? RenumberExistingForNew(int desired)
    {
        var existing = _model.Find(desired);
        if (existing is null) return desired;
        int free = ShiftUntilFree(_model.NextNumber());
        var gameNums = _model.Games.ToDictionary(x => x, x => x.Number);
        var loanNums = _loans.ToDictionary(l => l, l => l.GameNumber);
        int oldNum = existing.Number;
        foreach (var l in _loans)
            if (l.GameNumber == oldNum) l.GameNumber = free;
        foreach (var e in existing.Extras) e.GameNumber = free;
        existing.Number = free;
        _model.ReplaceAll(_model.Games.OrderBy(x => x.Number));
        PushUndo(() =>
        {
            foreach (var kv in gameNums)
            {
                foreach (var e in kv.Key.Extras) e.GameNumber = kv.Value;
                kv.Key.Number = kv.Value;
            }
            foreach (var kv in loanNums) kv.Key.GameNumber = kv.Value;
            _model.ReplaceAll(_model.Games.OrderBy(x => x.Number));
            MarkDirty();
        });
        MarkDirty();
        return desired;
    }

    private void DeleteGame()
    {
        // Skrót Delete nie może zadziałać podczas edycji tekstu (jak w AMC)
        if (_fields.ContainsFocus || _findBar.ContainsFocus || ActiveControl is TextBox) return;
        var g = SelectedGame();
        if (g is null) return;
        if (MessageBox.Show(this, $"Usunąć „{g.Title}”?", "Potwierdź",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        var backup = g.Clone();
        _model.Remove(g.Number);
        PushUndo(() => { _model.Add(backup); MarkDirty(); SelectNumber(backup.Number); });
        MarkDirty();
    }

    private void SelectNumber(int number)
    {
        foreach (ListViewItem it in _list.Items)
            if (it.Tag is int n && n == number) { it.Selected = true; it.EnsureVisible(); break; }
    }

    private void GoToNumber()
    {
        var g = SelectedGame();
        using var d = new NumberDialog(g?.Number ?? 1, Math.Max(1, _model.Games.Count == 0 ? 1 : _model.Games.Max(x => x.Number)));
        if (d.ShowDialog(this) != DialogResult.OK) return;
        var target = _model.Find(d.Value);
        if (target is null) { MessageBox.Show(this, $"Brak gry o numerze {d.Value}."); return; }
        _findOnly = null;
        RefreshList();
        SelectNumber(d.Value);
    }

    private void RandomGame()
    {
        var view = CurrentView();
        if (view.Count == 0) return;
        SelectNumber(view[Random.Shared.Next(view.Count)].Number);
        _stHint.Text = "Wylosowano grę.";
    }

    private void CopyGame()
    {
        var g = SelectedGame();
        if (g is null) return;
        Clipboard.SetText(ClipboardHeader + JsonSerializer.Serialize(g));
        _stHint.Text = $"Skopiowano „{g.Title}”.";
    }

    private void PasteGame()
    {
        try
        {
            var t = Clipboard.GetText();
            if (!t.StartsWith(ClipboardHeader, StringComparison.Ordinal))
            {
                MessageBox.Show(this, "Schowek nie zawiera gry.", "Wklej");
                return;
            }
            var g = JsonSerializer.Deserialize<Game>(t[ClipboardHeader.Length..]);
            if (g is null || string.IsNullOrWhiteSpace(g.Title)) return;
            g.Number = 0;
            _model.Add(g);
            PushUndo(() => { _model.Remove(g.Number); MarkDirty(); });
            MarkDirty();
            SelectNumber(g.Number);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Błąd wklejania: " + ex.Message, "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Undo()
    {
        if (_undo.Count == 0) { _stHint.Text = "Brak operacji do cofnięcia."; return; }
        _undo.Pop()();
        _stHint.Text = "Cofnięto operację.";
    }

    private void SetAllChecked(bool value)
    {
        foreach (var g in CurrentView()) g.Checked = value;
        _dirty = true; UpdateTitle(); RefreshList();
    }

    private void SetColorTag(int idx)
    {
        var g = SelectedGame();
        if (g is null) return;
        g.ColorTag = idx;
        _dirty = true; UpdateTitle(); RefreshRow(g);
    }

    private void Renumber()
    {
        var view = CurrentView();
        if (view.Count == 0) return;
        if (MessageBox.Show(this, $"Przenumerować {view.Count} gier (1…{view.Count}) wg bieżącego sortowania?",
                "Przenumeruj", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        var old = view.ToDictionary(g => g, g => g.Number);
        var loanOld = _loans.ToDictionary(l => l, l => l.GameNumber);
        var newByOld = new Dictionary<int, int>();
        int n = 1;
        foreach (var g in view)
        {
            int prev = g.Number;
            newByOld[prev] = n;
            g.Number = n++;
            foreach (var e in g.Extras) e.GameNumber = g.Number;
        }
        foreach (var l in _loans)
            if (newByOld.TryGetValue(l.GameNumber, out var nn)) l.GameNumber = nn;
        var games = _model.Games.ToList();
        _model.ReplaceAll(games.OrderBy(g => g.Number));
        PushUndo(() =>
        {
            foreach (var kv in old)
            {
                foreach (var e in kv.Key.Extras) e.GameNumber = kv.Value;
                kv.Key.Number = kv.Value;
            }
            foreach (var kv in loanOld) kv.Key.GameNumber = kv.Value;
            _model.ReplaceAll(_model.Games.OrderBy(g => g.Number));
            MarkDirty();
        });
        MarkDirty();
    }

    private void OpenUrl()
    {
        var url = _current?.Url;
        if (string.IsNullOrWhiteSpace(url)) return;
        try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true }); }
        catch (Exception ex) { MessageBox.Show(this, "Nie można otworzyć URL: " + ex.Message); }
    }

    private void CopyUrl()
    {
        if (!string.IsNullOrWhiteSpace(_current?.Url)) Clipboard.SetText(_current.Url);
    }

    private void OpenPath()
    {
        var p = _current?.InstallPath;
        if (string.IsNullOrWhiteSpace(p)) return;
        try
        {
            string target = Directory.Exists(p) ? p : File.Exists(p) ? Path.GetDirectoryName(p)! : p;
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("explorer", $"\"{target}\"") { UseShellExecute = true });
        }
        catch (Exception ex) { MessageBox.Show(this, "Nie można otworzyć ścieżki: " + ex.Message); }
    }

    private void CopyPath()
    {
        if (!string.IsNullOrWhiteSpace(_current?.InstallPath)) Clipboard.SetText(_current.InstallPath);
    }

    // ================= WIDOK =================
    private void SetSort(string tag)
    {
        _sortTag = tag;
        RefreshSortGroupChecks();
        RefreshList();
    }

    private void SetOrder(bool asc)
    {
        _ascending = asc;
        RefreshSortGroupChecks();
        RefreshList();
    }

    private void SetGroup(string tag)
    {
        _groupTag = tag;
        RefreshSortGroupChecks();
        RefreshGroupMenu();
        RefreshList();
    }

    private void ToggleThumbs()
    {
        _thumbs = !_thumbs;
        _mThumbs.Checked = _thumbs;
        RefreshList();
    }

    private void StretchList()
    {
        if (_list.Columns.Count == 0) return;
        int total = _list.ClientSize.Width;
        int fixedW = _list.Columns.Cast<ColumnHeader>().Skip(1).Sum(c => c.Width);
        _list.Columns[0].Width = Math.Max(40, total - fixedW - 4);
    }

    private static readonly List<(string Tag, string Name)> AvailableColumns = new()
    {
        ("Number", "#"), ("Title", "Tytuł"), ("Developer", "Developer"), ("Publisher", "Wydawca"),
        ("Genre", "Gatunek"), ("Platform", "Platforma"), ("ReleaseYear", "Rok"),
        ("CompletionStatus", "Status"), ("UserRating", "Ocena"), ("PlaytimeHours", "Godziny"), ("Store", "Sklep"),
    };

    private void RebuildFieldsMenu() => RebuildColumnsMenu(_fieldsMenu.DropDownItems);

    private void RebuildColumnsMenu(ToolStripItemCollection items)
    {
        items.Clear();
        var sel = new HashSet<string>(_columns.Select(c => c.Tag));
        foreach (var (tag, name) in AvailableColumns)
        {
            string t = tag;
            var item = new ToolStripMenuItem(name) { Checked = sel.Contains(t), CheckOnClick = true };
            item.CheckedChanged += (_, _) => ToggleColumn(t, item.Checked);
            items.Add(item);
        }
    }

    private void ToggleColumn(string tag, bool on)
    {
        if (on && !_columns.Any(c => c.Tag == tag))
        {
            var widths = new Dictionary<string, int> { ["Number"] = 40, ["Title"] = 210, ["ReleaseYear"] = 50, ["UserRating"] = 55, ["PlaytimeHours"] = 60, ["Platform"] = 80, ["Genre"] = 100, ["CompletionStatus"] = 105 };
            _columns.Add((tag, widths.TryGetValue(tag, out var w) ? w : 110));
        }
        else if (!on && _columns.Count > 1)
        {
            _columns.RemoveAll(c => c.Tag == tag);
        }
        RebuildColumns();
        RefreshList();
    }

    private void ToggleFindBar(bool? show = null)
    {
        _findBar.Visible = show ?? !_findBar.Visible;
        if (_findBar.Visible) _findText.Focus();
        else { _findOnly = null; RefreshList(); }
    }

    private void ApplyFind()
    {
        var text = _findText.Text.Trim();
        if (string.IsNullOrEmpty(text)) { _findOnly = null; RefreshList(); return; }
        bool Matches(Game g)
        {
            var v = FieldValues.Get(g, _findField);
            bool hit = _findWhole.Checked
                ? v.Equals(text, StringComparison.OrdinalIgnoreCase)
                : v.Contains(text, StringComparison.OrdinalIgnoreCase);
            return _findReverse.Checked ? !hit : hit;
        }
        if (_findShow.Checked)
        {
            _findOnly = _model.Games.Where(Matches).Select(g => g.Number).ToHashSet();
            RefreshList();
        }
        else
        {
            _findOnly = null;
            RefreshList();
            var m = CurrentView().FirstOrDefault(Matches);
            if (m is not null) SelectNumber(m.Number);
            _stHint.Text = m is null ? "Nie znaleziono." : $"Znaleziono: #{m.Number} {m.Title}";
        }
    }

    private void FindNext()
    {
        var text = _findText.Text.Trim();
        if (string.IsNullOrEmpty(text)) return;
        var view = CurrentView();
        bool Matches(Game g)
        {
            var v = FieldValues.Get(g, _findField);
            bool hit = _findWhole.Checked
                ? v.Equals(text, StringComparison.OrdinalIgnoreCase)
                : v.Contains(text, StringComparison.OrdinalIgnoreCase);
            return _findReverse.Checked ? !hit : hit;
        }
        int start = 0;
        var cur = SelectedGame();
        if (cur is not null)
        {
            int i = view.FindIndex(g => g.Number == cur.Number);
            if (i >= 0) start = (i + 1) % view.Count;
        }
        for (int k = 0; k < view.Count; k++)
        {
            var g = view[(start + k) % view.Count];
            if (Matches(g)) { SelectNumber(g.Number); return; }
        }
        _stHint.Text = "Nie znaleziono.";
    }

    // ================= SKRYPTY (jak getscript.pas) =================
    private void OpenScriptsManager()
    {
        var g = SelectedGame();
        if (g is null) { _stHint.Text = "Brak wybranej gry."; return; }
        using var f = new ScriptsForm(g, async script => await RunScriptAsync(script, testOnly: true));
        f.ShowDialog(this);
    }

    private void RunScriptDialog()
    {
        var g = SelectedGame();
        if (g is null) { _stHint.Text = "Brak wybranej gry."; return; }
        var scripts = Scripting.GameScript.LoadAll();
        if (scripts.Count == 0)
        {
            MessageBox.Show(this, "Brak skryptów w folderze Skrypty.", "Skrypty");
            return;
        }
        using var d = new Form
        {
            Text = "Pobierz informacje ze skryptu", Width = 440, Height = 150,
            StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false, MinimizeBox = false,
        };
        var cb = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
        foreach (var s in scripts) cb.Items.Add(s);
        cb.SelectedIndex = 0;
        var bar = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 44, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(8) };
        var ok = new Button { Text = "Uruchom", DialogResult = DialogResult.OK, Width = 90 };
        var cancel = new Button { Text = "Anuluj", DialogResult = DialogResult.Cancel, Width = 90 };
        bar.Controls.Add(ok); bar.Controls.Add(cancel);
        d.Controls.Add(cb);
        d.Controls.Add(bar);
        d.AcceptButton = ok; d.CancelButton = cancel;
        if (d.ShowDialog(this) != DialogResult.OK || cb.SelectedItem is not Scripting.GameScript script) return;
        _ = RunScriptAsync(script, testOnly: false);
    }

    private async Task RunScriptAsync(Scripting.GameScript script, bool testOnly)
    {
        var g = SelectedGame();
        if (g is null) return;
        int Pick(string title, IList<string> options)
        {
            if (InvokeRequired)
                return (int)Invoke(new Func<int>(() => Pick(title, options)));
            using var p = new PickForm(title, options);
            return p.ShowDialog(this) == DialogResult.OK ? p.SelectedIndex : -1;
        }
        UseWaitCursor = true;
        GameInfoResult result;
        try
        {
            result = await Scripting.ScriptRunner.RunAsync(script, g, Pick).ConfigureAwait(true);
        }
        finally { UseWaitCursor = false; }
        if (!result.Success)
        {
            MessageBox.Show(this, result.Error + "\n\n" + string.Join("\n", result.Log),
                "Skrypt — błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        using var rf = new ScriptResultForm(g, result);
        if (rf.ShowDialog(this) != DialogResult.OK || !result.HasData) return;
        var backup = g.Clone();
        ApplyScriptResult(g, rf.CheckedFields);
        foreach (var e in result.Extras) { e.GameNumber = g.Number; g.Extras.Add(e); }
        if (rf.IncludeCover) g.CoverImage = result.Cover;
        PushUndo(() =>
        {
            var list = _model.Games.ToList();
            int i = list.FindIndex(x => x.Number == backup.Number);
            if (i >= 0) { list[i] = backup; _model.ReplaceAll(list); MarkDirty(); SelectNumber(backup.Number); }
        });
        MarkDirty();
        if (!testOnly) SelectNumber(g.Number);
        _stHint.Text = $"Skrypt „{script.Name}” zastosowany.";
    }

    private static void ApplyScriptResult(Game g, IEnumerable<(string Tag, string Value)> fields)
    {
        foreach (var (tag, value) in fields)
            GameFieldWriter.Set(g, tag, value);
    }

    // ================= NOWE AKCJE MENU (jak w AMC) =================
    private void RebuildMruMenu()
    {
        _mruMenu.DropDownItems.Clear();
        var existing = _prefs.RecentFiles.Where(File.Exists).ToList();
        if (existing.Count == 0)
        {
            var none = new ToolStripMenuItem("(brak)") { Enabled = false };
            _mruMenu.DropDownItems.Add(none);
            return;
        }
        int i = 1;
        foreach (var path in existing)
        {
            string p = path;
            int n = i++;
            var item = new ToolStripMenuItem($"{n}: {p}");
            item.Click += (_, _) => OpenDatabase(p);
            _mruMenu.DropDownItems.Add(item);
        }
    }

    private void PushRecent(string path)
    {
        _prefs.PushRecent(path);
        _prefs.Save();
        RebuildMruMenu();
    }

    private void ImportDialog()
    {
        using var d = new Form
        {
            Text = "Importuj", Width = 300, Height = 220, StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false,
        };
        var p = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(20) };
        var b1 = new Button { Text = "Z folderu…", Width = 240, Height = 32 };
        var b2 = new Button { Text = "Z pliku CSV…", Width = 240, Height = 32 };
        var b3 = new Button { Text = "Z pliku AMC XML…", Width = 240, Height = 32 };
        b1.Click += (_, _) => { d.DialogResult = DialogResult.OK; d.Close(); ImportFolder(); };
        b2.Click += (_, _) => { d.DialogResult = DialogResult.OK; d.Close(); ImportCsv(); };
        b3.Click += (_, _) => { d.DialogResult = DialogResult.OK; d.Close(); ImportAmc(); };
        p.Controls.AddRange(new Control[] { b1, b2, b3 });
        d.Controls.Add(p);
        d.ShowDialog(this);
    }

    private void ExportDialog()
    {
        using var d = new Form
        {
            Text = "Eksportuj", Width = 300, Height = 180, StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false,
        };
        var p = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(20) };
        var b1 = new Button { Text = "Do pliku CSV…", Width = 240, Height = 32 };
        var b2 = new Button { Text = "Do pliku HTML…", Width = 240, Height = 32 };
        b1.Click += (_, _) => { d.DialogResult = DialogResult.OK; d.Close(); ExportCsv(); };
        b2.Click += (_, _) => { d.DialogResult = DialogResult.OK; d.Close(); ExportHtml(); };
        p.Controls.AddRange(new Control[] { b1, b2 });
        d.Controls.Add(p);
        d.ShowDialog(this);
    }

    private void ChangeNumber()
    {
        var g = SelectedGame();
        if (g is null) return;
        using var d = new NumberDialog(g.Number, Math.Max(_model.NextNumber(), g.Number));
        d.Text = "Zmień numer";
        if (d.ShowDialog(this) != DialogResult.OK || d.Value == g.Number) return;
        var other = _model.Find(d.Value);
        NumberConflictResolution choice = NumberConflictResolution.RenumberExisting;
        if (other is not null)
        {
            // Konflikt jak w AMC — pokaż okno „Wybór numeru gry”.
            using var dlg = new NumberConflictDialog(d.Value, Math.Max(_model.NextNumber(), d.Value), n => n != g.Number && _model.Find(n) is not null, n => n != g.Number ? _model.Find(n)?.DisplayTitle : null);
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            if (dlg.DontAskAgain) { _prefs.AutoNumbering = true; _prefs.Save(); }
            choice = dlg.Resolution;
            if (choice == NumberConflictResolution.ShiftNew)
            {
                int free = ShiftUntilFree(dlg.ChosenNumber == d.Value ? d.Value : dlg.ChosenNumber);
                if (TryChangeNumber(g, free, NumberConflictResolution.AcceptDuplicate)) SelectNumber(free);
                return;
            }
            int target = dlg.ChosenNumber;
            if (target != d.Value)
            {
                // Użytkownik zmienił numer w dialogu konfliktu.
                other = _model.Find(target);
                if (other is null) choice = NumberConflictResolution.AcceptDuplicate;
                if (TryChangeNumber(g, target, choice)) SelectNumber(target);
                return;
            }
        }
        if (TryChangeNumber(g, d.Value, choice)) SelectNumber(g.Number);
    }

    private bool TryChangeNumber(Game g, int newNum, NumberConflictResolution conflict = NumberConflictResolution.RenumberExisting)
    {
        if (newNum == g.Number) return false;
        var other = _model.Find(newNum);
        if (other is not null && conflict == NumberConflictResolution.AcceptDuplicate)
        {
            // Duplikat: przepnij tylko przenoszoną grę (wypożyczenia/dodatki za nią).
            var dupGameNums = _model.Games.ToDictionary(x => x, x => x.Number);
            var dupLoanNums = _loans.ToDictionary(l => l, l => l.GameNumber);
            int dupOldNum = g.Number;
            foreach (var l in _loans)
                if (l.GameNumber == dupOldNum) l.GameNumber = newNum;
            foreach (var e in g.Extras) e.GameNumber = newNum;
            g.Number = newNum;
            _model.ReplaceAll(_model.Games.OrderBy(x => x.Number));
            PushUndo(() =>
            {
                foreach (var kv in dupGameNums)
                {
                    foreach (var e in kv.Key.Extras) e.GameNumber = kv.Value;
                    kv.Key.Number = kv.Value;
                }
                foreach (var kv in dupLoanNums) kv.Key.GameNumber = kv.Value;
                _model.ReplaceAll(_model.Games.OrderBy(x => x.Number));
                MarkDirty();
            });
            MarkDirty();
            return true;
        }
        var gameNums = _model.Games.ToDictionary(x => x, x => x.Number);
        var loanNums = _loans.ToDictionary(l => l, l => l.GameNumber);
        int oldNum = g.Number;
        foreach (var l in _loans)
        {
            if (l.GameNumber == oldNum) l.GameNumber = newNum;
            else if (other is not null && l.GameNumber == newNum) l.GameNumber = oldNum;
        }
        foreach (var e in g.Extras) e.GameNumber = newNum;
        if (other is not null)
        {
            foreach (var e in other.Extras) e.GameNumber = oldNum;
            other.Number = oldNum;
        }
        g.Number = newNum;
        _model.ReplaceAll(_model.Games.OrderBy(x => x.Number));
        PushUndo(() =>
        {
            foreach (var kv in gameNums)
            {
                foreach (var e in kv.Key.Extras) e.GameNumber = kv.Value;
                kv.Key.Number = kv.Value;
            }
            foreach (var kv in loanNums) kv.Key.GameNumber = kv.Value;
            _model.ReplaceAll(_model.Games.OrderBy(x => x.Number));
            MarkDirty();
        });
        MarkDirty();
        return true;
    }

    private void WebSearch(string siteName)
    {
        var title = _current?.Title?.Trim();
        if (string.IsNullOrEmpty(title)) { _stHint.Text = "Brak wybranej gry."; return; }
        string q = Uri.EscapeDataString(title);
        var sites = _prefs.CustomSearchSites.Count > 0 ? _prefs.CustomSearchSites : PrefsStore.DefaultSearchSites();
        var site = sites.FirstOrDefault(s => s.Name.Equals(siteName, StringComparison.OrdinalIgnoreCase))
            ?? sites.FirstOrDefault() ?? new SearchSite("Google", "https://www.google.com/search?q={q}");
        string url = site.Url.Contains("{q}") ? site.Url.Replace("{q}", q) : site.Url + q;
        try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true }); }
        catch (Exception ex) { MessageBox.Show(this, "Nie można otworzyć wyszukiwarki: " + ex.Message); }
    }

    private void RebuildWebMenu()
    {
        if (_mWeb is null) return;
        _mWeb.DropDownItems.Clear();
        var sites = _prefs.CustomSearchSites.Count > 0 ? _prefs.CustomSearchSites : PrefsStore.DefaultSearchSites();
        foreach (var s in sites)
        {
            string name = s.Name;
            _mWeb.DropDownItems.Add(MI(name, (_, _) => WebSearch(name), AmcIcon.MovieSearch));
        }
    }

    private void ToggleGroupCount()
    {
        _prefs.ShowGroupCount = !_prefs.ShowGroupCount;
        _mGroupCount.Checked = _prefs.ShowGroupCount;
        _prefs.Save();
        RefreshList();
    }

    private void ShowFlatTable()
    {
        SetGroup("");
        if (_thumbs) ToggleThumbs();
        RefreshSortGroupChecks();
    }

    private void RebuildHtmlMenu()
    {
        _htmlMenu.DropDownItems.Clear();
        foreach (var name in HtmlTemplates.Names)
        {
            string n = name;
            var item = new ToolStripMenuItem(n) { Checked = _prefs.HtmlTemplate == n };
            item.Click += (_, _) =>
            {
                _prefs.HtmlTemplate = n;
                _prefs.Save();
                RefreshHtml();
            };
            _htmlMenu.DropDownItems.Add(item);
        }
    }

    private void ManageFields()
    {
        using var f = new CustomFieldsForm(_model.CustomFieldDefs);
        f.ShowDialog(this);
        _dirty = true;
        UpdateTitle();
        BuildCustomSection();
        RefreshAllFields();
    }

    private void ManagePictures()
    {
        using var f = new PicturesForm(_model);
        if (f.ShowDialog(this) != DialogResult.OK) return;
        _dirty = true;
        UpdateTitle();
        if (_current is not null) ShowCover(_current);
        RefreshList();
    }

    private void EditHtml()
    {
        using var f = new HtmlEditorForm(_prefs.HtmlTemplate);
        if (f.ShowDialog(this) == DialogResult.OK) RefreshHtml();
    }

    private void EditPrefs()
    {
        using var f = new PrefsForm(_prefs);
        if (f.ShowDialog(this) != DialogResult.OK) return;
        _prefs.AutoNumbering = f.AutoNumbering;
        _prefs.TitleFormat = f.TitleFormat;
        _prefs.NaturalSort = f.NaturalSort;
        _prefs.ForceFullRefresh = f.ForceFullRefresh;
        _prefs.ReopenLastFile = f.ReopenLastFile;
        _prefs.FullRowSelect = f.FullRowSelect;
        _prefs.ShowGridLines = f.ShowGridLines;
        _prefs.CoverVisible = f.CoverVisible;
        _prefs.ShowExtraCategory = f.ShowExtraCategory;
        _prefs.CsvDelimiter = f.CsvDelimiter;
        _prefs.BackupOnSave = f.BackupOnSave;
        _prefs.MediaPatterns = f.MediaPatterns;
        _prefs.MaxCoverWidth = f.MaxCoverWidth;
        _prefs.ScriptTimeoutSec = Math.Max(5, f.ScriptTimeoutSec);
        _prefs.CustomLists = f.CustomLists;
        _prefs.CustomSearchSites = f.CustomSearchSites;
        GameCatalog.Scripting.ScriptRunner.Timeout = TimeSpan.FromSeconds(_prefs.ScriptTimeoutSec);
        bool htmlChanged = f.HtmlTemplate != _prefs.HtmlTemplate;
        bool groupChanged = f.ShowGroupCount != _prefs.ShowGroupCount;
        bool themeChanged = f.Theme != _prefs.Theme;
        bool menuIconsChanged = f.ShowMenuIcons != _prefs.ShowMenuIcons;
        _prefs.HtmlTemplate = f.HtmlTemplate;
        _prefs.ShowGroupCount = f.ShowGroupCount;
        _prefs.Theme = f.Theme;
        _prefs.ShowMenuIcons = f.ShowMenuIcons;
        if (htmlChanged)
        {
            RebuildHtmlMenu();
            RefreshHtml();
        }
        if (groupChanged)
        {
            _mGroupCount.Checked = _prefs.ShowGroupCount;
        }
        if (themeChanged)
        {
            ApplyTheme();
        }
        if (menuIconsChanged)
        {
            ApplyIconLists();
        }
        _list.FullRowSelect = _prefs.FullRowSelect;
        _list.GridLines = _prefs.ShowGridLines;
        if (_picPanel.Visible != _prefs.CoverVisible)
        {
            _picPanel.Visible = _picSplitter.Visible = _prefs.CoverVisible;
            _mPicShow.Checked = _prefs.CoverVisible;
        }
        ApplyCustomLists();
        RebuildWebMenu();
        RefreshList();
        RefreshExtras();
        _prefs.Save();
    }

    private void BuildCustomSection()
    {
        if (_sectionBodies.TryGetValue("Pola własne", out var old) && old.Parent is Control ow)
        {
            ow.Parent?.Controls.Remove(ow);
            ow.Dispose();
            _sectionBodies.Remove("Pola własne");
            foreach (var k in _editors.Keys.Where(k => k.StartsWith("custom:", StringComparison.Ordinal)).ToList())
                _editors.Remove(k);
        }
        if (_model.CustomFieldDefs.Count == 0) return;
        var s = Section("Pola własne", collapsible: true, startCollapsed: false, host: _customScroll);
        foreach (var def in _model.CustomFieldDefs)
        {
            string key = "custom:" + def.Tag;
            Control ed = def.FieldType switch
            {
                "Number" => new NumericUpDown { Maximum = 1000000, Minimum = -1000000 },
                "Date" => new DateTimePicker { ShowCheckBox = true, Format = DateTimePickerFormat.Short },
                "List" => ComboboxWithItems(def.ListValues),
                "YesNo" => new CheckBox { Text = def.Name, AutoSize = true },
                _ => new TextBox(),
            };
            AddRow(s, key, def.Name + ":", ed);
        }
        LoadGameToFrame(_current);
    }

    private static ComboBox ComboboxWithItems(string listValues)
    {
        var cb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDown };
        foreach (var v in listValues.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            cb.Items.Add(v);
        return cb;
    }

    // ================= NARZĘDZIA =================
    private void ShowStats() => new StatsForm(_model, CurrentView().Select(g => g.Number).ToHashSet(), SelectedGame()?.Number, _dbPath).ShowDialog(this);

    private void ShowLoans()
    {
        using var f = new LoanForm(_model, _loans);
        if (f.ShowDialog(this) == DialogResult.OK)
        {
            _dirty = true; UpdateTitle();
            RefreshAllFields();
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        CommitAllFields();
        if (_dirty)
        {
            var r = MessageBox.Show(this, "Zapisać zmiany przed zamknięciem?", "GameCatalog",
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (r == DialogResult.Yes) Save();
            else if (r == DialogResult.Cancel) e.Cancel = true;
        }
        base.OnFormClosing(e);
    }
}
