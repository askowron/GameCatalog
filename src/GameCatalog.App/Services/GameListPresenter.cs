using GameCatalog.Core.Catalog;
using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;

namespace GameCatalog.App.Services;

/// <summary>A visible list column.</summary>
public sealed class ListColumn(string tag, int width)
{
    public string Tag { get; } = tag;
    public int Width { get; set; } = width;
}

/// <summary>View options of the game list (AMC sort / group / grid fields state).</summary>
public sealed class ListViewState
{
    public string SortTag { get; set; } = "Title";
    public bool Ascending { get; set; } = true;
    /// <summary>Empty = no grouping.</summary>
    public string GroupTag { get; set; } = "";
    public bool Thumbnails { get; set; }
    public List<ListColumn> Columns { get; } = new() { new("Number", 40), new("Title", 210) };
    /// <summary>When set, only these game numbers are shown (find bar "show results").</summary>
    public HashSet<int>? FindOnly { get; set; }
}

public sealed record FindCriteria(string FieldTag, string Text, bool WholeField, bool Reverse);
public sealed record GameRow(Game Game, IReadOnlyList<string> Cells, string GroupKey);
public sealed record GameGroup(string Key, string Caption);
public sealed record ListViewModel(IReadOnlyList<GameRow> Rows, IReadOnlyList<GameGroup> Groups, int Total);

/// <summary>Computes what the game list shows; free of WinForms so it can be tested.</summary>
public sealed class GameListPresenter(GameList games, PrefsStore prefs)
{
    /// <summary>Columns offered in View → Grid fields, with their default widths.</summary>
    public static readonly IReadOnlyList<(string Tag, int Width)> AvailableColumns = new[]
    {
        ("Number", 40), ("Title", 210), ("Developer", 110), ("Publisher", 110), ("Genre", 100),
        ("Platform", 80), ("ReleaseYear", 50), ("CompletionStatus", 105), ("UserRating", 55),
        ("PlaytimeHours", 60), ("Store", 110),
    };

    public ListViewState State { get; } = new();

    public List<Game> CurrentView()
    {
        IEnumerable<Game> q = games.Games;
        if (State.FindOnly is { } only) q = q.Where(g => only.Contains(g.Number));
        return GameList.Sort(q, State.SortTag, State.Ascending, prefs.NaturalSort).ToList();
    }

    public ListViewModel Build()
    {
        var view = CurrentView();
        var groups = new List<GameGroup>();
        if (State.GroupTag.Length > 0)
        {
            foreach (var grp in GameList.GroupBy(view, State.GroupTag))
            {
                var caption = GameList.GroupCaption(State.GroupTag, grp.Key, CatalogLabels.Current);
                groups.Add(new GameGroup(grp.Key, prefs.ShowGroupCount ? $"{caption} ({grp.Count()})" : caption));
            }
        }
        var rows = view.Select(g => new GameRow(
            g,
            State.Columns.Select(c => CellText(g, c.Tag)).ToList(),
            State.GroupTag.Length > 0 ? GameList.GroupKeyOf(g, State.GroupTag) : "")).ToList();
        return new ListViewModel(rows, groups, games.Games.Count);
    }

    /// <summary>Title as configured in preferences (AMC title formatting).</summary>
    public string FormatTitle(Game g) => prefs.TitleFormat switch
    {
        "Original" => g.Title,
        "Localized" => string.IsNullOrWhiteSpace(g.LocalizedTitle) ? g.Title : g.LocalizedTitle,
        _ => g.DisplayTitle,
    };

    public string CellText(Game g, string tag) => tag == "Title" ? FormatTitle(g) : GameFieldValues.Get(g, tag);

    public static bool Matches(Game g, FindCriteria c)
    {
        var value = GameFieldValues.Get(g, c.FieldTag);
        bool hit = c.WholeField
            ? value.Equals(c.Text, StringComparison.OrdinalIgnoreCase)
            : value.Contains(c.Text, StringComparison.OrdinalIgnoreCase);
        return c.Reverse ? !hit : hit;
    }

    public HashSet<int> FindAll(FindCriteria c) =>
        games.Games.Where(g => Matches(g, c)).Select(g => g.Number).ToHashSet();

    public Game? FindFirst(FindCriteria c) => CurrentView().FirstOrDefault(g => Matches(g, c));

    /// <summary>Next match after the current game in view order, wrapping around.</summary>
    public Game? FindNext(FindCriteria c, int? currentNumber)
    {
        var view = CurrentView();
        if (view.Count == 0) return null;
        int start = 0;
        if (currentNumber is int n)
        {
            int i = view.FindIndex(g => g.Number == n);
            if (i >= 0) start = (i + 1) % view.Count;
        }
        for (int k = 0; k < view.Count; k++)
        {
            var g = view[(start + k) % view.Count];
            if (Matches(g, c)) return g;
        }
        return null;
    }

    public void ToggleColumn(string tag, bool on)
    {
        if (on)
        {
            if (State.Columns.Any(c => c.Tag == tag)) return;
            var width = AvailableColumns.FirstOrDefault(c => c.Tag == tag).Width;
            State.Columns.Add(new ListColumn(tag, width > 0 ? width : 110));
        }
        else if (State.Columns.Count > 1)
        {
            State.Columns.RemoveAll(c => c.Tag == tag);
        }
    }
}
