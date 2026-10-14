namespace GameCatalog.Core.Fields;

/// <summary>
/// Storage format of YES/NO custom fields. New values are stored as "1"; legacy catalogs
/// written by the Polish-only version stored "Tak", which must still read as true.
/// </summary>
public static class YesNo
{
    public const string StoredTrue = "1";

    private static readonly string[] TrueValues = { "1", "tak", "true", "yes" };

    public static bool Parse(string? value) =>
        value is not null && TrueValues.Contains(value.Trim(), StringComparer.OrdinalIgnoreCase);
}
