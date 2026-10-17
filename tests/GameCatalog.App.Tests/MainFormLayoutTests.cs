using GameCatalog.App.Services;
using GameCatalog.Localization;
using GameCatalog.UI.Components;

namespace GameCatalog.App.Tests;

public class MainFormLayoutTests
{
    public MainFormLayoutTests()
    {
        AppServices.Languages = new LanguageManager(Path.Combine(AppContext.BaseDirectory, "Languages"), "pl");
        AppServices.Prefs = new PrefsStore();
        Localizer.Translator = AppServices.Languages;
    }

    [Fact]
    public void MainToolbar_StartsAtTheLeftEdge()   // the hidden plugin toolbar used to push it right
    {
        using var form = new MainForm { Size = new Size(1280, 800) };
        _ = form.Handle;
        form.PerformLayout();

        var toolbar = (ToolStrip)form.Controls.Find("tsMain", true).Single();
        Assert.True(toolbar.Left < 10, $"tsMain.Left = {toolbar.Left}, width = {toolbar.Width}");
    }
}
