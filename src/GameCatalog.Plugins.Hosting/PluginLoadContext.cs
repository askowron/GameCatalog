using System.Reflection;
using System.Runtime.Loader;

namespace GameCatalog.Plugins.Hosting;

/// <summary>
/// Isolated load context per plugin: the plugin's private dependencies come from its folder,
/// while the shared contract assemblies come from the application so types match.
/// </summary>
internal sealed class PluginLoadContext(string pluginPath, IReadOnlySet<string> sharedAssemblies)
    : AssemblyLoadContext(name: Path.GetFileNameWithoutExtension(pluginPath), isCollectible: false)
{
    private readonly AssemblyDependencyResolver _resolver = new(pluginPath);

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (assemblyName.Name is { } name && sharedAssemblies.Contains(name)) return null;   // use the host's copy
        var path = _resolver.ResolveAssemblyToPath(assemblyName);
        return path is null ? null : LoadFromAssemblyPath(path);
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var path = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        return path is null ? IntPtr.Zero : LoadUnmanagedDllFromPath(path);
    }
}
