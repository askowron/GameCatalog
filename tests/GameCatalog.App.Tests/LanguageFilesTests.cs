using System.Text.Json;
using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;

namespace GameCatalog.App.Tests;

/// <summary>Guards that every shipped language defines exactly the keys of the default language.</summary>
public class LanguageFilesTests
{
    private static string Dir => Path.Combine(AppContext.BaseDirectory, "Languages");

    private static (HashSet<string> Strings, HashSet<string> Forms) Keys(string code)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(Dir, code + ".json")));
        var strings = doc.RootElement.GetProperty("strings").EnumerateObject().Select(p => p.Name).ToHashSet();
        var forms = doc.RootElement.GetProperty("forms").EnumerateObject()
            .SelectMany(f => f.Value.EnumerateObject().Select(k => f.Name + "/" + k.Name)).ToHashSet();
        return (strings, forms);
    }

    [Fact]
    public void English_HasExactlyTheKeysOfPolish()
    {
        var pl = Keys("pl");
        var en = Keys("en");
        Assert.Empty(pl.Strings.Except(en.Strings));
        Assert.Empty(en.Strings.Except(pl.Strings));
        Assert.Empty(pl.Forms.Except(en.Forms));
        Assert.Empty(en.Forms.Except(pl.Forms));
    }

    [Fact]
    public void Polish_DefinesEveryFieldAndStatus()
    {
        var pl = Keys("pl").Strings;
        foreach (var f in GameFields.All) Assert.Contains(f.LabelKey, pl);
        foreach (var s in Enum.GetValues<CompletionStatus>()) Assert.Contains("Status." + s, pl);
    }
}
