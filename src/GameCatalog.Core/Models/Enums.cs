namespace GameCatalog.Core.Models;

/// <summary>
/// Completion status (AMC "DateWatched" extended with a player workflow).
/// </summary>
public enum CompletionStatus
{
    Backlog = 0,      // not started
    Playing = 1,      // in progress
    Completed = 2,    // finished
    Completed100 = 3, // 100% / platinum
    Paused = 4,       // on hold
    Dropped = 5       // abandoned
}

/// <summary>Hardware platform (game-specific field; in AMC it corresponded to VideoFormat/Resolution).</summary>
public static class Platforms
{
    public static readonly string[] All =
    {
        "PC", "PS5", "PS4", "Xbox Series", "Xbox One",
        "Switch", "Switch 2", "Steam Deck", "Mobile", "Retro", "Inna"
    };
}

/// <summary>Game mode (AMC: AudioFormat).</summary>
public static class GameModes
{
    public static readonly string[] All =
    {
        "Single-player", "Multiplayer", "Co-op", "Single + Multi", "MMO", "Party"
    };
}

/// <summary>Store / acquisition source (AMC: Source).</summary>
public static class Stores
{
    public static readonly string[] All =
    {
        "Steam", "GOG", "Epic", "Ubisoft Connect", "EA App", "Battle.net",
        "PS Store", "Xbox Store", "Nintendo eShop", "Pudełko", "Inne"
    };
}

/// <summary>Media type (AMC: MediaType).</summary>
public static class MediaTypes
{
    public static readonly string[] All = { "Cyfrowa", "Fizyczna", "Subskrypcja" };
}

/// <summary>Add-on/DLC category (counterpart of AMC Extra.Category).</summary>
public static class ExtraCategories
{
    public static readonly string[] All =
    {
        "DLC fabularne", "Rozszerzenie", "Przepustka sezonowa",
        "Kosmetyczne", "Mod", "Ścieżka dźwiękowa", "Inne"
    };
}
