using GameCatalog.App.Services;

namespace GameCatalog.App.Tests;

public sealed class PrefsStoreTests : IDisposable
{
    private readonly string _dir = Directory.CreateTempSubdirectory("gc-prefs-").FullName;
    private string PathOf(string name) => Path.Combine(_dir, name);

    public void Dispose() => Directory.Delete(_dir, recursive: true);

    [Fact]
    public void Load_MissingFile_ReturnsDefaultsWithFilePath()
    {
        var p = PrefsStore.Load(PathOf("none.json"));
        Assert.Equal(PathOf("none.json"), p.FilePath);
        Assert.Equal(new PrefsStore().TitleFormat, p.TitleFormat);
    }

    [Fact]
    public void Load_CorruptJson_ReturnsDefaults()
    {
        File.WriteAllText(PathOf("bad.json"), "{not json");
        var p = PrefsStore.Load(PathOf("bad.json"));
        Assert.Equal(PathOf("bad.json"), p.FilePath);
        Assert.Empty(p.RecentFiles);
    }

    [Fact]
    public void Save_WithoutFilePath_WritesNothing()
    {
        var before = Directory.GetFiles(_dir).Length;
        new PrefsStore { TitleFormat = "Original" }.Save();
        Assert.Equal(before, Directory.GetFiles(_dir).Length);
    }

    [Fact]
    public void SaveThenLoad_RoundTrips()
    {
        var p = PrefsStore.Load(PathOf("p.json"));
        p.TitleFormat = "Localized";
        p.PushRecent("x.gc");
        p.Save();

        var again = PrefsStore.Load(PathOf("p.json"));
        Assert.Equal("Localized", again.TitleFormat);
        Assert.Equal("x.gc", Assert.Single(again.RecentFiles));
    }
}
