namespace GameCatalog.Localization;

/// <summary>One translatable property of a form, collected for the translation template.</summary>
public sealed record FormKey(string Form, string Key, string DefaultText);
