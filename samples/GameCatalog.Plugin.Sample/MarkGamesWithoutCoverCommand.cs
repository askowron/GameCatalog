using GameCatalog.Plugins.Abstractions;

namespace GameCatalog.Plugin.Sample;

/// <summary>Tools → "Check games without a cover".</summary>
internal sealed class MarkGamesWithoutCoverCommand(IPluginHost host) : ICatalogCommand
{
    public string Id => "sample.check-without-cover";
    public string MenuPath => "Tools";
    public string Text => host.Translator.Get("Plugin.sample.CheckWithoutCover");
    public string? Hint => host.Translator.Get("Plugin.sample.CheckWithoutCoverHint");
    public byte[]? Icon => null;
    public string? Shortcut => null;

    public bool CanExecute(IPluginHost h) => h.Catalog.Games.Count > 0;

    public void Execute(IPluginHost h)
    {
        foreach (var game in h.Catalog.Games) game.Checked = !game.HasCover;
        h.Catalog.NotifyChanged();
    }
}
