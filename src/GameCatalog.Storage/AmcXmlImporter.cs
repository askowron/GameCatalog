using System.Xml.Linq;
using GameCatalog.Core.Models;

namespace GameCatalog.Storage;

/// <summary>
/// Import starego katalogu Ant Movie Catalog (.xml) z mapowaniem pól filmowych na growe.
/// Director→Developer, Producer→Publisher, Writer→Designer, Actors→VoiceActors,
/// Category→Genre, Certification→PEGI, Length(min)→PlaytimeHours(h), itd.
/// </summary>
public static class AmcXmlImporter
{
    public static List<Game> Import(string xmlPath, int startNumber)
    {
        var doc = XDocument.Load(xmlPath);
        var result = new List<Game>();
        int n = startNumber;
        foreach (var m in doc.Descendants("Movie"))
        {
            string V(string tag) => m.Element(tag)?.Value ?? "";
            int Int(string tag) => int.TryParse(V(tag), out var i) ? i : 0;
            var g = new Game
            {
                Number = n++,
                MediaLabel = V("MediaLabel"),
                MediaType = MapMediaType(V("MediaType")),
                Store = V("Source"),
                Borrower = V("Borrower"),
                UserRating = Int("UserRating"),
                CriticRating = Int("Rating"),
                Title = string.IsNullOrWhiteSpace(V("OriginalTitle")) ? V("TranslatedTitle") : V("OriginalTitle"),
                LocalizedTitle = V("TranslatedTitle"),
                Developer = V("Director"),
                Publisher = V("Producer"),
                Designer = V("Writer"),
                Composer = V("Composer"),
                VoiceActors = V("Actors"),
                Country = V("Country"),
                ReleaseYear = Int("Year"),
                PlaytimeHours = Math.Round(Int("Length") / 60.0, 1),
                Genre = V("Category"),
                Pegi = V("Certification"),
                Url = V("URL"),
                Description = V("Description"),
                Comments = V("Comments"),
                InstallPath = V("FilePath"),
                Engine = V("VideoFormat"),
                GameMode = "Single-player",
                AudioLanguages = V("Languages"),
                Subtitles = V("Subtitles"),
                SizeGb = V("Size"),
                DateAdded = DateTime.Today,
                Platform = "PC",
            };
            if (DateTime.TryParse(V("DateWatched"), out var dw)) g.DateFinished = dw;
            result.Add(g);
        }
        return result;
    }

    private static string MapMediaType(string mt) =>
        mt.Contains("DVD", StringComparison.OrdinalIgnoreCase) ? "Fizyczna" : "Cyfrowa";
}
