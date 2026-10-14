using GameCatalog.UI.Controls;

namespace GameCatalog.UI.Tests;

public class GameListViewTests
{
    private static GameListView Create()
    {
        var lv = new GameListView();
        lv.SetColumns(new[] { ("Number", 40), ("Title", 200) }, tag => tag == "Title" ? "Tytuł" : "#");
        lv.Display(
            new[]
            {
                new GameListItem(1, true, 0, new[] { "1", "Doom" }, "FPS"),
                new GameListItem(2, false, 1, new[] { "2", "Abe" }, ""),
            },
            new[] { new GameListGroup("FPS", "FPS (1)"), new GameListGroup("", "(brak) (1)") });
        return lv;
    }

    [Fact]
    public void Display_FillsItemsGroupsAndColumns()
    {
        using var lv = Create();
        Assert.Equal(2, lv.Items.Count);
        Assert.Equal(2, lv.Groups.Count);
        Assert.Equal("Tytuł", lv.Columns[1].Text);
        Assert.Equal("FPS (1)", lv.Items[0].Group!.Header);
        Assert.True(lv.Items[0].Checked);
        Assert.Equal(ColorTags.ToColor(1), lv.Items[1].BackColor);
    }

    [Fact]
    public void SelectNumber_SelectsAndRaisesEvent()
    {
        using var lv = Create();
        lv.CreateControl();   // selection events need a window handle
        int raised = 0;
        lv.SelectedGameChanged += (_, _) => raised++;
        Assert.True(lv.SelectNumber(2));
        Assert.Equal(2, lv.SelectedNumber);
        Assert.False(lv.SelectNumber(99));
        Assert.True(raised >= 1);
    }

    [Fact]
    public void DisplayAndUpdateItem_DoNotRaiseCheckedEvents()
    {
        using var lv = new GameListView();
        lv.SetColumns(new[] { ("Number", 40) }, t => t);
        lv.CreateControl();   // ItemChecked only fires with a window handle
        int raised = 0;
        lv.GameCheckedChanged += (_, _) => raised++;
        lv.Display(new[] { new GameListItem(1, true, 0, new[] { "1" }, "") }, Array.Empty<GameListGroup>());
        lv.UpdateItem(new GameListItem(1, false, 0, new[] { "1" }, ""));
        Assert.Equal(0, raised);
    }

    [Fact]
    public void Display_RaisesSelectionEventOnlyWhenSelectedGameChanges()
    {
        using var lv = Create();
        lv.CreateControl();
        Assert.True(lv.SelectNumber(2));
        int raised = 0;
        lv.SelectedGameChanged += (_, _) => raised++;
        var same = new[] { new GameListItem(1, true, 0, new[] { "1", "Doom" }, "FPS"), new GameListItem(2, false, 1, new[] { "2", "Abe" }, "") };

        lv.Display(same, Array.Empty<GameListGroup>());
        Assert.Equal(0, raised);
        Assert.Equal(2, lv.SelectedNumber);

        lv.Display(same.Take(1).ToArray(), Array.Empty<GameListGroup>());
        Assert.Equal(1, raised);
        Assert.Null(lv.SelectedNumber);
    }

    [Fact]
    public void UpdateItem_ReplacesCellsOfMatchingNumber()
    {
        using var lv = Create();
        lv.UpdateItem(new GameListItem(1, false, 0, new[] { "1", "Doom II" }, "FPS"));
        Assert.Equal("Doom II", lv.Items[0].SubItems[1].Text);
        Assert.False(lv.Items[0].Checked);
    }
}
