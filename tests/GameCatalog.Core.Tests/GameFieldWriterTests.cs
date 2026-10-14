using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;

namespace GameCatalog.Core.Tests;

public class GameFieldWriterTests
{
    [Theory]
    [InlineData("8.5", 85)]
    [InlineData("8,5", 85)]
    [InlineData("10", 100)]
    public void Set_Rating_AcceptsBothDecimalSeparators(string value, int expected)
    {
        var g = new Game();
        Assert.True(GameFieldWriter.Set(g, "UserRating", value));
        Assert.Equal(expected, g.UserRating);
    }

    [Fact]
    public void Set_InvalidNumber_ReturnsFalseAndKeepsValue()
    {
        var g = new Game { ReleaseYear = 2000 };
        Assert.False(GameFieldWriter.Set(g, "ReleaseYear", "soon"));
        Assert.Equal(2000, g.ReleaseYear);
    }

    [Fact]
    public void Set_StringDatesStatusChecked()
    {
        CatalogLabels.Current = new InvariantCatalogLabels();
        var g = new Game();
        Assert.True(GameFieldWriter.Set(g, "Developer", "id Software"));
        Assert.True(GameFieldWriter.Set(g, "LastPlayed", "2024-05-01"));
        Assert.True(GameFieldWriter.Set(g, "CompletionStatus", "Completed"));
        Assert.True(GameFieldWriter.Set(g, "Checked", "tak"));
        Assert.True(GameFieldWriter.Set(g, "PlaytimeHours", "12,5"));
        Assert.Equal("id Software", g.Developer);
        Assert.Equal(new DateTime(2024, 5, 1), g.LastPlayed);
        Assert.Equal(CompletionStatus.Completed, g.CompletionStatus);
        Assert.True(g.Checked);
        Assert.Equal(12.5, g.PlaytimeHours);
    }

    [Fact]
    public void Set_CustomPrefixAndUnknownTags_GoToCustomValues_EmptyRemoves()
    {
        var g = new Game();
        GameFieldWriter.Set(g, "custom:Disc", "Blue");
        GameFieldWriter.Set(g, "Mood", "Calm");
        Assert.Equal("Blue", g.CustomValues["Disc"]);
        Assert.Equal("Calm", g.CustomValues["Mood"]);
        GameFieldWriter.Set(g, "custom:Disc", "");
        Assert.False(g.CustomValues.ContainsKey("Disc"));
    }

    [Fact]
    public void Set_VirtualFields_AreIgnored()
    {
        var g = new Game { Title = "A" };
        Assert.False(GameFieldWriter.Set(g, "DisplayTitle", "B"));
        Assert.False(GameFieldWriter.Set(g, "NbExtras", "3"));
        Assert.Equal("A", g.Title);
        Assert.Empty(g.CustomValues);
    }
}
