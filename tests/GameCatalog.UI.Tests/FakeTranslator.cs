using GameCatalog.Localization;

namespace GameCatalog.UI.Tests;

internal sealed class FakeTranslator : ITranslator
{
    public Dictionary<string, string> Strings { get; } = new();
    public Dictionary<(string Form, string Key), string> Forms { get; } = new();
    public string CurrentLanguage { get; private set; } = "pl";
    public event EventHandler? LanguageChanged;

    public bool TryGet(string key, out string value)
    {
        if (Strings.TryGetValue(key, out var v)) { value = v; return true; }
        value = key;
        return false;
    }

    public string Get(string key, params object?[] args) => TryGet(key, out var v) ? string.Format(v, args) : key;
    public string? GetFormValue(string form, string key) => Forms.TryGetValue((form, key), out var v) ? v : null;

    public void Switch(string code)
    {
        CurrentLanguage = code;
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }
}
