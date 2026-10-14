using GameCatalog.App.Services;
using GameCatalog.Core.Models;
using GameCatalog.Localization;

namespace GameCatalog.App.Tests;

public class TranslatorCatalogLabelsTests
{
    private static LanguageManager Languages(string code)
    {
        var m = new LanguageManager(Path.Combine(AppContext.BaseDirectory, "Languages"), "pl");
        m.SetLanguage(code);
        return m;
    }

    [Fact]
    public void Polish_CaptionsMatchPreviousUi()
    {
        var l = new TranslatorCatalogLabels(Languages("pl"));
        Assert.Equal("Wydawca", l.Field("Publisher"));
        Assert.Equal("Ukończona 100%", l.Status(CompletionStatus.Completed100));
        Assert.Equal("(brak)", l.NoValue);
        Assert.Equal("Tak", l.Yes);
    }

    [Fact]
    public void English_Captions()
    {
        var l = new TranslatorCatalogLabels(Languages("en"));
        Assert.Equal("Publisher", l.Field("Publisher"));
        Assert.Equal("Dropped", l.Status(CompletionStatus.Dropped));
        Assert.Equal("(none)", l.NoValue);
    }

    [Fact]
    public void UnknownTag_ReturnsTag()
        => Assert.Equal("MyCustom", new TranslatorCatalogLabels(Languages("en")).Field("MyCustom"));
}
