using GameCatalog.App.Plugins;

namespace GameCatalog.App.Tests;

public class PluginMenuPathTests
{
    [Theory]
    [InlineData("Tools", "Tools", "")]
    [InlineData("game/Extras/More", "Game", "Extras|More")]
    [InlineData(" Help ", "Help", "")]
    [InlineData("Unknown/Sub", "Tools", "Sub")]
    [InlineData("", "Tools", "")]
    public void Parse_NormalizesRootAndSplitsSubmenus(string path, string root, string submenus)
    {
        var (r, subs) = PluginMenuPath.Parse(path);
        Assert.Equal(root, r);
        Assert.Equal(submenus, string.Join("|", subs));
    }

    [Theory]
    [InlineData("Ctrl+Shift+I", Keys.Control | Keys.Shift | Keys.I, true)]
    [InlineData("F7", Keys.F7, true)]
    [InlineData("nonsense", Keys.None, false)]
    [InlineData(null, Keys.None, false)]
    public void Shortcut_TryParse(string? text, Keys expected, bool ok)
    {
        Assert.Equal(ok, PluginShortcut.TryParse(text, out var keys));
        Assert.Equal(expected, keys);
    }
}
