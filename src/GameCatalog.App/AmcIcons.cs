namespace GameCatalog.App;

/// <summary>
/// Oryginalne ikony paska narzędzi Ant Movie Catalog (16x16).
/// Kolejność identyczna jak TIconIndex w ConstValues.pas — (int)AmcIcon to indeks ikony.
/// Motywy jak w opcjach AMC: Klasyczny, Kryształowy, Świeży, Gnom, Scrows, Windows XP.
/// </summary>
public enum AmcIcon
{
    FileNew, FileOpen, FileSave, FileSaveAs, FileImport, FileExport, FilePrint, FileProperties,
    MovieAdd, MovieNumber, MovieDelete, MovieFind, MovieCopy, MoviePaste, MovieUndo, MoviePicture,
    PictureOpen, PictureSave, PictureCopy, PictureDelete, PictureUndock,
    MovieImport, MovieImportFiles, MovieImportScript, MovieImportCD, MovieSearch,
    Scripting, GridMode, Renumber, Statistics, Loans, Options, Languages, Group, Refresh,
    Help, About, Exit,
    LoanBorrowAdd, LoanBorrowDel, LoanBorrowGet, LoanCheckIn, LoanCheckOut, LoanOptions, LoanFind,
    TemplateNew, TemplateLoad, TemplateSave, TemplateSaveAs,
    StatsSave, StatsCopy, StatsOptions,
    ScriptNew, ScriptOpen, ScriptSave, ScriptSaveAs, ScriptProperties, ScriptFind, ScriptFindNext,
    DebugRun, DebugStop, DebugBreakpoint, DebugBreakClear, DebugStepOver, DebugRunToCursor,
    DebugEvaluate, DebugWatchAdd, DebugWatchRemove, DebugWatchClear,
    ViewFilter, ViewList, ViewDetail,
    PrintRefresh, PrintPageFirst, PrintPagePrevious, PrintPageNext, PrintPageLast,
    PrintPageFull, PrintPageTwo, PrintPageWidth, PrintLoad, PrintZoomOut, PrintZoomIn, Print, PrintDesigner,
    Browse, RowInsert, RowDelete,
    MoveUp, MoveDown, MoveLeft, MoveRight, MoveLeftAll, MoveRightAll,
    Random, Html, FieldEdit, FieldAdd, FieldDel, FieldMove, Field, MovieUrl,
    StretchList, SortAscend, ViewThumbnails, Input, GroupsFormat, SortDescend,
    MovieExtras, ExtrasAdd, ExtrasDel, ExtrasEdit, ExtrasImportFiles,
}

/// <summary>Ładuje wybrany motyw paska ikon AMC i tnie go na ikony 16x16 do ImageList.</summary>
public static class AmcIcons
{
    public static readonly IReadOnlyList<(string Name, string File)> Themes = new List<(string, string)>
    {
        ("Klasyczny", "Classic.bmp"),
        ("Kryształowy", "Crystal.bmp"),
        ("Świeży", "Fresh.bmp"),
        ("Gnom", "Gnome.bmp"),
        ("Scrows", "Scrows.bmp"),
        ("Windows XP", "Windows XP.bmp"),
    };

    public const string DefaultTheme = "Windows XP";

    public static ImageList LoadStrip(string? theme = null)
    {
        var images = new ImageList
        {
            ImageSize = new Size(16, 16),
            ColorDepth = ColorDepth.Depth32Bit,
            TransparentColor = Color.Magenta,
        };
        string file = Themes.FirstOrDefault(t => t.Name == theme).File ?? "Windows XP.bmp";
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", file);
        if (!File.Exists(path))
            path = Path.Combine(Application.StartupPath, "Resources", file);
        using var strip = new Bitmap(path);
        int count = strip.Width / 16;
        for (int i = 0; i < count; i++)
        {
            var icon = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(icon))
                g.DrawImage(strip, new Rectangle(0, 0, 16, 16), new Rectangle(i * 16, 0, 16, 16), GraphicsUnit.Pixel);
            images.Images.Add(icon);
        }
        return images;
    }
}
