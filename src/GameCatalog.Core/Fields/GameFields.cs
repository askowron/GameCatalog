namespace GameCatalog.Core.Fields;

/// <summary>
/// Field catalogue of a game record — port of AMC fields.pas.
/// Tags are the canonical identifiers used everywhere (sorting, grouping, scripts, plugins).
/// SqlTags keep the original AMC names so legacy AMC data maps onto game fields.
/// </summary>
public static class GameFields
{
    /// <param name="Id">Stable index, same order as AMC field ids.</param>
    /// <param name="Tag">Canonical field identifier.</param>
    /// <param name="SqlTag">Original AMC tag.</param>
    /// <param name="DefaultCaption">English caption, used when no translation is available.</param>
    /// <param name="AmcOrigin">How the AMC movie field was ported (documentation only).</param>
    public sealed record FieldDef(int Id, string Tag, string SqlTag, string DefaultCaption, string AmcOrigin)
    {
        /// <summary>Translation key of the caption, e.g. <c>Field.Developer</c>.</summary>
        public string LabelKey => "Field." + Tag;
    }

    public static readonly IReadOnlyList<FieldDef> All = new List<FieldDef>
    {
        new(0,  "Number",           "NUM",             "Number",              "Number (unchanged)"),
        new(1,  "Checked",          "CHECKED",         "Checked",             "Checked (unchanged)"),
        new(2,  "ColorTag",         "COLORTAG",        "Color tag",           "ColorTag (unchanged)"),
        new(3,  "MediaLabel",       "MEDIA",           "Media label",         "MediaLabel (unchanged)"),
        new(4,  "MediaType",        "MEDIATYPE",       "Media type",          "MediaType: DVD → digital/physical"),
        new(5,  "Store",            "SOURCE",          "Store / source",      "Source → store (Steam, GOG, Epic…)"),
        new(6,  "DateAdded",        "DATEADDED",       "Date added",          "Date (unchanged)"),
        new(7,  "Borrower",         "BORROWER",        "Borrower",            "Borrower (unchanged)"),
        new(8,  "DateFinished",     "DATEWATCHED",     "Date finished",       "DateWatched → date finished"),
        new(9,  "UserRating",       "USERRATING",      "User rating",         "UserRating (unchanged, 0–10)"),
        new(10, "CriticRating",     "RATING",          "Critic rating",       "Rating → Metacritic/OpenCritic"),
        new(11, "Title",            "ORIGINALTITLE",   "Title",               "OriginalTitle → title"),
        new(12, "LocalizedTitle",   "TRANSLATEDTITLE", "Localized title",     "TranslatedTitle → localized title"),
        new(13, "DisplayTitle",     "FORMATTEDTITLE",  "Display title",       "FormattedTitle (virtual)"),
        new(14, "Developer",        "DIRECTOR",        "Developer",           "Director → developer studio"),
        new(15, "Publisher",        "PRODUCER",        "Publisher",           "Producer → publisher"),
        new(16, "Designer",         "WRITER",          "Designer",            "Writer → designer/writer"),
        new(17, "Composer",         "COMPOSER",        "Composer",            "Composer (unchanged)"),
        new(18, "VoiceActors",      "ACTORS",          "Voice actors",        "Actors → voice actors"),
        new(19, "Country",          "COUNTRY",         "Country",             "Country (unchanged)"),
        new(20, "ReleaseYear",      "YEAR",            "Release year",        "Year → release year"),
        new(21, "PlaytimeHours",    "LENGTH",          "Playtime (h)",        "Length (min) → hours played"),
        new(22, "Genre",            "CATEGORY",        "Genre",               "Category → genre"),
        new(23, "Pegi",             "CERTIFICATION",   "PEGI",                "Certification → PEGI"),
        new(24, "URL",              "URL",             "Web page",            "URL (unchanged)"),
        new(25, "Description",      "DESCRIPTION",     "Description",         "Description (unchanged)"),
        new(26, "Comments",         "COMMENTS",        "Comments",            "Comments (unchanged)"),
        new(27, "InstallPath",      "FILEPATH",        "Install path",        "FilePath → install path"),
        new(28, "Engine",           "VIDEOFORMAT",     "Engine",              "VideoFormat → engine (Unity/Unreal…)"),
        new(29, "Version",          "VIDEOBITRATE",    "Version",             "VideoBitrate → game version"),
        new(30, "GameMode",         "AUDIOFORMAT",     "Game mode",           "AudioFormat → single/multi/co-op"),
        new(31, "Platform",         "AUDIOBITRATE",    "Platform",            "NEW: PC/PS5/Xbox/Switch…"),
        new(32, "Resolution",       "RESOLUTION",      "Resolution",          "Resolution (unchanged)"),
        new(33, "Framerate",        "FRAMERATE",       "Framerate",           "Framerate (unchanged)"),
        new(34, "AudioLanguages",   "LANGUAGES",       "Audio languages",     "Languages → dubbing"),
        new(35, "Subtitles",        "SUBTITLES",       "Subtitles",           "Subtitles (unchanged)"),
        new(36, "SizeGb",           "FILESIZE",        "Size (GB)",           "Size → size in GB"),
        new(37, "DiskCount",        "DISKS",           "Disc count",          "Disks (unchanged)"),
        new(38, "CompletionStatus", "COMPLETION",      "Status",              "NEW: backlog/playing/completed…"),
        new(39, "LastPlayed",       "LASTPLAYED",      "Last played",         "NEW: last session"),
        new(40, "NbExtras",         "NBEXTRAS",        "Extras count",        "NbExtras → DLC count (virtual)"),
    };

    /// <summary>Fields that are computed and can be neither edited nor searched.</summary>
    public static readonly IReadOnlySet<string> VirtualTags =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "DisplayTitle", "NbExtras" };

    public static readonly IReadOnlyList<string> GroupableTags = new[]
    {
        "Genre", "Platform", "Developer", "Publisher", "Store",
        "CompletionStatus", "Pegi", "ReleaseYear", "GameMode", "Engine",
    };

    /// <summary>Sort options offered in the View → Sort menu (same set as the original port).</summary>
    public static readonly IReadOnlyList<string> SortableTags = new[]
    {
        "Number", "Title", "Genre", "Platform", "Developer", "Publisher", "ReleaseYear",
        "UserRating", "CriticRating", "PlaytimeHours", "CompletionStatus", "DateAdded", "LastPlayed", "Store",
    };

    /// <summary>Fields offered by the find bar (AMC cbxField).</summary>
    public static readonly IReadOnlyList<FieldDef> Searchable =
        All.Where(f => !VirtualTags.Contains(f.Tag)).ToList();

    public static FieldDef? Find(string tag) =>
        All.FirstOrDefault(f => f.Tag.Equals(tag, StringComparison.OrdinalIgnoreCase));
}
