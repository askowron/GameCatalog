using GameCatalog.UI.Components;

namespace GameCatalog.UI.Tests;

public class IconThemeImageListTests
{
    [Theory]
    [InlineData("Klasyczny", "Classic")]
    [InlineData("Kryształowy", "Crystal")]
    [InlineData("Świeży", "Fresh")]
    [InlineData("Gnom", "Gnome")]
    [InlineData("Scrows", "Scrows")]
    [InlineData("Windows XP", "WindowsXP")]
    [InlineData("WindowsXP", "WindowsXP")]
    [InlineData("crystal", "Crystal")]
    [InlineData("", "WindowsXP")]
    [InlineData(null, "WindowsXP")]
    [InlineData("Unknown", "WindowsXP")]
    public void NormalizeThemeId_MapsLegacyPolishNames(string? stored, string expected)
        => Assert.Equal(expected, IconThemeImageList.NormalizeThemeId(stored));

    [Fact]
    public void LoadStrip_CutsBitmapInto16x16Icons()
    {
        var dir = Directory.CreateTempSubdirectory("gc-icons-").FullName;
        using (var bmp = new Bitmap(48, 16)) bmp.Save(Path.Combine(dir, "Classic.bmp"));
        using var images = IconThemeImageList.LoadStrip("Classic", dir);
        Assert.Equal(3, images.Images.Count);
    }

    [Fact]
    public void LoadStrip_MissingFile_ReturnsEmptyList()
    {
        using var images = IconThemeImageList.LoadStrip("Classic", Path.Combine(Path.GetTempPath(), "nope-" + Guid.NewGuid()));
        Assert.Empty(images.Images);
    }

    [Fact]
    public void UseThemeIcons_AssignsImageListToStripAndDropDowns()
    {
        using var themes = new IconThemeImageList();
        var strip = new MenuStrip();
        var sub = new ToolStripMenuItem("x");
        sub.DropDownItems.Add("y");
        strip.Items.Add(sub);

        themes.SetUseThemeIcons(strip, true);

        Assert.Same(themes.ImageList, strip.ImageList);
        Assert.Same(themes.ImageList, sub.DropDown.ImageList);
    }
}
