using System.Globalization;
using System.Xml.Linq;
using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;

namespace GameCatalog.Storage;

/// <summary>
/// Imports a legacy Ant Movie Catalog .xml file, mapping movie fields to game fields:
/// Director→Developer, Producer→Publisher, Writer→Designer, Actors→VoiceActors,
/// Category→Genre, Certification→PEGI, Length (min)→PlaytimeHours (h), etc.
/// AMC stores field values as attributes of &lt;Movie&gt; (movieclass.pas TMovie.SaveToXML).
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
            string V(string tag) => m.Attribute(tag)?.Value ?? m.Element(tag)?.Value ?? "";
            int Int(string tag, int fallback) =>
                int.TryParse(V(tag), NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) ? i : fallback;
            var g = new Game
            {
                Number = n++,
                Checked = V("Checked").Equals("True", StringComparison.OrdinalIgnoreCase),
                MediaLabel = V("MediaLabel"),
                MediaType = MapMediaType(V("MediaType")),
                Store = V("Source"),
                Borrower = V("Borrower"),
                UserRating = Int("UserRating", -1),
                CriticRating = Int("Rating", -1),
                Title = string.IsNullOrWhiteSpace(V("OriginalTitle")) ? V("TranslatedTitle") : V("OriginalTitle"),
                LocalizedTitle = V("TranslatedTitle"),
                Developer = V("Director"),
                Publisher = V("Producer"),
                Designer = V("Writer"),
                Composer = V("Composer"),
                VoiceActors = V("Actors"),
                Country = V("Country"),
                ReleaseYear = Int("Year", 0),
                PlaytimeHours = Math.Round(Int("Length", 0) / 60.0, 1),
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
            if (DateTime.TryParse(V("DateWatched"), CultureInfo.InvariantCulture, DateTimeStyles.None, out var dw))
                g.DateFinished = dw;
            result.Add(g);
        }
        return result;
    }

    private static string MapMediaType(string mediaType) =>
        mediaType.Contains("DVD", StringComparison.OrdinalIgnoreCase) ? "Fizyczna" : "Cyfrowa";
}
