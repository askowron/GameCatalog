using GameCatalog.App.Diagnostics;

namespace GameCatalog.App.Tests;

public class AppLogTests
{
    [Fact]
    public void Write_AppendsTimestampedLines()
    {
        var dir = Directory.CreateTempSubdirectory("gc-log-").FullName;
        AppLog.Initialize(dir);
        AppLog.Write("hello");
        AppLog.Error(new InvalidOperationException("boom"), "test");
        var text = File.ReadAllText(AppLog.FilePath!);
        Assert.Contains("hello", text);
        Assert.Contains("ERROR test", text);
        Assert.Contains("boom", text);
    }
}
