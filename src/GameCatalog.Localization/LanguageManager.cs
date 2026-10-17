using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace GameCatalog.Localization;

/// <summary>
/// Loads Languages\*.json files and serves translations with a fallback chain
/// current language → default language → key. Missing or corrupt files never throw;
/// problems are reported through the log callback.
/// </summary>
public sealed class LanguageManager : ITranslator
{
    public const string DefaultLanguageCode = "pl";

    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private readonly Dictionary<string, LanguageFile> _files = new(StringComparer.OrdinalIgnoreCase);
    private readonly Action<string> _log;
    private LanguageFile _current;
    private readonly LanguageFile _default;

    public LanguageManager(string languagesDirectory, string defaultLanguage = DefaultLanguageCode, Action<string>? log = null)
    {
        _log = log ?? (_ => { });
        DefaultLanguage = defaultLanguage;
        CurrentLanguage = defaultLanguage;
        foreach (var file in ReadFiles(languagesDirectory))
            _files[file.Language.Code] = file;
        _default = _files.GetValueOrDefault(defaultLanguage) ?? new LanguageFile();
        _current = _default;
    }

    public string DefaultLanguage { get; }
    public string CurrentLanguage { get; private set; }

    public IReadOnlyList<LanguageInfo> Available =>
        _files.Values
            .Select(f => new LanguageInfo(f.Language.Code, f.Language.Name, f.Language.Author, f.Language.Version))
            .OrderBy(l => l.Name, StringComparer.CurrentCulture)
            .ToList();

    public event EventHandler? LanguageChanged;

    /// <summary>Switches the language; an unknown code selects the default language.</summary>
    public void SetLanguage(string code)
    {
        var file = code is not null ? _files.GetValueOrDefault(code) : null;
        var target = file?.Language.Code ?? DefaultLanguage;
        if (string.Equals(target, CurrentLanguage, StringComparison.OrdinalIgnoreCase)) return;
        CurrentLanguage = target;
        _current = file ?? _default;
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool TryGet(string key, out string value)
    {
        if (_current.Strings.TryGetValue(key, out var v) || _default.Strings.TryGetValue(key, out v))
        {
            value = v;
            return true;
        }
        value = key;
        return false;
    }

    public string Get(string key, params object?[] args)
    {
        if (!TryGet(key, out var text) || args is not { Length: > 0 }) return text;
        try
        {
            return string.Format(CultureInfo.CurrentCulture, text, args);
        }
        catch (FormatException)
        {
            _log($"Localization: invalid format string for key '{key}' in language '{CurrentLanguage}'.");
            return text;
        }
    }

    public string? GetFormValue(string form, string key)
    {
        if (_current.Forms.TryGetValue(form, out var f) && f.TryGetValue(key, out var v)) return v;
        if (_default.Forms.TryGetValue(form, out f) && f.TryGetValue(key, out v)) return v;
        return null;
    }

    /// <summary>
    /// Merges a plugin's language files: each "strings" key K becomes "Plugin.&lt;pluginId&gt;.K"
    /// in the language with the same code. Languages the host does not know are ignored.
    /// </summary>
    public void AddPluginLanguages(string pluginId, string directory)
    {
        foreach (var file in ReadFiles(directory))
        {
            if (!_files.TryGetValue(file.Language.Code, out var target)) continue;
            foreach (var (k, v) in file.Strings)
                target.Strings[$"Plugin.{pluginId}.{k}"] = v;
        }
    }

    /// <summary>
    /// Builds a translation template for the current language: every collected form key
    /// (current translation or designer text) plus every string known to the default or current language.
    /// </summary>
    public string ExportTemplate(IEnumerable<FormKey> formKeys)
    {
        var template = new LanguageFile
        {
            Language = new LanguageFile.Header
            {
                Code = CurrentLanguage,
                Name = _current.Language.Name,
                Author = _current.Language.Author,
                Version = _current.Language.Version,
            },
        };
        foreach (var fk in formKeys)
        {
            if (!template.Forms.TryGetValue(fk.Form, out var section))
                template.Forms[fk.Form] = section = new Dictionary<string, string>(StringComparer.Ordinal);
            section[fk.Key] = GetFormValue(fk.Form, fk.Key) ?? fk.DefaultText;
        }
        // Keys known to the default language but not collected from live windows (closed dialogs).
        foreach (var (form, keys) in _default.Forms)
        {
            if (!template.Forms.TryGetValue(form, out var section))
                template.Forms[form] = section = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var (key, value) in keys)
                section.TryAdd(key, GetFormValue(form, key) ?? value);
        }
        foreach (var key in _default.Strings.Keys.Union(_current.Strings.Keys).OrderBy(k => k, StringComparer.Ordinal))
            template.Strings[key] = _current.Strings.TryGetValue(key, out var v) ? v : _default.Strings[key];
        return JsonSerializer.Serialize(template, WriteOptions);
    }

    private IEnumerable<LanguageFile> ReadFiles(string directory)
    {
        if (!Directory.Exists(directory))
        {
            _log($"Localization: directory '{directory}' not found.");
            yield break;
        }
        foreach (var path in Directory.EnumerateFiles(directory, "*.json"))
        {
            var fileName = Path.GetFileName(path);
            LanguageFile? file = null;
            try
            {
                file = JsonSerializer.Deserialize<LanguageFile>(File.ReadAllText(path), ReadOptions);
            }
            catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
            {
                _log($"Localization: cannot read '{fileName}': {ex.Message}");
                continue;
            }
            if (file is null)
            {
                _log($"Localization: cannot read '{fileName}': file contains no language data.");
                continue;
            }
            if (Repair(file))
                _log($"Localization: '{fileName}' has missing or null entries; they were replaced with defaults.");
            if (file.Language.Code.Length == 0)
                file.Language.Code = Path.GetFileNameWithoutExtension(path);
            yield return file;
        }
    }

    /// <summary>
    /// Replaces JSON nulls (missing header, sections or entries) with empty values so that a
    /// malformed file cannot break lookups later. Returns true when anything was replaced.
    /// </summary>
    private static bool Repair(LanguageFile file)
    {
        var repaired = false;
        if (file.Language is null)
        {
            file.Language = new LanguageFile.Header();
            repaired = true;
        }
        file.Language.Code = Text(file.Language.Code, ref repaired);
        file.Language.Name = Text(file.Language.Name, ref repaired);
        file.Language.Author = Text(file.Language.Author, ref repaired);
        file.Language.Version = Text(file.Language.Version, ref repaired);

        if (file.Forms is null) repaired = true;
        var forms = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
        foreach (var (form, section) in file.Forms ?? new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal))
        {
            if (section is null)
            {
                repaired = true;
                continue;
            }
            forms[form] = NonNullValues(section, ref repaired);
        }
        file.Forms = forms;

        if (file.Strings is null) repaired = true;
        file.Strings = NonNullValues(file.Strings ?? new Dictionary<string, string>(StringComparer.Ordinal), ref repaired);
        return repaired;
    }

    private static string Text(string? value, ref bool repaired)
    {
        if (value is not null) return value;
        repaired = true;
        return "";
    }

    private static Dictionary<string, T> NonNullValues<T>(Dictionary<string, T> source, ref bool repaired) where T : class
    {
        var result = new Dictionary<string, T>(StringComparer.Ordinal);
        foreach (var (key, value) in source)
        {
            if (value is null) repaired = true;
            else result[key] = value;
        }
        return result;
    }
}
