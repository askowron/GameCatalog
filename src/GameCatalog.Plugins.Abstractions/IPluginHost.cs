using GameCatalog.Localization;

namespace GameCatalog.Plugins.Abstractions;

/// <summary>What the application offers to a plugin.</summary>
public interface IPluginHost
{
    string PluginId { get; }
    IGameCatalog Catalog { get; }
    IGameSelection Selection { get; }
    /// <summary>Translations; the plugin's own keys are available as "Plugin.&lt;Id&gt;.&lt;Key&gt;".</summary>
    ITranslator Translator { get; }
    /// <summary>Writable folder for the plugin's settings and caches.</summary>
    string DataDirectory { get; }
    void Log(string message);

    void Register(IGameImporter importer);
    void Register(IGameExporter exporter);
    void Register(IGameInfoProvider provider);
    void Register(ICatalogCommand command);
}
