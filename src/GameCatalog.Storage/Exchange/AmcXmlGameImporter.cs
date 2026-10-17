using GameCatalog.Core.Models;
using GameCatalog.Localization;
using GameCatalog.Plugins.Abstractions;

namespace GameCatalog.Storage.Exchange;

/// <summary>Ant Movie Catalog XML import with movie → game field mapping.</summary>
public sealed class AmcXmlGameImporter(ITranslator t) : IGameImporter
{
    public string Name => t.Get("Exchange.ImportAmc");
    public ImportSourceKind SourceKind => ImportSourceKind.File;
    public string FileFilter => t.Get("Filter.AmcXml");

    public IReadOnlyList<Game> Import(ImportContext context) => AmcXmlImporter.Import(context.Path, context.StartNumber);
}
