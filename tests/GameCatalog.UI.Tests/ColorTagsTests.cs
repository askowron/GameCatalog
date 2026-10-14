using GameCatalog.UI.Controls;

namespace GameCatalog.UI.Tests;

public class ColorTagsTests
{
    [Fact]
    public void ToColor_ConvertsAmcBgrValues()
    {
        Assert.Equal(Color.Empty, ColorTags.ToColor(0));
        Assert.Equal(Color.Empty, ColorTags.ToColor(99));
        Assert.Equal(Color.FromArgb(0xED, 0x1C, 0x24), ColorTags.ToColor(1));   // 0x241CED BGR → red
        Assert.Equal(13, ColorTags.Count);
    }
}
