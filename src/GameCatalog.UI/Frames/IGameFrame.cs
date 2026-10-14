using GameCatalog.Core.Models;

namespace GameCatalog.UI.Frames;

public sealed class FrameModifiedEventArgs(string tag) : EventArgs
{
    /// <summary>Field tag that changed ("custom:&lt;tag&gt;" for custom fields).</summary>
    public string Tag { get; } = tag;
}

/// <summary>
/// An editor frame of the game info panel (AMC TMovieFrame family): shows a game and writes
/// edits back to it. Frames never touch the catalog; the host reacts to <see cref="Modified"/>.
/// </summary>
public interface IGameFrame
{
    void Bind(Game? game);
    /// <summary>Writes all pending editor values into the bound game.</summary>
    void Commit();
    event EventHandler<FrameModifiedEventArgs>? Modified;
}
