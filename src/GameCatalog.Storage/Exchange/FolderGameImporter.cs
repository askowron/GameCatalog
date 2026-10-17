using GameCatalog.Core.Models;
using GameCatalog.Localization;
using GameCatalog.Plugins.Abstractions;

namespace GameCatalog.Storage.Exchange;

/// <summary>Folder import: every subfolder (or matching file) becomes a game (AMC import2 directory engine).</summary>
public sealed class FolderGameImporter(ITranslator t) : IGameImporter
{
    public string Name => t.Get("Exchange.ImportFolder");
    public ImportSourceKind SourceKind => ImportSourceKind.Folder;
    public string FileFilter => "";

    public IReadOnlyList<Game> Import(ImportContext context) =>
        CatalogTransfer.ImportFromFolder(context.Path, context.StartNumber,
            context.Options.GetValueOrDefault(ExchangeOptions.FilePatterns));
}
