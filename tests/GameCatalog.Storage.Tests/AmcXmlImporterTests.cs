namespace GameCatalog.Storage.Tests;

public class AmcXmlImporterTests
{
    private static string Sample => Path.Combine(AppContext.BaseDirectory, "TestData", "amc-sample.xml");

    [Fact]
    public void Import_ReadsAttributesAndMapsMovieFieldsToGameFields()
    {
        var games = AmcXmlImporter.Import(Sample, startNumber: 100);

        Assert.Equal(2, games.Count);
        var g = games[0];
        Assert.Equal(100, g.Number);
        Assert.Equal("Blade Runner", g.Title);
        Assert.Equal("Łowca androidów", g.LocalizedTitle);
        Assert.Equal("Ridley Scott", g.Developer);
        Assert.Equal("Warner", g.Publisher);
        Assert.Equal("Fancher", g.Designer);
        Assert.Equal("Sci-Fi", g.Genre);
        Assert.Equal(1982, g.ReleaseYear);
        Assert.Equal(2.5, g.PlaytimeHours);      // 150 min → 2.5 h
        Assert.Equal(80, g.UserRating);
        Assert.Equal(72, g.CriticRating);
        Assert.Equal("Fizyczna", g.MediaType);
        Assert.Equal("XviD", g.Engine);
        Assert.Equal(new DateTime(2020, 5, 6), g.DateFinished);
        Assert.True(g.Checked);
    }

    [Fact]
    public void Import_MissingOriginalTitle_FallsBackToTranslatedTitle()
    {
        var g = AmcXmlImporter.Import(Sample, 1)[1];
        Assert.Equal("Tylko polski", g.Title);
        Assert.Equal(-1, g.UserRating);
        Assert.Equal(-1, g.CriticRating);
    }
}
