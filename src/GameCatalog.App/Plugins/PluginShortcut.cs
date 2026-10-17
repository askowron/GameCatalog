namespace GameCatalog.App.Plugins;

/// <summary>Parses shortcut text such as "Ctrl+Shift+I".</summary>
public static class PluginShortcut
{
    public static bool TryParse(string? text, out Keys keys)
    {
        keys = Keys.None;
        if (string.IsNullOrWhiteSpace(text)) return false;
        try
        {
            keys = (Keys)(new KeysConverter().ConvertFromInvariantString(text) ?? Keys.None);
            return keys != Keys.None;
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or NotSupportedException)
        {
            keys = Keys.None;
            return false;
        }
    }
}
