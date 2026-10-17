using GameCatalog.Core.Models;
using GameCatalog.Plugins.Abstractions;

namespace GameCatalog.Plugins.Abstractions.WinForms;

/// <summary>A tab added to the game info panel.</summary>
public interface IGamePanelPlugin
{
    string Title { get; }
    /// <summary>Called once; the returned control is docked to fill the tab.</summary>
    Control CreatePanel(IPluginHost host);
    void OnCurrentGameChanged(Game? game);
}
