using GameCatalog.Core.Models;
using GameCatalog.Storage.Csv;

namespace GameCatalog.Storage.Tests;

public sealed class CsvTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"gc-test-{Guid.NewGuid():N}.csv");
    public void Dispose() { if (File.Exists(_path)) File.Delete(_path); }

    [Fact]
    public void ReadRecords_HandlesQuotesDelimitersAndNewlines()
    {
        var text = "a;b;c\n\"x;y\";\"he said \"\"hi\"\"\";\"line1\nline2\"\n";
        var rows = CsvReader.ReadRecords(new StringReader(text), ';').ToList();
        Assert.Equal(2, rows.Count);
        Assert.Equal(new[] { "x;y", "he said \"hi\"", "line1\nline2" }, rows[1]);
    }

    [Fact]
    public void ExportThenImport_PreservesTitleWithDelimiterAndQuotes()
    {
        var games = new[] { new Game { Number = 1, Title = "Warhammer 40,000; \"Dawn\"", Developer = "Relic", ReleaseYear = 2004, Genre = "RTS", Platform = "PC" } };
        CatalogTransfer.ExportCsv(games, _path, ';');

        var imported = CatalogTransfer.ImportCsv(_path, startNumber: 10, ';');

        var g = Assert.Single(imported);
        Assert.Equal(10, g.Number);
        Assert.Equal("Warhammer 40,000; \"Dawn\"", g.Title);
        Assert.Equal("Relic", g.Developer);
        Assert.Equal(2004, g.ReleaseYear);
        Assert.Equal("RTS", g.Genre);
    }

    [Fact]
    public void ImportCsv_SkipsBlankLinesAndShortRows()
    {
        File.WriteAllText(_path, "Number;Title\n\n1\n2;Quake\n");
        var g = Assert.Single(CatalogTransfer.ImportCsv(_path, 1, ';'));
        Assert.Equal("Quake", g.Title);
    }
}
