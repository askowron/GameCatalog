using GameCatalog.Plugins.Abstractions;
using GameCatalog.Plugins.Abstractions.WinForms;

namespace GameCatalog.Plugin.Sample;

/// <summary>Demonstrates every extension point of GameCatalog.</summary>
[GameCatalogPlugin("sample", "Sample plugin", "1.0", Author = "GameCatalog", Description = "Demonstrates every extension point.")]
public sealed class SamplePlugin : IPlugin
{
    public void Initialize(IPluginHost host)
    {
        host.Register(new JsonGameImporter(host));
        host.Register(new JsonGameExporter(host));
        host.Register(new SampleInfoProvider(host));
        host.Register(new MarkGamesWithoutCoverCommand(host));
        if (host is IWinFormsPluginHost desktop) desktop.Register(new GameSummaryPanel(host));
        host.Log("Sample plugin initialized.");
    }
}
