using GameCatalog.Localization;
using GameCatalog.Plugins.Abstractions;
using GameCatalog.Plugins.Abstractions.WinForms;
using GameCatalog.Plugins.Hosting;

namespace GameCatalog.App.Plugins;

/// <summary>Host given to each plugin; catalog and selection are facades over the main window.</summary>
internal sealed class AppPluginHost(
    string pluginId, ExtensionRegistry registry, string dataDirectory, Action<string> log,
    IGameCatalog catalog, IGameSelection selection, ITranslator translator)
    : PluginHostBase(pluginId, registry, dataDirectory, log), IWinFormsPluginHost
{
    public override IGameCatalog Catalog => catalog;
    public override IGameSelection Selection => selection;
    public override ITranslator Translator => translator;
    public void Register(IGamePanelPlugin panel) => Registry.Add(PluginId, panel);
}
