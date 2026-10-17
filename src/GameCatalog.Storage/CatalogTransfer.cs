using System.Text;
using GameCatalog.Core.Catalog;
using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;
using GameCatalog.Storage.Csv;

namespace GameCatalog.Storage;

/// <summary>CSV export/import + HTML export (counterpart of AMC export.pas), game edition.</summary>
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
                CatalogLabels.Current.Status(g.CompletionStatus),
                GameFieldValues.Rating(g.UserRating),
                GameFieldValues.Rating(g.CriticRating),
                g.PlaytimeHours, Csv(g.Store, delimiter), Csv(g.Engine, delimiter), Csv(g.Description, delimiter)));
        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
    }

    public static List<Game> ImportCsv(string path, int startNumber, char delimiter = ';')
    {
        var result = new List<Game>();
        using var reader = new StreamReader(path, Encoding.UTF8);
        int n = startNumber;
        foreach (var p in CsvReader.ReadRecords(reader, delimiter).Skip(1))
        {
            if (p.Length < 2 || string.IsNullOrWhiteSpace(p[1])) continue;
            string At(int i) => i < p.Length ? p[i].Trim() : "";
            result.Add(new Game
            {
                Number = n++,
                Title = At(1),
                LocalizedTitle = At(2),
                Developer = At(3),
                Publisher = At(4),
                ReleaseYear = int.TryParse(At(5), out var y) ? y : 0,
                Genre = At(6),
                Platform = At(7).Length > 0 ? At(7) : "PC",
                DateAdded = DateTime.Today,
            });
        }
        return result;
    }

    /// <summary>Simple HTML table; <paramref name="headers"/> are the 9 column captions in the UI language.</summary>
    public static void ExportHtml(IEnumerable<Game> games, string path, string title, IReadOnlyList<string> headers)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html lang='pl'><head><meta charset='utf-8'>");
        sb.AppendLine($"<title>{System.Net.WebUtility.HtmlEncode(title)}</title>");
        sb.AppendLine("<style>body{font-family:Segoe UI,Arial}table{border-collapse:collapse;width:100%}th,td{border:1px solid #ccc;padding:6px}th{background:#eee}</style></head><body>");
        sb.AppendLine($"<h1>{System.Net.WebUtility.HtmlEncode(title)}</h1>");
        sb.AppendLine("<table><tr>" + string.Concat(headers.Select(h => $"<th>{System.Net.WebUtility.HtmlEncode(h)}</th>")) + "</tr>");
        foreach (var g in games)
            sb.AppendLine($"<tr><td>{g.Number}</td><td>{System.Net.WebUtility.HtmlEncode(g.Title)}</td><td>{System.Net.WebUtility.HtmlEncode(g.Developer)}</td><td>{g.ReleaseYear}</td><td>{System.Net.WebUtility.HtmlEncode(g.Genre)}</td><td>{System.Net.WebUtility.HtmlEncode(g.Platform)}</td><td>{CatalogLabels.Current.Status(g.CompletionStatus)}</td><td>{GameFieldValues.Rating(g.UserRating)}</td><td>{g.PlaytimeHours}</td></tr>");
        sb.AppendLine("</table></body></html>");
        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
    }

    /// <summary>Import from a folder: every subfolder/file matching the patterns becomes a game entry (counterpart of AMC import2).</summary>
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
