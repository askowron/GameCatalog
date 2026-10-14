using GameCatalog.Core.Models;
using GameCatalog.UI.Frames;

namespace GameCatalog.UI.Tests;

public class GameCustomFrameTests
{
    [Fact]
    public void YesNoAndTextFields_RoundTrip_LegacyTakReadsAsChecked()
    {
        using var f = new GameCustomFrame();
        f.SetDefinitions(new[]
        {
            new CustomFieldDefinition { Tag = "Owned", Name = "Owned", FieldType = "YesNo" },
            new CustomFieldDefinition { Tag = "Disc", Name = "Disc", FieldType = "Text", DefaultValue = "none" },
        });
        var g = new Game();
        g.CustomValues["Owned"] = "Tak";
        var modified = new List<string>();
        f.Modified += (_, e) => modified.Add(e.Tag);

        f.Bind(g);
        Assert.True(((CheckBox)f.EditorFor("Owned")!).Checked);
        Assert.Equal("none", ((TextBox)f.EditorFor("Disc")!).Text);

        ((CheckBox)f.EditorFor("Owned")!).Checked = false;
        ((TextBox)f.EditorFor("Disc")!).Text = "Blue";
        f.Commit();

        Assert.False(g.CustomValues.ContainsKey("Owned"));
        Assert.Equal("Blue", g.CustomValues["Disc"]);
        Assert.Contains("custom:Disc", modified);
    }
}
