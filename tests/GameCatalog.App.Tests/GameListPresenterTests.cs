using GameCatalog.App.Services;
using GameCatalog.Core.Catalog;
using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;

namespace GameCatalog.App.Tests;

public class GameListPresenterTests
{
    private static GameListPresenter Create(PrefsStore? prefs = null)
    {
        CatalogLabels.Current = new InvariantCatalogLabels();
        var list = new GameList();
        list.Add(new Game { Title = "Doom 10", LocalizedTitle = "", Genre = "FPS" });
        list.Add(new Game { Title = "Doom 2", LocalizedTitle = "Zagłada 2", Genre = "FPS" });
        list.Add(new Game { Title = "Abe", Genre = "" });
        return new GameListPresenter(list, prefs ?? new PrefsStore { NaturalSort = true, TitleFormat = "Both", ShowGroupCount = true });
    }

    [Fact]
    public void CurrentView_SortsNaturallyAndRespectsFindOnly()
    {
        var p = Create();
        Assert.Equal(new[] { "Abe", "Doom 2", "Doom 10" }, p.CurrentView().Select(g => g.Title));
        p.State.FindOnly = new HashSet<int> { 1 };
        Assert.Equal("Doom 10", Assert.Single(p.CurrentView()).Title);
    }

    [Fact]
    public void Build_GroupsWithCaptionsAndCounts()
    {
        var p = Create();
        p.State.GroupTag = "Genre";
        var vm = p.Build();
        Assert.Equal(3, vm.Total);
        Assert.Contains(vm.Groups, g => g.Key == "" && g.Caption == "(none) (1)");
        Assert.Contains(vm.Groups, g => g.Key == "FPS" && g.Caption == "FPS (2)");
        Assert.All(vm.Rows, r => Assert.Equal(p.State.Columns.Count, r.Cells.Count));
    }

    [Theory]
    [InlineData("Both", "Doom 2 / Zagłada 2")]
    [InlineData("Original", "Doom 2")]
    [InlineData("Localized", "Zagłada 2")]
    public void FormatTitle_FollowsPreference(string format, string expected)
    {
        var p = Create(new PrefsStore { TitleFormat = format });
        var g = p.CurrentView().Single(x => x.Title == "Doom 2");
        Assert.Equal(expected, p.FormatTitle(g));
    }

    [Fact]
    public void Matches_WholeFieldAndReverse()
    {
        var g = new Game { Title = "Doom" };
        Assert.True(GameListPresenter.Matches(g, new FindCriteria("Title", "oo", false, false)));
        Assert.False(GameListPresenter.Matches(g, new FindCriteria("Title", "oo", true, false)));
        Assert.True(GameListPresenter.Matches(g, new FindCriteria("Title", "doom", true, false)));
        Assert.True(GameListPresenter.Matches(g, new FindCriteria("Title", "xyz", false, true)));
    }

    [Fact]
    public void FindNext_WrapsAround()
    {
        var p = Create();
        var c = new FindCriteria("Title", "Doom", false, false);
        var first = p.FindNext(c, currentNumber: null)!;
        var second = p.FindNext(c, first.Number)!;
        var third = p.FindNext(c, second.Number)!;
        Assert.Equal("Doom 2", first.Title);
        Assert.Equal("Doom 10", second.Title);
        Assert.Equal("Doom 2", third.Title);
    }

    [Fact]
    public void ToggleColumn_NeverRemovesTheLastColumn()
    {
        var p = Create();
        p.ToggleColumn("Genre", true);
        Assert.Contains(p.State.Columns, c => c.Tag == "Genre");
        p.ToggleColumn("Genre", false);
        p.ToggleColumn("Title", false);
        p.ToggleColumn("Number", false);
        Assert.Single(p.State.Columns);
    }
}
