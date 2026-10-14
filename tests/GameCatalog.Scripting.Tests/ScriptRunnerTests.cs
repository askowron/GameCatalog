using GameCatalog.Core.Models;

namespace GameCatalog.Scripting.Tests;

public class ScriptRunnerTests
{
    [Fact]
    public async Task RunAsync_ScriptSetsFields_ReturnsSuccess()
    {
        var game = new Game { Title = "Quake" };
        var result = await ScriptRunner.RunAsync(
            "Api.SetField(\"Developer\", \"id Software\"); Api.AddExtra(\"Scourge of Armagon\", \"Expansion\"); Api.Log(Api.Title);",
            game);

        Assert.True(result.Success, result.Error);
        Assert.Equal("id Software", result.Fields["Developer"]);
        Assert.Equal("Scourge of Armagon", Assert.Single(result.Extras).Title);
        Assert.Contains(result.Log, l => l.EndsWith("Quake"));
        Assert.True(result.HasData);
    }

    [Fact]
    public async Task RunAsync_CompileError_ReportsLine()
    {
        var result = await ScriptRunner.RunAsync("int x = ;", new Game());
        Assert.False(result.Success);
        Assert.Contains("1", result.Error);
    }

    [Fact]
    public async Task RunAsync_RuntimeException_IsReportedNotThrown()
    {
        var result = await ScriptRunner.RunAsync("throw new System.InvalidOperationException(\"boom\");", new Game());
        Assert.False(result.Success);
        Assert.Contains("boom", result.Error);
    }
}
