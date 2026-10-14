using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;

namespace GameCatalog.App;

/// <summary>Interim Polish captions (replaced by TranslatorCatalogLabels in Task 6).</summary>
internal sealed class PolishCatalogLabels : ICatalogLabels
{
    private static readonly Dictionary<string, string> Fields = new()
    {
        ["Number"] = "Numer", ["Checked"] = "Zaznaczona", ["ColorTag"] = "Etykieta koloru",
        ["MediaLabel"] = "Etykieta nośnika", ["MediaType"] = "Typ nośnika", ["Store"] = "Sklep / źródło",
        ["DateAdded"] = "Data dodania", ["Borrower"] = "Pożyczona komu", ["DateFinished"] = "Data ukończenia",
        ["UserRating"] = "Ocena własna", ["CriticRating"] = "Ocena recenzentów", ["Title"] = "Tytuł",
        ["LocalizedTitle"] = "Tytuł polski", ["DisplayTitle"] = "Tytuł wyświetlany", ["Developer"] = "Developer",
        ["Publisher"] = "Wydawca", ["Designer"] = "Projektant", ["Composer"] = "Kompozytor",
        ["VoiceActors"] = "Aktorzy głosowi", ["Country"] = "Kraj", ["ReleaseYear"] = "Rok wydania",
        ["PlaytimeHours"] = "Czas gry (h)", ["Genre"] = "Gatunek", ["Pegi"] = "PEGI", ["URL"] = "Strona WWW",
        ["Description"] = "Opis", ["Comments"] = "Komentarze", ["InstallPath"] = "Ścieżka instalacji",
        ["Engine"] = "Silnik", ["Version"] = "Wersja", ["GameMode"] = "Tryb gry", ["Platform"] = "Platforma",
        ["Resolution"] = "Rozdzielczość", ["Framerate"] = "Klatkaż", ["AudioLanguages"] = "Dubbing",
        ["Subtitles"] = "Napisy", ["SizeGb"] = "Rozmiar (GB)", ["DiskCount"] = "Liczba płyt",
        ["CompletionStatus"] = "Status", ["LastPlayed"] = "Ostatnio grano", ["NbExtras"] = "Liczba dodatków",
    };

    public string Field(string tag) => Fields.TryGetValue(tag, out var s) ? s : tag;

    public string Status(CompletionStatus status) => status switch
    {
        CompletionStatus.Backlog => "Do zagrania",
        CompletionStatus.Playing => "Ogrywana",
        CompletionStatus.Completed => "Ukończona",
        CompletionStatus.Completed100 => "Ukończona 100%",
        CompletionStatus.Paused => "Wstrzymana",
        CompletionStatus.Dropped => "Porzucona",
        _ => status.ToString(),
    };

    public string Yes => "Tak";
    public string No => "Nie";
    public string NoValue => "(brak)";
}
