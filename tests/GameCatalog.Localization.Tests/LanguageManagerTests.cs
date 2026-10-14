using System.Text.Json;

namespace GameCatalog.Localization.Tests;

public sealed class LanguageManagerTests : IDisposable
{
    private readonly string _dir = Directory.CreateTempSubdirectory("gc-lang-").FullName;
    private readonly List<string> _log = new();

    public void Dispose() => Directory.Delete(_dir, recursive: true);

    private void WriteLang(string code, string name, object forms, object strings)
    {
        var json = JsonSerializer.Serialize(new
        {
            language = new { name, code, author = "t", version = "1" },
            forms,
            strings,
        });
        File.WriteAllText(Path.Combine(_dir, code + ".json"), json);
    }

    private LanguageManager CreateDefault()
    {
        WriteLang("pl", "Polski",
            new { MainForm = new Dictionary<string, string> { ["mnuFile.Text"] = "&Plik", ["$this.Text"] = "Katalog" } },
            new Dictionary<string, string> { ["Msg.Hello"] = "Cześć {0}", ["Only.Pl"] = "tylko pl" });
        WriteLang("en", "English",
            new { MainForm = new Dictionary<string, string> { ["mnuFile.Text"] = "&File" } },
            new Dictionary<string, string> { ["Msg.Hello"] = "Hello {0}" });
        return new LanguageManager(_dir, "pl", _log.Add);
    }

    [Fact]
    public void Available_ListsLanguagesFromHeaders()
    {
        var m = CreateDefault();
        Assert.Equal(new[] { "en", "pl" }, m.Available.Select(l => l.Code).OrderBy(c => c));
        Assert.Equal("Polski", m.Available.Single(l => l.Code == "pl").Name);
    }

    [Fact]
    public void Get_UsesCurrentLanguage_FallsBackToDefault_ThenKey()
    {
        var m = CreateDefault();
        m.SetLanguage("en");
        Assert.Equal("Hello Ann", m.Get("Msg.Hello", "Ann"));
        Assert.Equal("tylko pl", m.Get("Only.Pl"));
        Assert.Equal("Missing.Key", m.Get("Missing.Key"));
        Assert.False(m.TryGet("Missing.Key", out _));
    }

    [Fact]
    public void Get_BadFormatString_ReturnsUnformattedTextAndLogs()
    {
        WriteLang("pl", "Polski", new { }, new Dictionary<string, string> { ["Bad"] = "x {1}" });
        var m = new LanguageManager(_dir, "pl", _log.Add);
        Assert.Equal("x {1}", m.Get("Bad", "only-one"));
        Assert.NotEmpty(_log);
    }

    [Fact]
    public void GetFormValue_FallsBackToDefault_NullWhenUnknown()
    {
        var m = CreateDefault();
        m.SetLanguage("en");
        Assert.Equal("&File", m.GetFormValue("MainForm", "mnuFile.Text"));
        Assert.Equal("Katalog", m.GetFormValue("MainForm", "$this.Text"));
        Assert.Null(m.GetFormValue("MainForm", "btnNope.Text"));
        Assert.Null(m.GetFormValue("NoSuchForm", "x.Text"));
    }

    [Fact]
    public void SetLanguage_RaisesEventOnlyOnChange_UnknownCodeFallsBackToDefault()
    {
        var m = CreateDefault();
        int raised = 0;
        m.LanguageChanged += (_, _) => raised++;
        m.SetLanguage("en");
        m.SetLanguage("en");
        m.SetLanguage("xx");
        Assert.Equal(2, raised);
        Assert.Equal("pl", m.CurrentLanguage);
    }

    [Fact]
    public void CorruptFile_IsSkippedAndLogged()
    {
        CreateDefault();
        File.WriteAllText(Path.Combine(_dir, "de.json"), "{ this is not json");
        var m = new LanguageManager(_dir, "pl", _log.Add);
        Assert.DoesNotContain(m.Available, l => l.Code == "de");
        Assert.Contains(_log, l => l.Contains("de.json"));
    }

