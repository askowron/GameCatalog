using GameCatalog.Core.Models;
using GameCatalog.Plugins.Abstractions;

namespace GameCatalog.Plugin.Sample;

/// <summary>Returns test data without network access — shows the info-provider flow end to end.</summary>
internal sealed class SampleInfoProvider(IPluginHost host) : IGameInfoProvider
{
    public string Name => host.Translator.Get("Plugin.sample.InfoProvider");

    public Task<GameInfoResult> FetchAsync(Game game, IInfoProviderUi ui, CancellationToken cancellationToken)
    {
        var result = new GameInfoResult { Success = true };
        result.Fields["Comments"] = host.Translator.Get("Plugin.sample.InfoComment", game.Title, DateTime.Today.ToString("yyyy-MM-dd"));
        result.Log.Add("Sample provider ran for " + game.Title);
        return Task.FromResult(result);
    }
}
