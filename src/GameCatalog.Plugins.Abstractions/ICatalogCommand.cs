namespace GameCatalog.Plugins.Abstractions;

/// <summary>A command added to the menu (and to the plugin toolbar when it has an icon).</summary>
public interface ICatalogCommand
{
    string Id { get; }
    /// <summary>Menu path: "File", "Game", "View", "Tools" or "Help", optionally followed by submenus, e.g. "Tools/My plugin".</summary>
    string MenuPath { get; }
    string Text { get; }
    string? Hint { get; }
    /// <summary>16x16 PNG, or null.</summary>
    byte[]? Icon { get; }
    /// <summary>Shortcut such as "Ctrl+Shift+I", or null.</summary>
    string? Shortcut { get; }
    bool CanExecute(IPluginHost host);
    void Execute(IPluginHost host);
}
