using GameCatalog.Core.Models;

namespace GameCatalog.Plugins.Abstractions;

/// <summary>The user's current position in the catalog.</summary>
public interface IGameSelection
{
    Game? Current { get; }
    /// <summary>Games visible in the list, in display order.</summary>
    IReadOnlyList<Game> Visible { get; }
    IReadOnlyList<Game> Checked { get; }
    void Select(int number);
    event EventHandler? CurrentChanged;
}
