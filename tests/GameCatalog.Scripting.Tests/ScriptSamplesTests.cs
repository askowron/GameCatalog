using GameCatalog.Core.Models;

namespace GameCatalog.Scripting.Tests;

public class ScriptSamplesTests
{
    public static IEnumerable<object[]> Samples() => ScriptSamples.FileNames.Select(n => new object[] { n });

    [Fact]
    public void FileNames_ListsTheBuiltInSamples()
    {
        Assert.Equal(new[] { "GRY-Online.pl.gcsx", "IGDB.gcsx", "Steam.gcsx", "Wikipedia PL.gcsx" }, ScriptSamples.FileNames);
    }

    [Theory]
    [MemberData(nameof(Samples))]
    public void Sample_HasHeaderAndCompiles(string fileName)
    {
        var code = ScriptSamples.Read(fileName);
        var header = GameScript.Parse(code);
        Assert.False(string.IsNullOrWhiteSpace(header.Name));
        Assert.Equal("GameCatalog", header.Author);
        Assert.Empty(ScriptRunner.CompileErrors(code));
    }

    [Fact]
    public void EnsureSamples_UpgradesOlderBuiltInsAndKeepsABackup()
    {
        var dir = Directory.CreateTempSubdirectory("gc-scripts-").FullName;
        try
        {
            var steam = Path.Combine(dir, "Steam.gcsx");
            var mine = Path.Combine(dir, "IGDB.gcsx");
            File.WriteAllText(steam, "// Name: Steam\n// Version: 1.0\n// Author: GameCatalog\nApi.Log(\"old\");");
            File.WriteAllText(mine, "// Name: IGDB\n// Version: 0.1\n// Author: Me\nApi.Log(\"mine\");");

            ScriptSamples.EnsureSamples(dir);

            Assert.Equal(ScriptSamples.Read("Steam.gcsx"), File.ReadAllText(steam));
            Assert.Contains("Api.Log(\"old\");", File.ReadAllText(steam + ".v1.0.bak"));
            Assert.Contains("Api.Log(\"mine\");", File.ReadAllText(mine));          // someone else's script stays
            Assert.True(File.Exists(Path.Combine(dir, "Wikipedia PL.gcsx")));      // missing samples are written

            File.WriteAllText(steam, ScriptSamples.Read("Steam.gcsx") + "// my edit");
            ScriptSamples.EnsureSamples(dir);
            Assert.EndsWith("// my edit", File.ReadAllText(steam));                // same version: edits kept
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void HtmlToText_KeepsParagraphsAndListItems()
    {
        var api = new ScriptApi(new Game(), new List<string>());
        var text = api.HtmlToText("<h1>Tytu&#322;</h1><p>Jeden<br>dwa</p><ul><li>A</li><li>B</li></ul>");
        Assert.Equal("Tytuł\n\nJeden\ndwa\n\n- A\n- B", text);
    }

    [Fact]
    public async Task AddExtra_KeepsUrlAndReturnsTheExtra()
    {
        var result = await ScriptRunner.RunAsync(
            "var e = Api.AddExtra(\"Blood and Wine\", \"Rozszerzenie\", \"Opis\", \"https://x/dlc\"); Api.Log(e!.Url);",
            new Game { Title = "W3" });
        Assert.True(result.Success, result.Error);
        Assert.Equal("https://x/dlc", Assert.Single(result.Extras).Url);
    }
}
