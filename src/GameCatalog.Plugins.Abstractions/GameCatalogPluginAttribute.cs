namespace GameCatalog.Plugins.Abstractions;

/// <summary>Marks the plugin entry class. The class must implement <see cref="IPlugin"/> and have a public parameterless constructor.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class GameCatalogPluginAttribute(string id, string name, string version) : Attribute
{
    /// <summary>Stable identifier; also the prefix of the plugin's translation keys ("Plugin.&lt;Id&gt;.").</summary>
    public string Id { get; } = id;
    public string Name { get; } = name;
    public string Version { get; } = version;
    public string Author { get; set; } = "";
    public string Description { get; set; } = "";
    /// <summary>Contract version the plugin was built against; defaults to the version it was compiled with.</summary>
    public int ApiVersion { get; set; } = PluginApi.Version;
}
