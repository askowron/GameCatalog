using GameCatalog.App.Services;
using GameCatalog.Core.Models;
using Microsoft.Data.Sqlite;

namespace GameCatalog.App.Tests;

public sealed class CatalogSessionTests : IDisposable
{
    private readonly string _dir = Directory.CreateTempSubdirectory("gc-session-").FullName;
    private string PathOf(string name) => Path.Combine(_dir, name);

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        Directory.Delete(_dir, recursive: true);
    }

    [Fact]
    public void Open_NewDefaultFile_AddsSamples()
    {
        var s = new CatalogSession(new PrefsStore());
        s.Open(PathOf("new.gc"), addSamplesIfNew: true);
        Assert.NotEmpty(s.Games.Games);
        Assert.False(s.IsDirty);
    }

    [Fact]
    public void Open_ExistingEmptyFile_DoesNotAddSamples()
    {
        var s = new CatalogSession(new PrefsStore());
        s.New(PathOf("empty.gc"));
        var reopened = new CatalogSession(new PrefsStore());
        reopened.Open(PathOf("empty.gc"), addSamplesIfNew: true);
        Assert.Empty(reopened.Games.Games);
    }

    [Fact]
    public void Save_RaisesSavingThenSaved_ClearsDirty_WritesBackup()
    {
        var prefs = new PrefsStore { BackupOnSave = true };
        var s = new CatalogSession(prefs);
        s.New(PathOf("a.gc"));
        var events = new List<string>();
        s.Saving += (_, _) => events.Add("saving");
        s.Saved += (_, _) => events.Add("saved");
        s.Games.Add(new Game { Title = "X" });
        s.MarkDirty();
        Assert.True(s.IsDirty);

        s.Save();

        Assert.Equal(new[] { "saving", "saved" }, events);
        Assert.False(s.IsDirty);
        Assert.True(File.Exists(PathOf("a.gc.bak")));
    }

    [Fact]
    public void SaveAs_WritesToNewFileAndKeepsGames()
    {
        var s = new CatalogSession(new PrefsStore());
        s.New(PathOf("a.gc"));
        s.Games.Add(new Game { Title = "X" });
        s.SaveAs(PathOf("b.gc"));
        var other = new CatalogSession(new PrefsStore());
        other.Open(PathOf("b.gc"));
        Assert.Equal("X", Assert.Single(other.Games.Games).Title);
        Assert.Equal(PathOf("b.gc"), s.FilePath);
    }

    [Fact]
    public void MarkDirty_RaisesDirtyChangedOnce_AndChangedEveryTime()
    {
        var s = new CatalogSession(new PrefsStore());
        s.New(PathOf("a.gc"));
        int dirty = 0, changed = 0;
        s.DirtyChanged += (_, _) => dirty++;
        s.Changed += (_, _) => changed++;
        s.MarkDirty();
        s.MarkDirty();
        Assert.Equal(1, dirty);
        Assert.Equal(2, changed);
    }

    [Fact]
    public void Open_PushesRecentFile()
    {
        var prefs = new PrefsStore();
        var s = new CatalogSession(prefs);
        s.New(PathOf("a.gc"));
        Assert.Equal(PathOf("a.gc"), prefs.RecentFiles[0]);
    }
}
