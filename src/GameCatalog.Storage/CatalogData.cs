using GameCatalog.Core.Models;

namespace GameCatalog.Storage;

/// <summary>Everything stored in one catalog file.</summary>
public sealed record CatalogData(
    List<Game> Games,
    List<CustomFieldDefinition> CustomFieldDefs,
    List<LoanRecord> Loans);
