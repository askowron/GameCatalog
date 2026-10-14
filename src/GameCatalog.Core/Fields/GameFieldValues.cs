using System.Globalization;
using GameCatalog.Core.Models;

namespace GameCatalog.Core.Fields;

/// <summary>Reads a game field as display text by tag (find bar, columns, scripts, preview).</summary>
public static class GameFieldValues
{
    public static string Get(Game g, string tag)
    {
        var labels = CatalogLabels.Current;
        return tag switch
        {
            "Number" => g.Number.ToString(CultureInfo.InvariantCulture),
            "Checked" => g.Checked ? labels.Yes : labels.No,
            "ColorTag" => g.ColorTag.ToString(CultureInfo.InvariantCulture),
            "MediaLabel" => g.MediaLabel,
            "MediaType" => g.MediaType,
            "Store" => g.Store,
            "DateAdded" => Date(g.DateAdded),
            "Borrower" => g.Borrower,
            "DateFinished" => Date(g.DateFinished),
            "UserRating" => Rating(g.UserRating),
            "CriticRating" => Rating(g.CriticRating),
            "Title" => g.Title,
            "LocalizedTitle" => g.LocalizedTitle,
            "DisplayTitle" => g.DisplayTitle,
            "Developer" => g.Developer,
            "Publisher" => g.Publisher,
            "Designer" => g.Designer,
            "Composer" => g.Composer,
            "VoiceActors" => g.VoiceActors,
            "Country" => g.Country,
            "ReleaseYear" => g.ReleaseYear > 0 ? g.ReleaseYear.ToString(CultureInfo.InvariantCulture) : "",
            "PlaytimeHours" => g.PlaytimeHours > 0 ? g.PlaytimeHours.ToString("0.#", CultureInfo.CurrentCulture) : "",
            "Genre" => g.Genre,
            "Pegi" => g.Pegi,
            "URL" => g.Url,
            "Description" => g.Description,
            "Comments" => g.Comments,
            "InstallPath" => g.InstallPath,
            "Engine" => g.Engine,
            "Version" => g.Version,
            "GameMode" => g.GameMode,
            "Platform" => g.Platform,
            "Resolution" => g.Resolution,
            "Framerate" => g.Framerate,
            "AudioLanguages" => g.AudioLanguages,
            "Subtitles" => g.Subtitles,
            "SizeGb" => g.SizeGb,
            "DiskCount" => g.DiskCount > 0 ? g.DiskCount.ToString(CultureInfo.InvariantCulture) : "",
            "CompletionStatus" => labels.Status(g.CompletionStatus),
            "LastPlayed" => Date(g.LastPlayed),
            "NbExtras" => g.NbExtras.ToString(CultureInfo.InvariantCulture),
            _ => g.CustomValues.TryGetValue(tag, out var v) ? v : "",
        };
    }

    /// <summary>Ratings are stored as 0..100 (-1 = none) and shown as 0.0..10.0.</summary>
    public static string Rating(int rating) =>
        rating < 0 ? "" : (rating / 10.0).ToString("0.0", CultureInfo.CurrentCulture);

    private static string Date(DateTime? d) => d?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "";
}
