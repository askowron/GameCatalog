using GameCatalog.Localization;
using GameCatalog.Plugins.Abstractions;
using GameCatalog.Plugins.Abstractions.WinForms;
using GameCatalog.Plugins.Hosting;

namespace GameCatalog.Plugins.Hosting.Tests;

/// <summary>Host whose catalog/selection are not needed by Initialize of the tested plugins.</summary>
internal sealed class TestHost(string pluginId, ExtensionRegistry registry, ITranslator translator)
    : PluginHostBase(pluginId, registry, Path.GetTempPath(), _ => { }), IWinFormsPluginHost
{
    public override IGameCatalog Catalog => throw new NotSupportedException();
    public override IGameSelection Selection => throw new NotSupportedException();
    public override ITranslator Translator => translator;
    public void Register(IGamePanelPlugin panel) => Registry.Add(PluginId, panel);
}
