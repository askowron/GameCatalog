namespace GameCatalog.Core.Catalog;

/// <summary>
/// Definicje pól katalogu gier — odpowiednik AMC fields.pas.
/// Zachowuje identyfikatory/tagі w stylu AMC (do importu starych XML),
/// ale nazwy i część pól przeportowano z filmowych na growe.
/// </summary>
public static class GameFields
{
    public sealed record FieldDef(int Id, string Tag, string SqlTag, string DisplayName, string AmcOrigin);

    public static readonly IReadOnlyList<FieldDef> All = new List<FieldDef>
    {
        new(0,  "Number",          "NUM",             "Numer",            "Number (bez zmian)"),
        new(1,  "Checked",         "CHECKED",         "Zaznaczona",       "Checked (bez zmian)"),
        new(2,  "ColorTag",        "COLORTAG",        "Etykieta koloru",  "ColorTag (bez zmian)"),
        new(3,  "MediaLabel",      "MEDIA",           "Etykieta nośnika", "MediaLabel (bez zmian)"),
        new(4,  "MediaType",       "MEDIATYPE",       "Typ nośnika",      "MediaType: DVD→Cyfrowa/Fizyczna"),
        new(5,  "Store",           "SOURCE",          "Sklep / źródło",   "Source→Store (Steam, GOG, Epic…)"),
        new(6,  "DateAdded",       "DATEADDED",       "Data dodania",     "Date (bez zmian)"),
        new(7,  "Borrower",        "BORROWER",        "Pożyczona komu",   "Borrower (bez zmian)"),
        new(8,  "DateFinished",    "DATEWATCHED",     "Data ukończenia",  "DateWatched→Data ukończenia"),
        new(9,  "UserRating",      "USERRATING",      "Ocena własna",     "UserRating (bez zmian, 0–10)"),
        new(10, "CriticRating",    "RATING",          "Ocena recenzentów","Rating→Metacritic/OpenCritic"),
        new(11, "Title",           "ORIGINALTITLE",   "Tytuł",            "OriginalTitle→Tytuł"),
        new(12, "LocalizedTitle",  "TRANSLATEDTITLE", "Tytuł polski",     "TranslatedTitle→Tytuł polski"),
        new(13, "DisplayTitle",    "FORMATTEDTITLE",  "Tytuł wyświetlany","FormattedTitle (wirtualne)"),
        new(14, "Developer",       "DIRECTOR",        "Developer",        "Director→Developer (studio)"),
        new(15, "Publisher",       "PRODUCER",        "Wydawca",          "Producer→Publisher"),
        new(16, "Designer",        "WRITER",          "Projektant",       "Writer→Designer/scenarzysta"),
        new(17, "Composer",        "COMPOSER",        "Kompozytor",       "Composer (bez zmian)"),
        new(18, "VoiceActors",     "ACTORS",          "Aktorzy głosowi",  "Actors→Aktorzy głosowi"),
        new(19, "Country",         "COUNTRY",         "Kraj",             "Country (bez zmian)"),
        new(20, "ReleaseYear",     "YEAR",            "Rok wydania",      "Year→Rok wydania"),
        new(21, "PlaytimeHours",   "LENGTH",          "Czas gry (h)",     "Length (min)→godziny gry"),
        new(22, "Genre",           "CATEGORY",        "Gatunek",          "Category→Gatunek"),
        new(23, "Pegi",            "CERTIFICATION",   "PEGI",             "Certification→PEGI"),
        new(24, "URL",             "URL",             "Strona WWW",       "URL (bez zmian)"),
        new(25, "Description",     "DESCRIPTION",     "Opis",             "Description (bez zmian)"),
        new(26, "Comments",        "COMMENTS",        "Komentarze",       "Comments (bez zmian)"),
        new(27, "InstallPath",     "FILEPATH",        "Ścieżka instalacji","FilePath→Ścieżka instalacji"),
        new(28, "Engine",          "VIDEOFORMAT",     "Silnik",           "VideoFormat→Silnik (Unity/Unreal…)"),
        new(29, "Version",         "VIDEOBITRATE",    "Wersja",           "VideoBitrate→Wersja gry"),
        new(30, "GameMode",        "AUDIOFORMAT",     "Tryb gry",         "AudioFormat→Single/Multi/Co-op"),
        new(31, "Platform",        "AUDIOBITRATE",    "Platforma",        "NOWE: PC/PS5/Xbox/Switch…"),
        new(32, "Resolution",      "RESOLUTION",      "Rozdzielczość",    "Resolution (bez zmian)"),
        new(33, "Framerate",       "FRAMERATE",       "Klatkaż",          "Framerate (bez zmian)"),
        new(34, "AudioLanguages",  "LANGUAGES",       "Dubbing",          "Languages→Dubbing"),
        new(35, "Subtitles",       "SUBTITLES",       "Napisy",           "Subtitles (bez zmian)"),
        new(36, "SizeGb",          "FILESIZE",        "Rozmiar (GB)",     "Size→Rozmiar GB"),
        new(37, "DiskCount",       "DISKS",           "Liczba płyt",      "Disks (bez zmian)"),
        new(38, "CompletionStatus","COMPLETION",      "Status",           "NOWE: Backlog/Ogrywana/Ukończona…"),
        new(39, "LastPlayed",      "LASTPLAYED",      "Ostatnio grano",   "NOWE: ostatnia sesja"),
        new(40, "NbExtras",        "NBEXTRAS",        "Liczba dodatków",  "NbExtras→Liczba DLC (wirtualne)"),
    };

    public static readonly IReadOnlyList<string> GroupableTags = new[]
    {
        "Genre", "Platform", "Developer", "Publisher", "Store",
        "CompletionStatus", "Pegi", "ReleaseYear", "GameMode", "Engine"
    };

    public static readonly IReadOnlyList<string> SortableTags =
        All.Select(f => f.Tag).ToList();

    public static string DisplayNameOf(string tag)
        => All.FirstOrDefault(f => f.Tag.Equals(tag, StringComparison.OrdinalIgnoreCase))?.DisplayName ?? tag;
}
