using GameCatalog.Core.Models;
using GameCatalog.UI.Frames;

namespace GameCatalog.UI.Tests;

public class GameExtraFrameTests
{
    [Fact]
    public void Commit_TrimsAndWritesAllFields()
    {
        using var f = new GameExtraFrame();
        var e = new GameExtra { Title = "A" };
        f.Bind(e);
        f.TitleText = "  Hearts of Stone ";
        f.Commit();
        Assert.Equal("Hearts of Stone", e.Title);
    }
}
