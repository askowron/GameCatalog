using System.Text.Json;
using GameCatalog.Core.Models;
using GameCatalog.Plugins.Abstractions;

namespace GameCatalog.Plugin.Sample;

/// <summary>Imports a JSON array of games (the format written by <see cref="JsonGameExporter"/>).</summary>
internal sealed class JsonGameImporter(IPluginHost host) : IGameImporter
{
    public string Name => host.Translator.Get("Plugin.sample.ImportJson");
    public ImportSourceKind SourceKind => ImportSourceKind.File;
    public string FileFilter => "JSON (*.json)|*.json";

    public IReadOnlyList<Game> Import(ImportContext context)
    {
        var games = JsonSerializer.Deserialize<List<Game>>(File.ReadAllText(context.Path)) ?? new List<Game>();
        int number = context.StartNumber;
        foreach (var game in games) game.Number = number++;
        return games;
    }
}
