namespace GameCatalog.App.Plugins;

/// <summary>Parses ICatalogCommand.MenuPath ("Tools/My plugin") into a top-level menu and submenus.</summary>
public static class PluginMenuPath
{
    private static readonly string[] Roots = { "File", "Game", "View", "Tools", "Help" };

    public static (string Root, IReadOnlyList<string> Submenus) Parse(string? path)
    {
        var parts = (path ?? "").Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0) return ("Tools", Array.Empty<string>());
        var root = Roots.FirstOrDefault(r => r.Equals(parts[0], StringComparison.OrdinalIgnoreCase));
        return root is null ? ("Tools", parts[1..]) : (root, parts[1..]);
    }
}
