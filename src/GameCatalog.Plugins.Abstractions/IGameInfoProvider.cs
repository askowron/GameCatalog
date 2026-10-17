using GameCatalog.Core.Models;

namespace GameCatalog.Plugins.Abstractions;

/// <summary>Interaction available to an info provider while it runs.</summary>
public interface IInfoProviderUi
{
    /// <summary>Lets the user pick one option; -1 when cancelled.</summary>
    int PickList(string title, IList<string> options);
    void ShowMessage(string message);
}

/// <summary>
/// Fetches information about a game (an alternative to .gcsx scripts, e.g. a native API client).
/// Receives a copy of the game; the result is shown to the user before anything is applied.
/// </summary>
public interface IGameInfoProvider
{
    string Name { get; }
    Task<GameInfoResult> FetchAsync(Game game, IInfoProviderUi ui, CancellationToken cancellationToken);
}
