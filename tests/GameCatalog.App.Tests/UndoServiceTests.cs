using GameCatalog.App.Services;

namespace GameCatalog.App.Tests;

public class UndoServiceTests
{
    [Fact]
    public void TryUndo_RunsLastActionFirst()
    {
        var u = new UndoService();
        var log = new List<int>();
        u.Push(() => log.Add(1));
        u.Push(() => log.Add(2));
        Assert.True(u.TryUndo());
        Assert.True(u.TryUndo());
        Assert.False(u.TryUndo());
        Assert.Equal(new[] { 2, 1 }, log);
    }

    [Fact]
    public void Push_KeepsOnlyTheNewestActions()
    {
        var u = new UndoService();
        for (int i = 0; i < UndoService.Capacity + 10; i++) u.Push(() => { });
        Assert.Equal(UndoService.Capacity, u.Count);
    }
}
