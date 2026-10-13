using GameCatalog.Core.Models;

namespace GameCatalog.Core.Catalog;

/// <summary>Odczyt wartości pól gry po tagu (szukanie, sortowanie, skrypty, podgląd).</summary>
public static class GameFieldValues
{
    public static string Get(Game g, string tag) => tag switch
    {
        "Number" => g.Number.ToString(),
        "Checked" => g.Checked ? "Tak" : "Nie",
        "ColorTag" => g.ColorTag.ToString(),
        "MediaLabel" => g.MediaLabel,
        "MediaType" => g.MediaType,
        "Store" => g.Store,
        "DateAdded" => g.DateAdded?.ToString("yyyy-MM-dd") ?? "",
        "Borrower" => g.Borrower,
        "DateFinished" => g.DateFinished?.ToString("yyyy-MM-dd") ?? "",
        "UserRating" => g.UserRating < 0 ? "" : (g.UserRating / 10.0).ToString("0.0"),
        "CriticRating" => g.CriticRating < 0 ? "" : (g.CriticRating / 10.0).ToString("0.0"),
        "Title" => g.Title,
        "LocalizedTitle" => g.LocalizedTitle,
        "DisplayTitle" => g.DisplayTitle,
        "Developer" => g.Developer,
        "Publisher" => g.Publisher,
        "Designer" => g.Designer,
        "Composer" => g.Composer,
        "VoiceActors" => g.VoiceActors,
        "Country" => g.Country,
        "ReleaseYear" => g.ReleaseYear > 0 ? g.ReleaseYear.ToString() : "",
        "PlaytimeHours" => g.PlaytimeHours > 0 ? g.PlaytimeHours.ToString("0.#") : "",
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
        "DiskCount" => g.DiskCount > 0 ? g.DiskCount.ToString() : "",
        "CompletionStatus" => GameCatalogModel.StatusLabel(g.CompletionStatus),
        "LastPlayed" => g.LastPlayed?.ToString("yyyy-MM-dd") ?? "",
        "NbExtras" => g.NbExtras.ToString(),
        _ => g.CustomValues.TryGetValue(tag, out var v) ? v : "",
    };
}
