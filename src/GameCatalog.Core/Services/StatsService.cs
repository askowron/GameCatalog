using GameCatalog.Core.Catalog;
using GameCatalog.Core.Models;

namespace GameCatalog.Core.Services;

/// <summary>Statystyki katalogu — odpowiednik AMC stats.pas, w wersji growej.</summary>
public sealed record CatalogStats(
    int Total,
    double TotalHours,
    double? AvgUserRating,
    double? AvgCriticRating,
    int CompletedCount,
    int PlayingCount,
    IReadOnlyDictionary<string, int> ByGenre,
    IReadOnlyDictionary<string, int> ByPlatform,
    IReadOnlyDictionary<string, int> ByStatus);

public static class StatsService
{
    public static CatalogStats Compute(IEnumerable<Game> games)
    {
        var list = games.ToList();
        double totalHours = list.Sum(g => g.PlaytimeHours);
        var user = list.Where(g => g.UserRating >= 0).Select(g => (double)g.UserRating / 10).ToList();
        var critic = list.Where(g => g.CriticRating >= 0).Select(g => (double)g.CriticRating / 10).ToList();

        static Dictionary<string, int> CountBy(IEnumerable<Game> src, Func<Game, string> key) =>
            src.GroupBy(g => string.IsNullOrWhiteSpace(key(g)) ? "(brak)" : key(g))
               .ToDictionary(g => g.Key, g => g.Count());

        return new CatalogStats(
            list.Count,
            totalHours,
            user.Count > 0 ? user.Average() : null,
            critic.Count > 0 ? critic.Average() : null,
            list.Count(g => g.CompletionStatus is CompletionStatus.Completed or CompletionStatus.Completed100),
            list.Count(g => g.CompletionStatus == CompletionStatus.Playing),
            CountBy(list, g => g.Genre),
            CountBy(list, g => g.Platform),
            list.GroupBy(g => GameCatalogModel.StatusLabel(g.CompletionStatus))
                .ToDictionary(g => g.Key, g => g.Count()));
    }
}
