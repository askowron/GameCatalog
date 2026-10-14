using GameCatalog.Localization;

namespace GameCatalog.App.Services;

/// <summary>
/// Process-wide services created in Program.Main. Designer-built forms need parameterless
/// constructors, so they read shared services from here.
/// </summary>
public static class AppServices
{
    public static LanguageManager Languages { get; set; } = null!;
    public static PrefsStore Prefs { get; set; } = null!;
}
