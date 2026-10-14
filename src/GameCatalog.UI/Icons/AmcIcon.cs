namespace GameCatalog.UI.Icons;

/// <summary>
/// Original Ant Movie Catalog 16x16 toolbar icons. Order is identical to TIconIndex in
/// ConstValues.pas — the numeric value is the index in the icon strip bitmap.
/// <see cref="None"/> means "no icon".
/// </summary>
public enum AmcIcon
{
    None = -1,
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
