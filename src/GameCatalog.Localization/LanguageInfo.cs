namespace GameCatalog.Localization;

/// <summary>Header of a language file shown in the language selection window.</summary>
public sealed record LanguageInfo(string Code, string Name, string Author, string Version);
