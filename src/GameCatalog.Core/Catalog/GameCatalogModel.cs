using GameCatalog.Core.Models;

namespace GameCatalog.Core.Catalog;

/// <summary>
/// WMemory repozytorium katalogu — odpowiednik AMC TMovieList + operacje
/// filtrowania/sortowania/grupowania (main.pas: filter/sort/group).
/// Persistencją zajmuje się Storage (SQLite).
/// </summary>
public sealed class GameCatalogModel
{
    private readonly List<Game> _games = new();

    public IReadOnlyList<Game> Games => _games;
    public List<CustomFieldDefinition> CustomFieldDefs { get; } = new();

    public int NextNumber() => _games.Count == 0 ? 1 : _games.Max(g => g.Number) + 1;

    public void Add(Game game)
    {
        if (game.Number <= 0) game.Number = NextNumber();
        game.DateAdded ??= DateTime.Today;
        _games.Add(game);
    }

    public bool Remove(int number) => _games.RemoveAll(g => g.Number == number) > 0;

    public Game? Find(int number) => _games.FirstOrDefault(g => g.Number == number);

    public void Clear() => _games.Clear();

    public void ReplaceAll(IEnumerable<Game> games)
    {
        // Snapshot PRZED Clear — wywołania z games.OrderBy(...) na żywej liście
        // inaczej wycięłyby same siebie (deferred execution).
        var snapshot = games.ToList();
        _games.Clear();
        _games.AddRange(snapshot);
    }

    // --- Filtrowanie (odpowiednik AMC Filter) ---
    public IEnumerable<Game> Filter(
        string? text = null,
        string? genre = null,
        string? platform = null,
        CompletionStatus? status = null,
        bool? checkedOnly = null)
    {
        IEnumerable<Game> q = _games;
        if (!string.IsNullOrWhiteSpace(text))
        {
            var t = text.Trim().ToLowerInvariant();
            q = q.Where(g =>
                (g.Title ?? string.Empty).ToLowerInvariant().Contains(t) ||
                (g.LocalizedTitle ?? string.Empty).ToLowerInvariant().Contains(t) ||
                (g.Developer ?? string.Empty).ToLowerInvariant().Contains(t) ||
                (g.Publisher ?? string.Empty).ToLowerInvariant().Contains(t) ||
                (g.Genre ?? string.Empty).ToLowerInvariant().Contains(t) ||
                (g.Description ?? string.Empty).ToLowerInvariant().Contains(t));
        }
        if (!string.IsNullOrWhiteSpace(genre) && genre != "(wszystkie)")
            q = q.Where(g => g.Genre == genre);
        if (!string.IsNullOrWhiteSpace(platform) && platform != "(wszystkie)")
            q = q.Where(g => g.Platform == platform);
        if (status.HasValue)
            q = q.Where(g => g.CompletionStatus == status.Value);
        if (checkedOnly == true)
            q = q.Where(g => g.Checked);
        return q;
    }

    // --- Sortowanie (odpowiednik AMC Sort) ---
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

    // --- Grupowanie (odpowiednik AMC GroupBy) ---
    public static IEnumerable<IGrouping<string, Game>> GroupBy(IEnumerable<Game> source, string tag)
    {
        Func<Game, string> key = tag switch
        {
            "Genre" => g => string.IsNullOrWhiteSpace(g.Genre) ? "(brak)" : g.Genre,
            "Platform" => g => string.IsNullOrWhiteSpace(g.Platform) ? "(brak)" : g.Platform,
            "Developer" => g => string.IsNullOrWhiteSpace(g.Developer) ? "(brak)" : g.Developer,
            "Publisher" => g => string.IsNullOrWhiteSpace(g.Publisher) ? "(brak)" : g.Publisher,
            "Store" => g => string.IsNullOrWhiteSpace(g.Store) ? "(brak)" : g.Store,
            "CompletionStatus" => g => StatusLabel(g.CompletionStatus),
            "ReleaseYear" => g => g.ReleaseYear > 0 ? g.ReleaseYear.ToString() : "(brak)",
            "Pegi" => g => string.IsNullOrWhiteSpace(g.Pegi) ? "(brak)" : g.Pegi,
            "GameMode" => g => string.IsNullOrWhiteSpace(g.GameMode) ? "(brak)" : g.GameMode,
            "Engine" => g => string.IsNullOrWhiteSpace(g.Engine) ? "(brak)" : g.Engine,
            _ => g => "(wszystkie)",
        };
        return source.GroupBy(key).OrderBy(g => g.Key);
    }

    public static string StatusLabel(CompletionStatus s) => s switch
    {
        CompletionStatus.Backlog => "Do zagrania",
        CompletionStatus.Playing => "Ogrywana",
        CompletionStatus.Completed => "Ukończona",
        CompletionStatus.Completed100 => "Ukończona 100%",
        CompletionStatus.Paused => "Wstrzymana",
        CompletionStatus.Dropped => "Porzucona",
        _ => s.ToString(),
    };

    public IEnumerable<string> DistinctGenres() =>
        _games.Select(g => g.Genre).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().OrderBy(s => s);

    public IEnumerable<string> DistinctPlatforms() =>
        _games.Select(g => g.Platform).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().OrderBy(s => s);
}

/// <summary>Porównywanie naturalne (jak AMC: liczby w tekście po wartości, nie leksykalnie).</summary>
public sealed class NaturalStringComparer : IComparer<string?>
{
    public static readonly NaturalStringComparer Instance = new();

    public int Compare(string? x, string? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x is null) return -1;
        if (y is null) return 1;
        int i = 0, j = 0;
        while (i < x.Length && j < y.Length)
        {
            bool dx = char.IsDigit(x[i]), dy = char.IsDigit(y[j]);
            if (dx && dy)
            {
                int sx = i, sy = j;
                while (i < x.Length && char.IsDigit(x[i])) i++;
                while (j < y.Length && char.IsDigit(y[j])) j++;
                string nx = x.Substring(sx, i - sx).TrimStart('0');
                string ny = y.Substring(sy, j - sy).TrimStart('0');
                int c = nx.Length.CompareTo(ny.Length);
                if (c != 0) return c;
                c = string.Compare(nx, ny, StringComparison.Ordinal);
                if (c != 0) return c;
                // Równe wartości — mniej zer wiodących wygrywa (stabilnie).
                c = (i - sx).CompareTo(j - sy);
                if (c != 0) return c;
            }
            else
            {
                int c = char.ToUpperInvariant(x[i]).CompareTo(char.ToUpperInvariant(y[j]));
                if (c != 0) return c;
                i++; j++;
            }
        }
        return (x.Length - i).CompareTo(y.Length - j);
    }
}