    [Fact]
    public void MissingDirectory_DoesNotThrow_ReturnsKeys()
    {
        var m = new LanguageManager(Path.Combine(_dir, "nope"), "pl", _log.Add);
        Assert.Empty(m.Available);
        Assert.Equal("Any.Key", m.Get("Any.Key"));
        Assert.Null(m.GetFormValue("F", "k"));
    }

    [Fact]
    public void AddPluginLanguages_MergesStringsUnderPluginPrefix()
    {
        var m = CreateDefault();
        var pluginDir = Directory.CreateDirectory(Path.Combine(_dir, "plugin")).FullName;
        File.WriteAllText(Path.Combine(pluginDir, "pl.json"),
            "{\"language\":{\"code\":\"pl\",\"name\":\"Polski\"},\"strings\":{\"Hello\":\"Witaj z pluginu\"}}");
        m.AddPluginLanguages("sample", pluginDir);
        Assert.Equal("Witaj z pluginu", m.Get("Plugin.sample.Hello"));
    }

    [Fact]
    public void ExportTemplate_ContainsFormKeysAndAllStrings()
    {
        var m = CreateDefault();
        m.SetLanguage("en");
        var json = m.ExportTemplate(new[]
        {
            new FormKey("MainForm", "mnuFile.Text", "&Plik"),
            new FormKey("AboutForm", "lblVersion.Text", "Wersja"),
        });
        using var doc = JsonDocument.Parse(json);
        var forms = doc.RootElement.GetProperty("forms");
        Assert.Equal("&File", forms.GetProperty("MainForm").GetProperty("mnuFile.Text").GetString());
        Assert.Equal("Wersja", forms.GetProperty("AboutForm").GetProperty("lblVersion.Text").GetString());
        var strings = doc.RootElement.GetProperty("strings");
        Assert.Equal("Hello {0}", strings.GetProperty("Msg.Hello").GetString());
        Assert.Equal("tylko pl", strings.GetProperty("Only.Pl").GetString());
        Assert.Equal("en", doc.RootElement.GetProperty("language").GetProperty("code").GetString());
    }

    [Fact]
    public void NullSections_LoadWithoutThrowing_CodeFromFileName()
    {
        CreateDefault();
        File.WriteAllText(Path.Combine(_dir, "fr.json"), "{\"language\":null,\"strings\":null,\"forms\":null}");
        var m = new LanguageManager(_dir, "pl", _log.Add);
        Assert.Contains(m.Available, l => l.Code == "fr");
        m.SetLanguage("fr");
        Assert.Equal("Any.Key", m.Get("Any.Key"));
        Assert.Equal("&Plik", m.GetFormValue("MainForm", "mnuFile.Text"));
        Assert.Contains(_log, l => l.Contains("fr.json"));
    }

    [Fact]
    public void NullDocument_IsSkippedAndLogged()
    {
        CreateDefault();
        File.WriteAllText(Path.Combine(_dir, "bad.json"), "null");
        var m = new LanguageManager(_dir, "pl", _log.Add);
        Assert.DoesNotContain(m.Available, l => l.Code == "bad");
        Assert.Contains(_log, l => l.Contains("bad.json"));
    }

    [Fact]
    public void NullEntries_AreDroppedAndLogged()
    {
        CreateDefault();
        File.WriteAllText(Path.Combine(_dir, "de.json"),
            "{\"language\":{\"code\":\"de\"},\"forms\":{\"MainForm\":null,\"Other\":{\"x.Text\":null}},\"strings\":{\"K\":null}}");
        var m = new LanguageManager(_dir, "pl", _log.Add);
        m.SetLanguage("de");
        Assert.Equal("K", m.Get("K"));
        Assert.Null(m.GetFormValue("Other", "x.Text"));
        Assert.Contains(_log, l => l.Contains("de.json"));
    }

    [Fact]
    public void SetLanguage_StoresCanonicalCode()
    {
        var m = CreateDefault();
        m.SetLanguage("EN");
        Assert.Equal("en", m.CurrentLanguage);
    }
}
