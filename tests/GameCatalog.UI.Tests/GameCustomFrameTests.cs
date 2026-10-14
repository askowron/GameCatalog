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

    [Fact]
    public void LegacyCommaNumber_ReadsAsDecimal()
    {
        using var f = new GameCustomFrame();
        f.SetDefinitions(new[] { new CustomFieldDefinition { Tag = "Hrs", Name = "Hrs", FieldType = "Number" } });
        var g = new Game();
        g.CustomValues["Hrs"] = "12,5";
        f.Bind(g);
        Assert.Equal(12.5m, ((NumericUpDown)f.EditorFor("Hrs")!).Value);
    }

    [Fact]
    public void Bind_ThenCommitWithoutEdits_ChangesNothing()
    {
        using var f = new GameCustomFrame();
        f.SetDefinitions(new[]
        {
            new CustomFieldDefinition { Tag = "N1", Name = "N1", FieldType = "Number" },
            new CustomFieldDefinition { Tag = "N2", Name = "N2", FieldType = "Number" },
            new CustomFieldDefinition { Tag = "T", Name = "T", FieldType = "Text", DefaultValue = "dflt" },
            new CustomFieldDefinition { Tag = "D", Name = "D", FieldType = "Date" },
            new CustomFieldDefinition { Tag = "L", Name = "L", FieldType = "List", ListValues = "a;b" },
            new CustomFieldDefinition { Tag = "Y", Name = "Y", FieldType = "YesNo" },
        });
        var g = new Game();
        g.CustomValues["N2"] = "12,5";
        g.CustomValues["Y"] = "Tak";
        var before = new Dictionary<string, string>(g.CustomValues);
        var modified = new List<string>();
        f.Modified += (_, e) => modified.Add(e.Tag);

        f.Bind(g);
        f.Commit();

        Assert.Empty(modified);
        Assert.Equal(before, g.CustomValues);
    }
}
