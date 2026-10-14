using System.Text.Json;
using GameCatalog.Core.Models;

namespace GameCatalog.App.Services;

/// <summary>Text format used to copy/paste whole games through the clipboard.</summary>
public static class GameClipboard
{
    public const string Header = "GAMECATALOG1:";

    public static string Serialize(Game game) => Header + JsonSerializer.Serialize(game);

    /// <summary>The pasted game with Number = 0 (a new number is assigned on add), or null.</summary>
    public static Game? TryDeserialize(string? text)
    {
        if (text is null || !text.StartsWith(Header, StringComparison.Ordinal)) return null;
        try
        {
            var game = JsonSerializer.Deserialize<Game>(text[Header.Length..]);
            if (game is null || string.IsNullOrWhiteSpace(game.Title)) return null;
            game.Number = 0;
            return game;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
