using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;

namespace GameCatalog.App.Services;

/// <summary>
/// HTML templates of the game card — counterpart of the AMC templates (HTMLViewer + templates).
/// Placeholders: {Title}, {LocalizedTitle}, {Developer}, {Publisher}, {Designer},
/// {Composer}, {VoiceActors}, {Country}, {ReleaseYear}, {PlaytimeHours}, {Genre},
/// {Pegi}, {Platform}, {Store}, {Engine}, {GameMode}, {Status}, {UserRating},
/// {CriticRating}, {Url}, {Description}, {Comments}, {Number}, {Cover}, {Extras}.
/// </summary>
public static class HtmlTemplates
{
    public static readonly IReadOnlyList<string> Names = new[] { "Szczegółowy", "Kompaktowy" };

    public static string Default(string name) => name == "Kompaktowy" ? Compact : Detailed;

    public const string Detailed =
        "<html><head><meta charset='utf-8'></head><body style='font-family:Segoe UI,Arial;font-size:13px;background:#fff'>\n" +
        "<h2 style='margin:0'>{Title}</h2>\n" +
        "<i>{LocalizedTitle}</i><br>\n" +
        "{Cover}\n" +
        "<table cellpadding='3'>\n" +
        "<tr><td><b>Developer</b></td><td>{Developer}</td></tr>\n" +
        "<tr><td><b>Wydawca</b></td><td>{Publisher}</td></tr>\n" +
        "<tr><td><b>Rok wydania</b></td><td>{ReleaseYear}</td></tr>\n" +
        "<tr><td><b>Gatunek</b></td><td>{Genre}</td></tr>\n" +
        "<tr><td><b>Platforma</b></td><td>{Platform}</td></tr>\n" +
        "<tr><td><b>Status</b></td><td>{Status}</td></tr>\n" +
        "<tr><td><b>Czas gry</b></td><td>{PlaytimeHours}</td></tr>\n" +
        "<tr><td><b>Ocena własna</b></td><td>{UserRating}</td></tr>\n" +
        "<tr><td><b>Ocena recenzentów</b></td><td>{CriticRating}</td></tr>\n" +
        "<tr><td><b>Sklep</b></td><td>{Store}</td></tr>\n" +
        "<tr><td><b>Silnik</b></td><td>{Engine}</td></tr>\n" +
        "</table>\n" +
        "<p>{Description}</p>\n" +
        "<b>Dodatki / DLC:</b>{Extras}\n" +
        "</body></html>\n";

    public const string Compact =
        "<html><head><meta charset='utf-8'></head><body style='font-family:Segoe UI,Arial;font-size:12px;background:#fff'>\n" +
        "<b>{Title}</b> ({ReleaseYear}) — {Genre} / {Platform} — {Status}<br>\n" +
        "{Developer} / {Publisher} — ocena: {UserRating}<br>\n" +
        "{Description}\n" +
        "</body></html>\n";

    public static string CustomPath(string name)
    {
        return Path.Combine(AppPaths.TemplatesDirectory, name + ".html");
    }

    public static string Load(string name)
    {
        var p = CustomPath(name);
        if (File.Exists(p))
        {
            try { return File.ReadAllText(p); } catch { }
        }
        return Default(name);
    }

    public static string Expand(string template, Game g, string? coverUri)
    {
        string H(string? s) => System.Net.WebUtility.HtmlEncode(s ?? "");
        var extras = "";
        if (g.Extras.Count > 0)
        {
            extras = "<ul>" + string.Join("", g.Extras.Select(e =>
                $"<li>{H(e.Title)} <i>[{H(e.Category)}]</i></li>")) + "</ul>";
        }
        string cover = coverUri is null ? "" :
            $"<img src='{coverUri}' style='max-width:220px;float:right;margin:0 0 8px 8px'>";
        string desc = H(g.Description).Replace("\n", "<br>");
        return template
            .Replace("{Title}", H(g.Title))
            .Replace("{LocalizedTitle}", H(g.LocalizedTitle))
            .Replace("{Developer}", H(g.Developer))
            .Replace("{Publisher}", H(g.Publisher))
            .Replace("{Designer}", H(g.Designer))
            .Replace("{Composer}", H(g.Composer))
            .Replace("{VoiceActors}", H(g.VoiceActors))
            .Replace("{Country}", H(g.Country))
            .Replace("{ReleaseYear}", g.ReleaseYear > 0 ? g.ReleaseYear.ToString() : "")
            .Replace("{PlaytimeHours}", g.PlaytimeHours > 0 ? $"{g.PlaytimeHours:0.#} h" : "")
            .Replace("{Genre}", H(g.Genre))
            .Replace("{Pegi}", H(g.Pegi))
            .Replace("{Platform}", H(g.Platform))
            .Replace("{Store}", H(g.Store))
            .Replace("{Engine}", H(g.Engine))
            .Replace("{GameMode}", H(g.GameMode))
            .Replace("{Status}", H(CatalogLabels.Current.Status(g.CompletionStatus)))
            .Replace("{UserRating}", GameFieldValues.Rating(g.UserRating))
            .Replace("{CriticRating}", GameFieldValues.Rating(g.CriticRating))
            .Replace("{Url}", H(g.Url))
            .Replace("{Description}", desc)
            .Replace("{Comments}", H(g.Comments))
            .Replace("{Number}", g.Number.ToString())
            .Replace("{Cover}", cover)
            .Replace("{Extras}", extras);
    }
}
