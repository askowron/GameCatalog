using System.Globalization;
using GameCatalog.App.Services;
using GameCatalog.Localization;

namespace GameCatalog.App.Tests;

public class StartupLanguageTests
{
    private static readonly LanguageInfo[] Available =
        { new("pl", "Polski", "", ""), new("en", "English", "", "") };

    [Theory]
    [InlineData("", "pl-PL", "pl")]
    [InlineData("", "en-US", "en")]
    [InlineData(null, "en-GB", "en")]
    [InlineData("", "de-DE", "pl")]       // no German translation: default language
    [InlineData("en", "pl-PL", "en")]     // explicit choice wins over the system
    [InlineData("xx", "en-US", "en")]     // unknown stored code: fall back to the system
    public void Resolve_PrefersChoiceThenSystemThenDefault(string? preferred, string culture, string expected) =>
        Assert.Equal(expected, StartupLanguage.Resolve(preferred, Available, new CultureInfo(culture), "pl"));
}
