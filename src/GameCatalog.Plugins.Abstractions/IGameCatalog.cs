using GameCatalog.Core.Models;

namespace GameCatalog.Plugins.Abstractions;

/// <summary>Read/write access to the open catalog. Changes made through it are undoable and refresh the views.</summary>
public interface IGameCatalog
{
    string FilePath { get; }
    IReadOnlyList<Game> Games { get; }
    IReadOnlyList<CustomFieldDefinition> CustomFieldDefinitions { get; }
    Game? Find(int number);
    int NextNumber();
    /// <summary>Adds a game (a free number is assigned when Number is not positive).</summary>
    void Add(Game game);
    void Remove(Game game);
    /// <summary>Call after modifying games in place; null means "many games changed".</summary>
    void NotifyChanged(Game? game = null);
}
