using GameCatalog.Core.Models;
using GameCatalog.Localization;
using GameCatalog.Plugins.Abstractions;

namespace GameCatalog.Storage.Exchange;

/// <summary>CSV export of the visible games.</summary>
public sealed class CsvGameExporter(ITranslator t) : IGameExporter
{
    public string Name => t.Get("Exchange.ExportCsv");
    public string FileFilter => t.Get("Filter.Csv");

    public void Export(IReadOnlyList<Game> games, ExportContext context) =>
        CatalogTransfer.ExportCsv(games, context.Path, BuiltInExchange.Delimiter(context.Options));
}
