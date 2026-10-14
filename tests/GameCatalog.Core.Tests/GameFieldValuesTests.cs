using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;

namespace GameCatalog.Core.Tests;

public class GameFieldValuesTests
{
    [Fact]
    public void Get_FormatsRatingsYearsAndLabels()
    {
        CatalogLabels.Current = new InvariantCatalogLabels();
        var g = new Game { UserRating = 85, CriticRating = -1, ReleaseYear = 0, Checked = true,
                           CompletionStatus = CompletionStatus.Playing };
        Assert.Equal("8.5", GameFieldValues.Get(g, "UserRating").Replace(',', '.'));
        Assert.Equal("", GameFieldValues.Get(g, "CriticRating"));
        Assert.Equal("", GameFieldValues.Get(g, "ReleaseYear"));
        Assert.Equal("Yes", GameFieldValues.Get(g, "Checked"));
        Assert.Equal("Playing", GameFieldValues.Get(g, "CompletionStatus"));
    }

    [Fact]
    public void Get_UnknownTag_ReadsCustomValue()
    {
        var g = new Game();
        g.CustomValues["Disc"] = "Blue";
        Assert.Equal("Blue", GameFieldValues.Get(g, "disc"));
        Assert.Equal("", GameFieldValues.Get(g, "Missing"));
    }

    [Fact]
    public void EveryFieldTag_IsReadable()
    {
        var g = new Game();
        foreach (var f in GameFields.All) _ = GameFieldValues.Get(g, f.Tag);
    }

    [Fact]
    public void Searchable_ExcludesVirtualFields()
    {
        Assert.DoesNotContain(GameFields.Searchable, f => f.Tag is "DisplayTitle" or "NbExtras");
        Assert.Contains(GameFields.Searchable, f => f.Tag == "Title");
    }
}
