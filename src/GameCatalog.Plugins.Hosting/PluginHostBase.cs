using GameCatalog.Localization;
using GameCatalog.Plugins.Abstractions;

namespace GameCatalog.Plugins.Hosting;

/// <summary>Registration and logging shared by every host implementation; one instance per plugin.</summary>
public abstract class PluginHostBase(string pluginId, ExtensionRegistry registry, string dataDirectory, Action<string> log) : IPluginHost
{
    protected ExtensionRegistry Registry { get; } = registry;

    public string PluginId { get; } = pluginId;
    public string DataDirectory { get; } = dataDirectory;
    public abstract IGameCatalog Catalog { get; }
    public abstract IGameSelection Selection { get; }
    public abstract ITranslator Translator { get; }

    public void Log(string message) => log($"[{PluginId}] {message}");

    public void Register(IGameImporter importer) => Registry.Add(PluginId, importer);
    public void Register(IGameExporter exporter) => Registry.Add(PluginId, exporter);
    public void Register(IGameInfoProvider provider) => Registry.Add(PluginId, provider);
    public void Register(ICatalogCommand command) => Registry.Add(PluginId, command);
}
