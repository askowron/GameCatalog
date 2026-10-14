namespace GameCatalog.App.Services;

/// <summary>Well-known folders of the application.</summary>
public static class AppPaths
{
    /// <summary>User data root: Documents\GameCatalog.</summary>
    public static string DataDirectory =>
        Ensure(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GameCatalog"));

    public static string LogsDirectory => Ensure(Path.Combine(DataDirectory, "Logs"));
    public static string TemplatesDirectory => Ensure(Path.Combine(DataDirectory, "Templates"));
    public static string PluginDataDirectory(string pluginId) => Ensure(Path.Combine(DataDirectory, "PluginData", pluginId));

    /// <summary>Shipped next to the executable.</summary>
    public static string LanguagesDirectory => Path.Combine(AppContext.BaseDirectory, "Languages");
    public static string PluginsDirectory => Path.Combine(AppContext.BaseDirectory, "Plugins");

    private static string Ensure(string dir)
    {
        Directory.CreateDirectory(dir);
        return dir;
    }
}
