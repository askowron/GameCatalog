using System.Reflection;
using GameCatalog.Plugins.Abstractions;

namespace GameCatalog.Plugins.Hosting;

/// <summary>
/// Discovers plugins in Plugins\&lt;Name&gt;\&lt;Name&gt;.dll, checks their contract version and initializes them.
/// A plugin that fails never stops the others or the application: it is reported with its error.
/// </summary>
public sealed class PluginManager(ExtensionRegistry registry, Func<PluginDescriptor, IPluginHost> hostFactory, Action<string>? log = null)
{
    /// <summary>Assemblies always taken from the application, never from a plugin folder.</summary>
    public static readonly IReadOnlySet<string> SharedAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "GameCatalog.Core",
        "GameCatalog.Localization",
        "GameCatalog.Plugins.Abstractions",
        "GameCatalog.Plugins.Abstractions.WinForms",
    };

    private readonly List<PluginDescriptor> _plugins = new();
    private readonly Action<string> _log = log ?? (_ => { });

    public IReadOnlyList<PluginDescriptor> Plugins => _plugins;

    public void LoadFrom(string pluginsDirectory, IReadOnlyCollection<string> disabledIds)
    {
        if (!Directory.Exists(pluginsDirectory)) return;
        // Sorted so the load order (and menu order) does not depend on the file system.
        foreach (var dir in Directory.GetDirectories(pluginsDirectory).Order(StringComparer.OrdinalIgnoreCase))
        {
            var dll = Path.Combine(dir, Path.GetFileName(dir) + ".dll");
            if (File.Exists(dll)) LoadAssembly(dll, disabledIds);
            else _log($"Plugins: '{dir}' has no {Path.GetFileName(dll)}, skipped.");
        }
    }

    public void LoadAssembly(string path, IReadOnlyCollection<string> disabledIds)
    {
        Assembly assembly;
        try
        {
            var context = new PluginLoadContext(Path.GetFullPath(path), SharedAssemblies);
            assembly = context.LoadFromAssemblyPath(Path.GetFullPath(path));
        }
        catch (Exception ex) when (ex is BadImageFormatException or FileLoadException or FileNotFoundException or InvalidOperationException)
        {
            var id = Path.GetFileNameWithoutExtension(path);
            _plugins.Add(new PluginDescriptor(id, id, "", "", "", path) { State = PluginState.Failed, Error = ex.Message });
            _log($"Plugins: cannot load '{path}': {ex.Message}");
            return;
        }

        foreach (var type in PluginTypes(assembly))
        {
            var info = type.GetCustomAttribute<GameCatalogPluginAttribute>();
            if (info is null)
            {
                _log($"Plugins: {type.FullName} has no [GameCatalogPlugin] attribute, skipped.");
                continue;
            }
            var descriptor = new PluginDescriptor(info.Id, info.Name, info.Version, info.Author, info.Description, path);
            _plugins.Add(descriptor);
            if (disabledIds.Contains(info.Id))
            {
                descriptor.State = PluginState.Disabled;
            }
            else if (info.ApiVersion != PluginApi.Version)
            {
                descriptor.State = PluginState.Incompatible;
                descriptor.Error = $"Plugin API {info.ApiVersion}, application API {PluginApi.Version}.";
            }
            else if (_plugins.Any(p => p != descriptor && p.Id == info.Id && p.State == PluginState.Loaded))
            {
                descriptor.State = PluginState.Failed;
                descriptor.Error = $"Another plugin already uses the id '{info.Id}'.";
            }
            else
            {
                Initialize(type, descriptor);
            }
            _log($"Plugins: {descriptor.Id} {descriptor.Version} — {descriptor.State}{(descriptor.Error is null ? "" : ": " + descriptor.Error)}");
        }
    }

    private void Initialize(Type type, PluginDescriptor descriptor)
    {
        try
        {
            var plugin = (IPlugin)Activator.CreateInstance(type)!;
            plugin.Initialize(hostFactory(descriptor));
            descriptor.State = PluginState.Loaded;
        }
        catch (Exception ex)   // isolation boundary: any plugin failure is reported, never propagated
        {
            registry.RemoveAll(descriptor.Id);
            descriptor.State = PluginState.Failed;
            descriptor.Error = (ex is TargetInvocationException { InnerException: { } inner } ? inner : ex).Message;
        }
    }

    private IEnumerable<Type> PluginTypes(Assembly assembly)
    {
        Type?[] types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            _log($"Plugins: some types of {assembly.GetName().Name} could not be loaded: {ex.LoaderExceptions.FirstOrDefault()?.Message}");
            types = ex.Types;
        }
        return types.OfType<Type>().Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IPlugin).IsAssignableFrom(t));
    }
}
