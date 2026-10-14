namespace GameCatalog.Localization;

/// <summary>Read access to the current language (AMC TAntTranslator counterpart).</summary>
public interface ITranslator
{
    /// <summary>Code of the active language, e.g. "pl".</summary>
    string CurrentLanguage { get; }

    /// <summary>
    /// Text of a key from the "strings" section formatted with <paramref name="args"/>.
    /// Falls back to the default language, then to the key itself — never throws.
    /// </summary>
    string Get(string key, params object?[] args);

    /// <summary>Unformatted text of a key; false (and the key as value) when no loaded language defines it.</summary>
    bool TryGet(string key, out string value);

    /// <summary>
    /// Text for a control property of a form ("forms" section), or null when no language defines it —
    /// the caller then keeps the text set in the Designer.
    /// </summary>
    string? GetFormValue(string form, string key);

    event EventHandler? LanguageChanged;
}
