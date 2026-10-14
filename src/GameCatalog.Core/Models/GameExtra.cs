namespace GameCatalog.Core.Models;

/// <summary>
/// Add-on / DLC — counterpart of AMC TMovieExtra (Extras).
/// For games: story DLC, expansions, passes, mods, soundtracks.
/// </summary>
public sealed class GameExtra
{
    public int Id { get; set; }
    public int GameNumber { get; set; }
    public bool Checked { get; set; }
    public string Tag { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Comments { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public byte[]? CoverImage { get; set; }

    public GameExtra Clone() => (GameExtra)MemberwiseClone();
}
