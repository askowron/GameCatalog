using GameCatalog.Core.Models;
using GameCatalog.Plugins.Abstractions;
using GameCatalog.Plugins.Abstractions.WinForms;

namespace GameCatalog.Plugin.Sample;

/// <summary>"Summary" tab showing a one-paragraph description of the current game.</summary>
internal sealed class GameSummaryPanel(IPluginHost host) : IGamePanelPlugin
{
    private Label? _label;

    public string Title => host.Translator.Get("Plugin.sample.PanelTitle");

    public Control CreatePanel(IPluginHost _)
    {
        _label = new Label { Dock = DockStyle.Fill, Padding = new Padding(12), Font = new Font(SystemFonts.MessageBoxFont!.FontFamily, 11f) };
        return _label;
    }

    public void OnCurrentGameChanged(Game? game)
    {
        if (_label is null) return;
        _label.Text = game is null
            ? ""
            : host.Translator.Get("Plugin.sample.PanelSummary", game.DisplayTitle, game.Extras.Count, game.PlaytimeHours);
    }
}
