using System.Text;
using GameCatalog.Core.Catalog;
using GameCatalog.Core.Models;

namespace GameCatalog.Storage;

/// <summary>Eksport/import CSV + eksport HTML (odpowiednik AMC export.pas), w wersji growej.</summary>
public static class CatalogTransfer
{
    private static string Csv(string s, char delimiter)
    {
        s ??= "";
        return s.Contains('"') || s.Contains(delimiter) || s.Contains('\n')
            ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;
    }

    public static void ExportCsv(IEnumerable<Game> games, string path, char delimiter = ';')
    {
        var sb = new StringBuilder();
        sb.AppendLine("Number;Title;LocalizedTitle;Developer;Publisher;ReleaseYear;Genre;Platform;CompletionStatus;UserRating;CriticRating;PlaytimeHours;Store;Engine;Description".Replace(';', delimiter));
        foreach (var g in games)
            sb.AppendLine(string.Join(delimiter, g.Number, Csv(g.Title, delimiter), Csv(g.LocalizedTitle, delimiter), Csv(g.Developer, delimiter),
                Csv(g.Publisher, delimiter), g.ReleaseYear, Csv(g.Genre, delimiter), Csv(g.Platform, delimiter),
                GameCatalogModel.StatusLabel(g.CompletionStatus),
                g.UserRating < 0 ? "" : (g.UserRating / 10.0).ToString("0.0"),
                g.CriticRating < 0 ? "" : (g.CriticRating / 10.0).ToString("0.0"),
                g.PlaytimeHours, Csv(g.Store, delimiter), Csv(g.Engine, delimiter), Csv(g.Description, delimiter)));
        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
    }

    public static List<Game> ImportCsv(string path, int startNumber, char delimiter = ';')
    {
        var result = new List<Game>();
        var lines = File.ReadAllLines(path, Encoding.UTF8);
        int n = startNumber;
        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var p = line.Split(delimiter);
            if (p.Length < 2) continue;
            result.Add(new Game
            {
                Number = n++,
                Title = p.ElementAtOrDefault(1)?.Trim('"') ?? "",
                Developer = p.ElementAtOrDefault(3)?.Trim('"') ?? "",
                Publisher = p.ElementAtOrDefault(4)?.Trim('"') ?? "",
                ReleaseYear = int.TryParse(p.ElementAtOrDefault(5), out var y) ? y : 0,
                Genre = p.ElementAtOrDefault(6)?.Trim('"') ?? "",
                Platform = p.ElementAtOrDefault(7)?.Trim('"') ?? "PC",
                DateAdded = DateTime.Today,
            });
        }
        return result;
    }

    public static void ExportHtml(IEnumerable<Game> games, string path, string title = "Katalog gier")
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html lang='pl'><head><meta charset='utf-8'>");
        sb.AppendLine($"<title>{System.Net.WebUtility.HtmlEncode(title)}</title>");
        sb.AppendLine("<style>body{font-family:Segoe UI,Arial}table{border-collapse:collapse;width:100%}th,td{border:1px solid #ccc;padding:6px}th{background:#eee}</style></head><body>");
        sb.AppendLine($"<h1>{System.Net.WebUtility.HtmlEncode(title)}</h1>");
        sb.AppendLine("<table><tr><th>#</th><th>Tytuł</th><th>Developer</th><th>Rok</th><th>Gatunek</th><th>Platforma</th><th>Status</th><th>Ocena</th><th>Czas (h)</th></tr>");
        foreach (var g in games)
            sb.AppendLine($"<tr><td>{g.Number}</td><td>{System.Net.WebUtility.HtmlEncode(g.Title)}</td><td>{System.Net.WebUtility.HtmlEncode(g.Developer)}</td><td>{g.ReleaseYear}</td><td>{System.Net.WebUtility.HtmlEncode(g.Genre)}</td><td>{System.Net.WebUtility.HtmlEncode(g.Platform)}</td><td>{GameCatalogModel.StatusLabel(g.CompletionStatus)}</td><td>{(g.UserRating < 0 ? "–" : (g.UserRating / 10.0).ToString("0.0"))}</td><td>{g.PlaytimeHours}</td></tr>");
        sb.AppendLine("</table></body></html>");
        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
    }

    /// <summary>Import z folderu: każdy podfolder/plik pasujący do wzorców tworzy wpis gry (odpowiednik AMC import2).</summary>
    public static List<Game> ImportFromFolder(string folder, int startNumber, string? patterns = null)
    {
        var pats = (patterns ?? "*.exe").Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (pats.Length == 0) pats = new[] { "*.exe" };
        var result = new List<Game>();
        int n = startNumber;
        foreach (var dir in Directory.GetDirectories(folder))
        {
            var exe = pats.SelectMany(p => { try { return Directory.GetFiles(dir, p, SearchOption.AllDirectories); } catch { return Array.Empty<string>(); } }).FirstOrDefault();
            result.Add(new Game
            {
                Number = n++,
                Title = Path.GetFileName(dir),
                InstallPath = exe ?? dir,
                DateAdded = DateTime.Today,
                Platform = "PC",
            });
        }
        foreach (var f in pats.SelectMany(p => { try { return Directory.GetFiles(folder, p, SearchOption.TopDirectoryOnly); } catch { return Array.Empty<string>(); } }).Distinct())
        {
            result.Add(new Game
            {
                Number = n++,
                Title = Path.GetFileNameWithoutExtension(f),
                InstallPath = f,
                DateAdded = DateTime.Today,
                Platform = "PC",
            });
        }
        return result;
    }
}
