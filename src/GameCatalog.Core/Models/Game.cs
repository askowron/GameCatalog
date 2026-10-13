namespace GameCatalog.Core.Models;

/// <summary>
/// Gra — główny rekord katalogu. Odpowiednik AMC TMovie,
/// z polami filmowymi przeportowanymi na growe (patrz <see cref="Catalog.GameFields"/>).
/// Mapowanie AMC→Game:
/// Director→Developer, Producer→Publisher, Writer→Designer,
/// Actors→VoiceActors, Category→Genre, Certification→Pegi,
/// Length→PlaytimeHours, DateWatched→DateFinished, VideoFormat→Engine,
/// AudioFormat→GameMode + nowe: Platform, CompletionStatus, LastPlayed.
/// </summary>
public sealed class Game
{
    public int Number { get; set; }
    public bool Checked { get; set; }
    public int ColorTag { get; set; }

    // Nośnik / źródło (jak w AMC: Media, MediaType, Source)
    public string MediaLabel { get; set; } = string.Empty;
    public string MediaType { get; set; } = "Cyfrowa";
    public string Store { get; set; } = string.Empty;

    public DateTime? DateAdded { get; set; }
    public string Borrower { get; set; } = string.Empty;

    // Postęp gracza
    public DateTime? DateFinished { get; set; }
    public DateTime? LastPlayed { get; set; }
    public CompletionStatus CompletionStatus { get; set; } = CompletionStatus.Backlog;

    // Oceny jak w AMC: -1 = brak; w UI 0..10
    public int UserRating { get; set; } = -1;
    public int CriticRating { get; set; } = -1;

    // Tytuły
    public string Title { get; set; } = string.Empty;
    public string LocalizedTitle { get; set; } = string.Empty;
    public string DisplayTitle => string.IsNullOrWhiteSpace(LocalizedTitle) ? Title : $"{Title} / {LocalizedTitle}";

    // Twórcy (przeportowane z Director/Producer/Writer/Composer/Actors)
    public string Developer { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public string Designer { get; set; } = string.Empty;
    public string Composer { get; set; } = string.Empty;
    public string VoiceActors { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    // Dane wydawnicze i klasyfikacja
    public int ReleaseYear { get; set; }
    public double PlaytimeHours { get; set; }
    public string Genre { get; set; } = string.Empty;
    public string Pegi { get; set; } = string.Empty;
    public string Platform { get; set; } = "PC";

    public string Url { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Comments { get; set; } = string.Empty;

    // Pliki / technikalia (przeportowane z FilePath/Video*/Audio*/Resolution/...)
    public string InstallPath { get; set; } = string.Empty;
    public string Engine { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string GameMode { get; set; } = string.Empty;
    public string Resolution { get; set; } = string.Empty;
    public string Framerate { get; set; } = string.Empty;
    public string AudioLanguages { get; set; } = string.Empty;
    public string Subtitles { get; set; } = string.Empty;
    public string SizeGb { get; set; } = string.Empty;
    public int DiskCount { get; set; }

    // Okładka (w AMC: Picture) — bajty PNG/JPG, null = brak
    public byte[]? CoverImage { get; set; }
    public bool HasCover => CoverImage is { Length: > 0 };

    // Dodatki/DLC (w AMC: Extras)
    public List<GameExtra> Extras { get; set; } = new();
    public int NbExtras => Extras.Count;

    // Pola własne (w AMC: CustomFields) — słownik Tag→Wartość
    public Dictionary<string, string> CustomValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public string GetFormattedTitle(string format = "{title} ({year})")
    {
        return format
            .Replace("{title}", Title)
            .Replace("{localized}", LocalizedTitle)
            .Replace("{year}", ReleaseYear > 0 ? ReleaseYear.ToString() : "?")
            .Replace("{platform}", Platform ?? string.Empty)
            .Replace("{genre}", Genre ?? string.Empty)
            .Replace("{number}", Number.ToString());
    }

    public Game Clone()
    {
        var copy = (Game)MemberwiseClone();
        copy.Extras = Extras.Select(e => e.Clone()).ToList();
        copy.CustomValues = new Dictionary<string, string>(CustomValues, StringComparer.OrdinalIgnoreCase);
        if (CoverImage is not null) copy.CoverImage = (byte[])CoverImage.Clone();
        return copy;
    }
}
