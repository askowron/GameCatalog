using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;
using GameCatalog.UI.Frames;

namespace GameCatalog.UI.Tests;

public class GameFrameTests
{
    private static Game Sample() => new()
    {
        Title = "Doom", Developer = "id", UserRating = 85, CriticRating = -1, ReleaseYear = 1993,
        PlaytimeHours = 12.5, LastPlayed = null, CompletionStatus = CompletionStatus.Playing, Checked = true,
    };

    [Fact]
    public void Bind_ThenCommitWithoutEdits_ChangesNothing()
    {
        CatalogLabels.Current = new InvariantCatalogLabels();
        using var f = new GameFrame();
        var g = Sample();
        var modified = new List<string>();
        f.Modified += (_, e) => modified.Add(e.Tag);

        f.Bind(g);
        f.Commit();

        Assert.Empty(modified);
        Assert.Equal(85, g.UserRating);
        Assert.Equal(-1, g.CriticRating);
        Assert.Null(g.LastPlayed);
        Assert.Equal(CompletionStatus.Playing, g.CompletionStatus);
    }

    [Fact]
    public void EditingEditors_WritesBackAndRaisesModified()
    {
        CatalogLabels.Current = new InvariantCatalogLabels();
        using var f = new GameFrame();
        var g = Sample();
        var modified = new List<string>();
        f.Modified += (_, e) => modified.Add(e.Tag);
        f.Bind(g);

        ((TextBox)f.EditorFor("Title")!).Text = "Doom II";
        ((NumericUpDown)f.EditorFor("UserRating")!).Value = 9.0m;
        ((NumericUpDown)f.EditorFor("ReleaseYear")!).Value = 1994;
        ((ComboBox)f.EditorFor("CompletionStatus")!).SelectedIndex = (int)CompletionStatus.Completed;
        f.Commit();

        Assert.Equal("Doom II", g.Title);
        Assert.Equal(90, g.UserRating);
        Assert.Equal(1994, g.ReleaseYear);
        Assert.Equal(CompletionStatus.Completed, g.CompletionStatus);
        Assert.Contains("Title", modified);
        Assert.Contains("UserRating", modified);
    }

    [Fact]
    public void RatingNoneCheckbox_StoresMinusOne()
    {
        using var f = new GameFrame();
        var g = Sample();
        f.Bind(g);
        ((CheckBox)f.EditorFor("UserRatingNone")!).Checked = true;
        f.Commit();
        Assert.Equal(-1, g.UserRating);
    }

    [Fact]
    public void Bind_Null_DisablesFrame()
    {
        using var f = new GameFrame();
        f.Bind(null);
        Assert.False(f.Enabled);
    }

    [Fact]
    public void SetSuggestions_KeepsPresetsAndCurrentText()
    {
        using var f = new GameFrame();
        var g = Sample();
        g.Store = "Steam";
        f.Bind(g);
        f.SetSuggestions("Store", new[] { "itch.io", "Steam" });
        var cbx = (ComboBox)f.EditorFor("Store")!;
        Assert.Contains("itch.io", cbx.Items.Cast<string>());
        Assert.Contains("GOG", cbx.Items.Cast<string>());
        Assert.Equal("Steam", cbx.Text);
    }
}
