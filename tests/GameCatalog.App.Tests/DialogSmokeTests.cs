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
        new object[] { (Func<Form>)(() => new ScriptsForm(new Game { Title = "A" }, _ => Task.CompletedTask)) },
        new object[] { (Func<Form>)(() => new PicturesForm(new GameCatalog.Core.Catalog.GameList())) },
        new object[] { (Func<Form>)(() => new LoanForm(new GameCatalog.Core.Catalog.GameList(), new List<LoanRecord>())) },
        new object[] { (Func<Form>)(() => new StatsForm(new GameCatalog.Core.Catalog.GameList(), new HashSet<int>(), null, "c.gc")) },
        new object[] { (Func<Form>)(() => new PrefsForm(new PrefsStore())) },
        new object[] { (Func<Form>)(() => new ChoiceDialog("T", "P", new[] { "a" })) },
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
    public void PrefsForm_SavingUnchangedKeepsEveryValue()
    {
        var prefs = new PrefsStore
        {
            Theme = GameCatalog.UI.Components.IconThemeImageList.Themes[^1].Id,
            AutoNumbering = true, ShowGroupCount = false, HtmlTemplate = HtmlTemplates.Names[^1],
            TitleFormat = "Localized", NaturalSort = false, ForceFullRefresh = true, ReopenLastFile = true,
            FullRowSelect = false, ShowGridLines = true, ShowMenuIcons = false, CoverVisible = false,
            ShowExtraCategory = false, CsvDelimiter = "TAB", BackupOnSave = true, MediaPatterns = "*.lnk",
            MaxCoverWidth = 300, ScriptTimeoutSec = 30,
            CustomLists = { ["Platform"] = new List<string> { "PC" } },
            CustomSearchSites = { new SearchSite("S", "https://s/?q={q}") },
        };
        using var form = new PrefsForm(prefs);
        _ = form.Handle;
        var save = (Button)form.Controls.Find("btnSave", true).Single();
        typeof(Button).GetMethod("OnClick", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .Invoke(save, new object[] { EventArgs.Empty });

        Assert.Equal(prefs.Theme, form.Theme);
        Assert.Equal((true, false, prefs.HtmlTemplate, "Localized", false, true, true),
            (form.AutoNumbering, form.ShowGroupCount, form.HtmlTemplate, form.TitleFormat, form.NaturalSort, form.ForceFullRefresh, form.ReopenLastFile));
        Assert.Equal((false, true, false, false, false, "TAB", true),
            (form.FullRowSelect, form.ShowGridLines, form.ShowMenuIcons, form.CoverVisible, form.ShowExtraCategory, form.CsvDelimiter, form.BackupOnSave));
        Assert.Equal(("*.lnk", 300, 30), (form.MediaPatterns, form.MaxCoverWidth, form.ScriptTimeoutSec));
        Assert.Equal(new[] { "PC" }, form.CustomLists["Platform"]);
        Assert.Equal(prefs.CustomSearchSites, form.CustomSearchSites);
    }

    [Fact]
    public void PrefsForm_TreeFollowsAmcHierarchyAndShowsOnePage()
    {
        AppServices.Languages.SetLanguage("en");
        using var form = new PrefsForm(new PrefsStore())
        {
            StartPosition = FormStartPosition.Manual, Location = new Point(-10000, -10000), ShowInTaskbar = false,
        };
        form.Show();   // Control.Visible is only true for a shown window
        var tree = (TreeView)form.Controls.Find("tvPages", true).Single();
        Assert.Equal(9, tree.Nodes.Count);
        Assert.Equal("General options", tree.Nodes[0].Text);
        Assert.Equal(4, tree.Nodes[5].Nodes.Count);   // Game: lists, media import, picture import, web search
        var pages = form.Controls.Find("pnlPages", true).Single().Controls.Cast<Control>().ToList();
        Assert.Equal(15, pages.Count);
        Assert.Equal(new[] { "pnlPageGeneral" }, pages.Where(p => p.Visible).Select(p => p.Name));
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
