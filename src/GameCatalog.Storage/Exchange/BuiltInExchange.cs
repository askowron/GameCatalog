using GameCatalog.Localization;
using GameCatalog.Plugins.Abstractions;

namespace GameCatalog.Storage.Exchange;

/// <summary>The application's own formats, exposed through the same contracts as plugin formats.</summary>
public static class BuiltInExchange
{
    public static (IReadOnlyList<IGameImporter> Importers, IReadOnlyList<IGameExporter> Exporters) Create(ITranslator t) =>
        (new IGameImporter[] { new FolderGameImporter(t), new CsvGameImporter(t), new AmcXmlGameImporter(t) },
         new IGameExporter[] { new CsvGameExporter(t), new HtmlGameExporter(t) });

    internal static char Delimiter(IReadOnlyDictionary<string, string> options) =>
        options.TryGetValue(ExchangeOptions.Delimiter, out var d) && d.Length > 0 ? (d == "TAB" ? '\t' : d[0]) : ';';
}
