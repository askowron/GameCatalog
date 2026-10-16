using GameCatalog.App.Forms;
using GameCatalog.App.Services;
using GameCatalog.Core.Models;
using GameCatalog.Localization;
using GameCatalog.Plugins.Abstractions;
using GameCatalog.UI.Components;

namespace GameCatalog.App.Tests;

/// <summary>Every converted dialog can be constructed and translated without showing it.</summary>
public class DialogSmokeTests
{
    public DialogSmokeTests()
    {
        AppServices.Languages = new LanguageManager(Path.Combine(AppContext.BaseDirectory, "Languages"), "pl");
        AppServices.Prefs = new PrefsStore();
        Localizer.Translator = AppServices.Languages;
    }

    public static IEnumerable<object[]> Dialogs() => new[]
    {
        new object[] { (Func<Form>)(() => new AboutForm()) },
        new object[] { (Func<Form>)(() => new HelpForm()) },
        new object[] { (Func<Form>)(() => new VersionsForm()) },
        new object[] { (Func<Form>)(() => new HtmlEditorForm("default")) },
        new object[] { (Func<Form>)(() => new ExtraEditForm(new GameExtra { Title = "DLC" })) },
        new object[] { (Func<Form>)(() => new CustomFieldsForm(new List<CustomFieldDefinition>())) },
        new object[] { (Func<Form>)(() => new CustomFieldEditForm(new CustomFieldDefinition { Tag = "F1", Name = "F" }, Array.Empty<string>())) },
        new object[] { (Func<Form>)(() => new NumberDialog(1, 10)) },
        new object[] { (Func<Form>)(() => new NumberConflictDialog(2, 10, n => n == 2, _ => "Doom")) },
        new object[] { (Func<Form>)(() => new CatalogPropertiesDialog("c.gc", 1, 2, 3.5, 0)) },
        new object[] { (Func<Form>)(() => new PickForm("Pick", new List<string> { "a", "b" })) },
        new object[] { (Func<Form>)(() => new ScriptResultForm(new Game { Title = "A" }, new GameInfoResult { Success = true, Fields = { ["Title"] = "B" } })) },
    };

    [Theory]
    [MemberData(nameof(Dialogs))]
    public void Dialog_CreatesAndTranslatesInBothLanguages(Func<Form> create)
    {
        foreach (var code in new[] { "pl", "en" })
        {
            AppServices.Languages.SetLanguage(code);
            using var form = create();
            _ = form.Handle;               // invisible top-level forms may skip CreateControl; Handle forces Localizer.Apply
            Assert.False(string.IsNullOrWhiteSpace(form.Text));
        }
    }

    [Fact]
    public void ExtraEditForm_TranslatesCaptionToEnglish()
    {
        AppServices.Languages.SetLanguage("en");
        using var form = new ExtraEditForm(new GameExtra { Title = "DLC" });
        _ = form.Handle;
        Assert.Equal("Extra / DLC", form.Text);
    }

    [Fact]
    public void NumberConflictDialog_NamesTheGameHoldingTheNumber()
    {
        AppServices.Languages.SetLanguage("en");
        using var form = new NumberConflictDialog(2, 10, n => n == 2, _ => "Doom");
        _ = form.Handle;
        var label = Assert.Single(form.Controls.Find("lblTakenBy", true));
        Assert.Equal("Number 2 is already used by: Doom", label.Text);
        Assert.Equal("Accept the duplicate", form.Controls.Find("rbAcceptDuplicate", true).Single().Text);
    }

    [Fact]
    public void HelpForm_ListsTranslatedTopicsAndShowsTheFirst()
    {
        AppServices.Languages.SetLanguage("en");
        using var form = new HelpForm();
        _ = form.Handle;
        var topics = (ListBox)form.Controls.Find("lstTopics", true).Single();
        Assert.Equal(9, topics.Items.Count);
        Assert.Equal("Getting started", topics.Items[0]);
        Assert.StartsWith("GameCatalog keeps games", form.Controls.Find("rtbText", true).Single().Text);
    }

    [Fact]
    public void NumberDialog_CallerCaptionSurvivesTranslation()
    {
        AppServices.Languages.SetLanguage("en");
        using var form = new NumberDialog(1, 10) { Text = "Custom" };
        _ = form.Handle;
        Assert.Equal("Custom", form.Text);
    }
}
