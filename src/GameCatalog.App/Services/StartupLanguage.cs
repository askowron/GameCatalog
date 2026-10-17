using System.Globalization;
using GameCatalog.Localization;

namespace GameCatalog.App.Services;

/// <summary>Language used at start: the user's choice, otherwise the system language when a translation exists.</summary>
public static class StartupLanguage
{
    /// <param name="preferred">Language chosen in Tools → Language; empty means "follow the system".</param>
    public static string Resolve(string? preferred, IReadOnlyList<LanguageInfo> available, CultureInfo uiCulture, string defaultCode)
    {
        bool Exists(string code) => available.Any(l => l.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(preferred) && Exists(preferred)) return preferred;
        string system = uiCulture.TwoLetterISOLanguageName;
        return Exists(system) ? system : defaultCode;
    }
}
