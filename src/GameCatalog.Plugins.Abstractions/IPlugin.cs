namespace GameCatalog.Plugins.Abstractions;

/// <summary>Plugin entry point. Initialize registers the plugin's extensions with the host.</summary>
public interface IPlugin
{
    void Initialize(IPluginHost host);
}
