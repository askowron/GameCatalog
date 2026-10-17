using System.Text.Json;
using GameCatalog.Core.Models;
using GameCatalog.Plugins.Abstractions;

namespace GameCatalog.Plugin.Sample;

/// <summary>Exports the visible games as an indented JSON array.</summary>
internal sealed class JsonGameExporter(IPluginHost host) : IGameExporter
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public string Name => host.Translator.Get("Plugin.sample.ExportJson");
    public string FileFilter => "JSON (*.json)|*.json";

    public void Export(IReadOnlyList<Game> games, ExportContext context) =>
        File.WriteAllText(context.Path, JsonSerializer.Serialize(games, Options));
}
