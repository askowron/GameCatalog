using GameCatalog.Plugins.Abstractions;

namespace GameCatalog.Plugins.Abstractions.WinForms;

/// <summary>Host of the desktop application; plugins with UI cast <see cref="IPluginHost"/> to it.</summary>
public interface IWinFormsPluginHost : IPluginHost
{
    void Register(IGamePanelPlugin panel);
}
