using GameCatalog.UI.Controls;

namespace GameCatalog.UI.Tests;

public class FindBarTests
{
    [Fact]
    public void Criteria_ReflectControls_AndChangesRaiseEvent()
    {
        using var bar = new FindBar();
        bar.SetFields(new[] { ("Title", "Tytuł"), ("Genre", "Gatunek") }, "Genre");
        int changed = 0;
        bar.CriteriaChanged += (_, _) => changed++;

        bar.SearchText = "rpg";
        bar.WholeField = true;

        Assert.Equal("Genre", bar.FieldTag);
        Assert.Equal("rpg", bar.SearchText);
        Assert.True(bar.WholeField);
        Assert.False(bar.Reverse);
        Assert.True(changed >= 2);
    }
}
