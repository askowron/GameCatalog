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

    [Fact]
    public void SetFields_RaisesCriteriaChangedAtMostOnce_AndOnlyWhenFieldChanges()
    {
        using var bar = new FindBar();
        var fields = new[] { ("Title", "Tytuł"), ("Genre", "Gatunek") };
        int changed = 0;
        bar.CriteriaChanged += (_, _) => changed++;

        bar.SetFields(fields, "Genre");   // Title (default) -> Genre
        Assert.Equal(1, changed);

        bar.SetFields(fields, "Genre");   // refill, same field
        Assert.Equal(1, changed);
        Assert.Equal("Genre", bar.FieldTag);
    }
}
