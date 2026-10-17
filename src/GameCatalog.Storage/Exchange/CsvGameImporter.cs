using GameCatalog.Core.Models;
using GameCatalog.Localization;
using GameCatalog.Plugins.Abstractions;

namespace GameCatalog.Storage.Exchange;

/// <summary>CSV import (AMC import2 CSV engine).</summary>
public sealed class CsvGameImporter(ITranslator t) : IGameImporter
{
    public string Name => t.Get("Exchange.ImportCsv");
    public ImportSourceKind SourceKind => ImportSourceKind.File;
    public string FileFilter => t.Get("Filter.Csv");

    public IReadOnlyList<Game> Import(ImportContext context) =>
        CatalogTransfer.ImportCsv(context.Path, context.StartNumber, BuiltInExchange.Delimiter(context.Options));
}
