namespace GameCatalog.Plugins.Hosting;

/// <summary>Every extension registered by plugins and by the application itself, with its owner.</summary>
public sealed class ExtensionRegistry
{
    /// <summary>Owner id used for the application's built-in extensions.</summary>
    public const string BuiltInId = "builtin";

    private readonly List<(string PluginId, object Extension)> _items = new();

    public void Add(string pluginId, object extension) => _items.Add((pluginId, extension));

    public IReadOnlyList<T> All<T>() => _items.Select(i => i.Extension).OfType<T>().ToList();

    public IReadOnlyList<(string PluginId, T Extension)> AllWithOwner<T>() =>
        _items.Where(i => i.Extension is T).Select(i => (i.PluginId, (T)i.Extension)).ToList();

    public void RemoveAll(string pluginId) => _items.RemoveAll(i => i.PluginId == pluginId);
}
