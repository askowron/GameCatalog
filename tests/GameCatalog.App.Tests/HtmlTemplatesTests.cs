using GameCatalog.App.Services;
using GameCatalog.Core.Models;
using GameCatalog.Localization;

namespace GameCatalog.App.Tests;

/// <summary>The built-in game card follows the UI language.</summary>
public class HtmlTemplatesTests
{
    public HtmlTemplatesTests()
    {
        AppServices.Languages = new LanguageManager(Path.Combine(AppContext.BaseDirectory, "Languages"), "pl");
    }

    [Fact]
    public void Expand_BuiltInTemplate_UsesCaptionsOfTheCurrentLanguage()
    {
        AppServices.Languages.SetLanguage("en");
        var game = new Game { Title = "Doom", Publisher = "id Software" };

        var html = HtmlTemplates.Expand(HtmlTemplates.Detailed, game, null);

        Assert.Contains("<b>Publisher</b>", html);
        Assert.Contains("Extras / DLC", html);
        Assert.DoesNotContain("Wydawca", html);
        Assert.DoesNotContain("{L:", html);
    }

    [Fact]
    public void Expand_UserTemplateWithoutPlaceholders_IsUnchanged()
    {
        AppServices.Languages.SetLanguage("en");
        var html = HtmlTemplates.Expand("<b>Wydawca</b> {Title}", new Game { Title = "Doom" }, null);
        Assert.Equal("<b>Wydawca</b> Doom", html);
    }

    [Theory]
    [InlineData("pl", "Szczegółowy")]
    [InlineData("en", "Detailed")]
    public void DisplayName_TranslatesBuiltInTemplates(string language, string expected)
    {
        AppServices.Languages.SetLanguage(language);
        Assert.Equal(expected, HtmlTemplates.DisplayName(HtmlTemplates.Names[0]));
        Assert.Equal("my-template", HtmlTemplates.DisplayName("my-template"));
    }
}
