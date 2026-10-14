using System.Text.Json.Serialization;

namespace GameCatalog.Localization;

/// <summary>JSON model of Languages\&lt;code&gt;.json.</summary>
public sealed class LanguageFile
{
    [JsonPropertyName("language")] public Header Language { get; set; } = new();
    [JsonPropertyName("forms")] public Dictionary<string, Dictionary<string, string>> Forms { get; set; } = new(StringComparer.Ordinal);
    [JsonPropertyName("strings")] public Dictionary<string, string> Strings { get; set; } = new(StringComparer.Ordinal);

    public sealed class Header
    {
        [JsonPropertyName("code")] public string Code { get; set; } = "";
        [JsonPropertyName("name")] public string Name { get; set; } = "";
        [JsonPropertyName("author")] public string Author { get; set; } = "";
        [JsonPropertyName("version")] public string Version { get; set; } = "";
    }
}
