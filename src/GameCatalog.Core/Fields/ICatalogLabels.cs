using GameCatalog.Core.Models;

namespace GameCatalog.Core.Fields;

/// <summary>
/// Language-dependent captions the core needs when it renders values as text
/// (field names, completion statuses, yes/no, the "(none)" bucket).
/// The UI layer supplies a translated implementation; the core never hard-codes a language.
/// </summary>
public interface ICatalogLabels
{
    /// <summary>Caption of the field identified by <paramref name="tag"/>.</summary>
    string Field(string tag);
    /// <summary>Caption of a completion status.</summary>
    string Status(CompletionStatus status);
    string Yes { get; }
    string No { get; }
    /// <summary>Caption of the group/statistics bucket for empty values (AMC "(none)").</summary>
    string NoValue { get; }
}

/// <summary>English captions taken from the field catalogue; used by tests and as a safe default.</summary>
public sealed class InvariantCatalogLabels : ICatalogLabels
{
    public string Field(string tag) => GameFields.Find(tag)?.DefaultCaption ?? tag;

    public string Status(CompletionStatus status) => status switch
    {
        CompletionStatus.Completed100 => "Completed 100%",
        _ => status.ToString(),
    };

    public string Yes => "Yes";
    public string No => "No";
    public string NoValue => "(none)";
}

/// <summary>Process-wide labels used by value formatting; replaced by the App at start-up.</summary>
public static class CatalogLabels
{
    public static ICatalogLabels Current { get; set; } = new InvariantCatalogLabels();
}
