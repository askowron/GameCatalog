namespace GameCatalog.Plugins.Hosting;

public enum PluginState { Loaded, Disabled, Incompatible, Failed }

/// <summary>What the Plugins window shows about one plugin.</summary>
public sealed class PluginDescriptor(string id, string name, string version, string author, string description, string assemblyPath)
{
    public string Id { get; } = id;
    public string Name { get; } = name;
    public string Version { get; } = version;
    public string Author { get; } = author;
    public string Description { get; } = description;
    public string AssemblyPath { get; } = assemblyPath;
    public PluginState State { get; set; }
    public string? Error { get; set; }
}
