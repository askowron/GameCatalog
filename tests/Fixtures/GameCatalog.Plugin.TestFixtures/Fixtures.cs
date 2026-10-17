using GameCatalog.Core.Models;
using GameCatalog.Plugins.Abstractions;

namespace GameCatalog.Plugin.TestFixtures;

/// <summary>Registers an importer, then fails: the host must roll the registration back.</summary>
[GameCatalogPlugin("fixture.throws", "Throwing plugin", "1.0")]
public sealed class ThrowingPlugin : IPlugin
{
    public void Initialize(IPluginHost host)
    {
        host.Register(new NullImporter());
        throw new InvalidOperationException("Initialization failed on purpose.");
    }
}

/// <summary>Built for a future contract version: must be reported as incompatible.</summary>
[GameCatalogPlugin("fixture.future", "Future plugin", "1.0", ApiVersion = PluginApi.Version + 1)]
public sealed class FuturePlugin : IPlugin
{
    public void Initialize(IPluginHost host) => host.Register(new NullImporter());
}

/// <summary>No attribute: must be ignored.</summary>
public sealed class UnmarkedPlugin : IPlugin
{
    public void Initialize(IPluginHost host) => host.Register(new NullImporter());
}

internal sealed class NullImporter : IGameImporter
{
    public string Name => "Null";
    public ImportSourceKind SourceKind => ImportSourceKind.File;
    public string FileFilter => "*.*|*.*";
    public IReadOnlyList<Game> Import(ImportContext context) => Array.Empty<Game>();
}
