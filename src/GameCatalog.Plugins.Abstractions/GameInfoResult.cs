using GameCatalog.Core.Models;

namespace GameCatalog.Plugins.Abstractions;

/// <summary>
/// Result of fetching information about a game — produced by .gcsx scripts and by
/// <see cref="IGameInfoProvider"/> plugins, shown to the user in the same results window
/// (AMC getscript_results). Nothing is applied to the game until the user confirms.
/// </summary>
public sealed class GameInfoResult
{
    public bool Success { get; set; }
    public string Error { get; set; } = "";
    public List<string> Log { get; set; } = new();
    /// <summary>Field values keyed by field tag (see GameFields).</summary>
    public Dictionary<string, string> Fields { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    /// <summary>Custom field values keyed by custom field tag.</summary>
    public Dictionary<string, string> CustomFields { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<GameExtra> Extras { get; set; } = new();
    /// <summary>Cover image bytes (PNG/JPEG) or null.</summary>
    public byte[]? Cover { get; set; }

    public bool HasData => Fields.Count > 0 || CustomFields.Count > 0 || Extras.Count > 0 || Cover is not null;

    public static GameInfoResult Failed(string error, List<string>? log = null) =>
        new() { Success = false, Error = error, Log = log ?? new() };
}
