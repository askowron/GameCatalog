using GameCatalog.Core.Catalog;
using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;

namespace GameCatalog.Core.Tests;

public class GameListTests
{
    private static GameList Sample()
    {
        var list = new GameList();
        list.Add(new Game { Title = "Doom 2", Genre = "FPS", ReleaseYear = 1994 });
        list.Add(new Game { Title = "Doom 10", Genre = "", ReleaseYear = 0 });
        list.Add(new Game { Title = "Abe", Genre = "Platformer", CompletionStatus = CompletionStatus.Completed });
        return list;
    }

    [Fact]
    public void Add_AssignsSequentialNumbersAndDateAdded()
    {
        var list = Sample();
        Assert.Equal(new[] { 1, 2, 3 }, list.Games.Select(g => g.Number));
        Assert.All(list.Games, g => Assert.Equal(DateTime.Today, g.DateAdded));
        Assert.Equal(4, list.NextNumber());
    }

    [Fact]
    public void Add_KeepsExplicitNumber()
    {
        var list = new GameList();
        list.Add(new Game { Number = 42, Title = "X" });
        Assert.Equal(42, list.Find(42)!.Number);
        Assert.Equal(43, list.NextNumber());
    }

    [Fact]
    public void ReplaceAll_WithQueryOverOwnGames_DoesNotLoseItems()
    {
        var list = Sample();
        list.ReplaceAll(list.Games.OrderByDescending(g => g.Number));
        Assert.Equal(new[] { 3, 2, 1 }, list.Games.Select(g => g.Number));
    }

    [Fact]
    public void Sort_Natural_OrdersNumbersByValue()
    {
        var sorted = GameList.Sort(Sample().Games, "Title", ascending: true, natural: true).Select(g => g.Title);
        Assert.Equal(new[] { "Abe", "Doom 2", "Doom 10" }, sorted);
    }

    [Fact]
    public void Sort_Descending_ByYear()
    {
        var sorted = GameList.Sort(Sample().Games, "ReleaseYear", ascending: false).Select(g => g.ReleaseYear);
        Assert.Equal(new[] { 1994, 0, 0 }, sorted);
    }

    [Fact]
    public void GroupBy_EmptyValuesUseEmptyKey_StatusUsesEnumName()
    {
        var byGenre = GameList.GroupBy(Sample().Games, "Genre").Select(g => g.Key).ToList();
        Assert.Contains(string.Empty, byGenre);
        var byStatus = GameList.GroupBy(Sample().Games, "CompletionStatus").Select(g => g.Key).ToList();
        Assert.Contains("Completed", byStatus);
        Assert.Contains("Backlog", byStatus);
    }

    [Fact]
    public void GroupCaption_TranslatesEmptyAndStatus()
    {
        var labels = new InvariantCatalogLabels();
        Assert.Equal(labels.NoValue, GameList.GroupCaption("Genre", "", labels));
        Assert.Equal("Completed", GameList.GroupCaption("CompletionStatus", "Completed", labels));
        Assert.Equal("FPS", GameList.GroupCaption("Genre", "FPS", labels));
    }

    [Fact]
    public void RemoveByInstance_KeepsDuplicateNumber()
    {
        var list = new GameList();
        var a = new Game { Number = 1, Title = "A" };
        var b = new Game { Number = 1, Title = "B" };
        list.Add(a);
        list.Add(b);
        Assert.True(list.Remove(a));
        Assert.Same(b, Assert.Single(list.Games));
    }

    [Fact]
    public void CopyFrom_OverwritesFieldsWithDeepCopies()
    {
        var source = new Game { Number = 3, Title = "S", UserRating = 50, CoverImage = new byte[] { 1 } };
        source.Extras.Add(new GameExtra { Title = "E" });
        source.CustomValues["K"] = "V";
        var target = new Game { Title = "T" };

        target.CopyFrom(source);
        source.Extras[0].Title = "changed";
        source.CoverImage[0] = 9;
        source.CustomValues["K"] = "changed";

        Assert.Equal(3, target.Number);
        Assert.Equal("S", target.Title);
        Assert.Equal(50, target.UserRating);
        Assert.Equal("E", target.Extras[0].Title);
        Assert.Equal(1, target.CoverImage![0]);
        Assert.Equal("V", target.CustomValues["K"]);
    }
}
