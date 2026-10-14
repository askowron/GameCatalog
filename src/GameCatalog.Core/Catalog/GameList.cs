using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;

namespace GameCatalog.Core.Catalog;

/// <summary>
/// In-memory catalog — counterpart of AMC TMovieList plus its sort/group operations (main.pas).
/// Persistence is handled by GameCatalog.Storage.
/// </summary>
public sealed class GameList
{
    private readonly List<Game> _games = new();

    public IReadOnlyList<Game> Games => _games;
    public List<CustomFieldDefinition> CustomFieldDefs { get; } = new();

    public int NextNumber() => _games.Count == 0 ? 1 : _games.Max(g => g.Number) + 1;

    /// <summary>Adds a game; assigns the next free number when <see cref="Game.Number"/> is not positive.</summary>
    public void Add(Game game)
    {
        if (game.Number <= 0) game.Number = NextNumber();
        game.DateAdded ??= DateTime.Today;
        _games.Add(game);
    }

    public bool Remove(int number) => _games.RemoveAll(g => g.Number == number) > 0;

    /// <summary>Removes this instance only — other games with the same number stay.</summary>
    public bool Remove(Game game) => _games.Remove(game);

    public Game? Find(int number) => _games.FirstOrDefault(g => g.Number == number);

    public void Clear() => _games.Clear();

    public void ReplaceAll(IEnumerable<Game> games)
    {
        // Snapshot before Clear: callers often pass a deferred query over this very list.
        var snapshot = games.ToList();
        _games.Clear();
        _games.AddRange(snapshot);
    }

    /// <summary>Distinct non-empty values of one field, sorted (feeds editor drop-downs like AMC lists).</summary>
    public IEnumerable<string> DistinctValues(Func<Game, string> selector) =>
        _games.Select(selector).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().OrderBy(s => s);

    // --- Sorting (AMC Sort) ---
    public static IEnumerable<Game> Sort(IEnumerable<Game> source, string tag, bool ascending = true, bool natural = false)
    {
        if (natural && tag == "Title")
            return ascending
                ? source.OrderBy(g => g.Title, NaturalStringComparer.Instance)
                : source.OrderByDescending(g => g.Title, NaturalStringComparer.Instance);
        Func<Game, object?> key = tag switch
        {
            "Number" => g => g.Number,
            "Title" => g => g.Title,
            "Genre" => g => g.Genre,
            "Platform" => g => g.Platform,
            "Developer" => g => g.Developer,
            "Publisher" => g => g.Publisher,
            "ReleaseYear" => g => g.ReleaseYear,
            "UserRating" => g => g.UserRating,
            "CriticRating" => g => g.CriticRating,
            "PlaytimeHours" => g => g.PlaytimeHours,
            "CompletionStatus" => g => g.CompletionStatus,
            "DateAdded" => g => g.DateAdded,
            "LastPlayed" => g => g.LastPlayed,
            "Store" => g => g.Store,
            _ => g => g.Number,
        };
        return ascending ? source.OrderBy(key) : source.OrderByDescending(key);
    }

    // --- Grouping (AMC GroupBy) ---

    /// <summary>
    /// Language-neutral group key of a game: empty string for "no value",
    /// enum name for <see cref="CompletionStatus"/>, raw field text otherwise.
    /// </summary>
    public static string GroupKeyOf(Game g, string groupTag) => groupTag switch
    {
        "Genre" => g.Genre?.Trim() ?? "",
        "Platform" => g.Platform?.Trim() ?? "",
        "Developer" => g.Developer?.Trim() ?? "",
        "Publisher" => g.Publisher?.Trim() ?? "",
        "Store" => g.Store?.Trim() ?? "",
        "CompletionStatus" => g.CompletionStatus.ToString(),
        "ReleaseYear" => g.ReleaseYear > 0 ? g.ReleaseYear.ToString() : "",
        "Pegi" => g.Pegi?.Trim() ?? "",
        "GameMode" => g.GameMode?.Trim() ?? "",
        "Engine" => g.Engine?.Trim() ?? "",
        _ => "",
    };

    public static IEnumerable<IGrouping<string, Game>> GroupBy(IEnumerable<Game> source, string groupTag) =>
        source.GroupBy(g => GroupKeyOf(g, groupTag)).OrderBy(g => g.Key, StringComparer.CurrentCulture);

    /// <summary>Caption shown for a group key in the current language.</summary>
    public static string GroupCaption(string groupTag, string key, ICatalogLabels labels)
    {
        if (key.Length == 0) return labels.NoValue;
        if (groupTag == "CompletionStatus" && Enum.TryParse<CompletionStatus>(key, out var s)) return labels.Status(s);
        return key;
    }
}
