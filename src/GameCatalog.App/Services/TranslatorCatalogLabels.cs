using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;
using GameCatalog.Localization;

namespace GameCatalog.App.Services;

/// <summary>Core captions served from the language files.</summary>
public sealed class TranslatorCatalogLabels(ITranslator translator) : ICatalogLabels
{
    public string Field(string tag) => translator.TryGet("Field." + tag, out var text) ? text : tag;
    public string Status(CompletionStatus status) => translator.Get("Status." + status);
    public string Yes => translator.Get("Common.Yes");
    public string No => translator.Get("Common.No");
    public string NoValue => translator.Get("Common.None");
}
