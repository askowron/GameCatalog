namespace GameCatalog.Core.Models;

/// <summary>
/// Status ukończenia gry (odpowiednik AMC "DateWatched" rozszerzony o workflow gracza).
/// </summary>
public enum CompletionStatus
{
    Backlog = 0,      // do zagrania
    Playing = 1,      // w trakcie
    Completed = 2,    // ukończona
    Completed100 = 3, // ukończona na 100% / platyna
    Paused = 4,       // wstrzymana
    Dropped = 5       // porzucona
}

/// <summary>Platforma sprzętowa (pole growе — w AMC odpowiadało VideoFormat/Resolution).</summary>
public static class Platforms
{
    public static readonly string[] All =
    {
        "PC", "PS5", "PS4", "Xbox Series", "Xbox One",
        "Switch", "Switch 2", "Steam Deck", "Mobile", "Retro", "Inna"
    };
}

/// <summary>Tryb gry (w AMC: AudioFormat).</summary>
public static class GameModes
{
    public static readonly string[] All =
    {
        "Single-player", "Multiplayer", "Co-op", "Single + Multi", "MMO", "Party"
    };
}

/// <summary>Sklep / źródło pozyskania (w AMC: Source).</summary>
public static class Stores
{
    public static readonly string[] All =
    {
        "Steam", "GOG", "Epic", "Ubisoft Connect", "EA App", "Battle.net",
        "PS Store", "Xbox Store", "Nintendo eShop", "Pudełko", "Inne"
    };
}

/// <summary>Typ nośnika (w AMC: MediaType).</summary>
public static class MediaTypes
{
    public static readonly string[] All = { "Cyfrowa", "Fizyczna", "Subskrypcja" };
}

/// <summary>Kategoria dodatku/DLC (odpowiednik AMC Extra.Category).</summary>
public static class ExtraCategories
{
    public static readonly string[] All =
    {
        "DLC fabularne", "Rozszerzenie", "Przepustka sezonowa",
        "Kosmetyczne", "Mod", "Ścieżka dźwiękowa", "Inne"
    };
}
