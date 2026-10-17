using GameCatalog.Core.Models;
using GameCatalog.Localization;
using GameCatalog.Plugins.Abstractions;

namespace GameCatalog.Storage.Exchange;

/// <summary>Simple HTML table export of the visible games (stage 5 brings AMC templates).</summary>
public sealed class HtmlGameExporter(ITranslator t) : IGameExporter
{
    public string Name => t.Get("Exchange.ExportHtml");
    public string FileFilter => t.Get("Filter.Html");

    public void Export(IReadOnlyList<Game> games, ExportContext context) =>
        CatalogTransfer.ExportHtml(games, context.Path,
            context.Options.GetValueOrDefault(ExchangeOptions.Title) ?? t.Get("Export.HtmlTitle"),
            new[] { "#", t.Get("Field.Title"), t.Get("Field.Developer"), t.Get("Field.ReleaseYear"), t.Get("Field.Genre"),
                    t.Get("Field.Platform"), t.Get("Field.CompletionStatus"), t.Get("Field.UserRating"), t.Get("Field.PlaytimeHours") });
}
