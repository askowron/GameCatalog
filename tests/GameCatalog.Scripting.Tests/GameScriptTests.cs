namespace GameCatalog.Scripting.Tests;

public class GameScriptTests
{
    [Fact]
    public void Load_ParsesHeaderMetadata()
    {
        var path = Path.Combine(Path.GetTempPath(), $"s-{Guid.NewGuid():N}.gcsx");
        File.WriteAllText(path, "// Name: Test\n// Description: Desc\n// Version: 2.1\n// Author: Me\n// Site: https://x\nApi.Log(\"x\");");
        try
        {
            var s = GameScript.Load(path);
            Assert.Equal("Test", s.Name);
            Assert.Equal("Desc", s.Description);
            Assert.Equal("2.1", s.Version);
            Assert.Equal("Me", s.Author);
            Assert.Equal("https://x", s.Site);
            Assert.Equal("Test (2.1)", s.ToString());
        }
        finally { File.Delete(path); }
    }
}
