namespace GameCatalog.Core.Models;

/// <summary>
/// Game — the main catalog record. Counterpart of AMC TMovie,
/// with movie fields ported to game fields (see <see cref="Fields.GameFields"/>).
/// AMC→Game mapping:
/// Director→Developer, Producer→Publisher, Writer→Designer,
/// Actors→VoiceActors, Category→Genre, Certification→Pegi,
/// Length→PlaytimeHours, DateWatched→DateFinished, VideoFormat→Engine,
/// AudioFormat→GameMode + new: Platform, CompletionStatus, LastPlayed.
/// </summary>
public sealed class Game
{
    public int Number { get; set; }
    public bool Checked { get; set; }
    public int ColorTag { get; set; }

    // Media / source (as in AMC: Media, MediaType, Source)
    public string MediaLabel { get; set; } = string.Empty;
    public string MediaType { get; set; } = "Cyfrowa";
    public string Store { get; set; } = string.Empty;

    public DateTime? DateAdded { get; set; }
    public string Borrower { get; set; } = string.Empty;

    // Player progress
    public DateTime? DateFinished { get; set; }
    public DateTime? LastPlayed { get; set; }
    public CompletionStatus CompletionStatus { get; set; } = CompletionStatus.Backlog;

    // Ratings as in AMC: -1 = none; shown as 0..10 in the UI
    public int UserRating { get; set; } = -1;
    public int CriticRating { get; set; } = -1;

    // Titles
    public string Title { get; set; } = string.Empty;
    public string LocalizedTitle { get; set; } = string.Empty;
    public string DisplayTitle => string.IsNullOrWhiteSpace(LocalizedTitle) ? Title : $"{Title} / {LocalizedTitle}";

    // Creators (ported from Director/Producer/Writer/Composer/Actors)
    public string Developer { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public string Designer { get; set; } = string.Empty;
    public string Composer { get; set; } = string.Empty;
    public string VoiceActors { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    // Publishing data and classification
    public int ReleaseYear { get; set; }
    public double PlaytimeHours { get; set; }
    public string Genre { get; set; } = string.Empty;
    public string Pegi { get; set; } = string.Empty;
    public string Platform { get; set; } = "PC";

    public string Url { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Comments { get; set; } = string.Empty;

    // Files / technical data (ported from FilePath/Video*/Audio*/Resolution/...)
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

    // Cover (AMC: Picture) — PNG/JPG bytes, null = none
    public byte[]? CoverImage { get; set; }
    public bool HasCover => CoverImage is { Length: > 0 };

    // Add-ons/DLC (AMC: Extras)
    public List<GameExtra> Extras { get; set; } = new();
    public int NbExtras => Extras.Count;

    // Custom fields (AMC: CustomFields) — Tag→Value dictionary
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

    /// <summary>Overwrites every field with deep copies of <paramref name="source"/>'s values (used by undo).</summary>
    public void CopyFrom(Game source)
    {
        foreach (var p in typeof(Game).GetProperties())
            if (p.CanRead && p.CanWrite) p.SetValue(this, p.GetValue(source));
        Extras = source.Extras.Select(e => e.Clone()).ToList();
        CustomValues = new Dictionary<string, string>(source.CustomValues, StringComparer.OrdinalIgnoreCase);
        CoverImage = source.CoverImage is null ? null : (byte[])source.CoverImage.Clone();
    }
}
