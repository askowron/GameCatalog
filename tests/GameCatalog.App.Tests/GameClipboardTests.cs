using GameCatalog.App.Services;
using GameCatalog.Core.Models;

namespace GameCatalog.App.Tests;

public class GameClipboardTests
{
    [Fact]
    public void RoundTrip_ResetsNumberAndKeepsData()
    {
        var g = new Game { Number = 7, Title = "Quake", CoverImage = new byte[] { 9 } };
        g.Extras.Add(new GameExtra { Title = "DLC" });
        var copy = GameClipboard.TryDeserialize(GameClipboard.Serialize(g))!;
        Assert.Equal(0, copy.Number);
        Assert.Equal("Quake", copy.Title);
        Assert.Equal(new byte[] { 9 }, copy.CoverImage);
        Assert.Equal("DLC", Assert.Single(copy.Extras).Title);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("hello")]
    [InlineData("GAMECATALOG1:{not json")]
    [InlineData("GAMECATALOG1:{\"Title\":\"\"}")]
    public void TryDeserialize_RejectsForeignOrEmptyContent(string? text)
        => Assert.Null(GameClipboard.TryDeserialize(text));
}
