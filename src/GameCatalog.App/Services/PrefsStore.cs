using System.Text.Json;

namespace GameCatalog.App.Services;

/// <summary>Custom web search site: a URL template with a {q} placeholder for the query.</summary>
public sealed record SearchSite(string Name, string Url);

/// <summary>Application preferences (equivalent of the AMC options).</summary>
public sealed class PrefsStore
{
    /// <summary>Language code chosen by the user; empty until the first-run language selection.</summary>
    public string Language { get; set; } = "";
    public string Theme { get; set; } = AmcIcons.DefaultTheme;
    public List<string> RecentFiles { get; set; } = new();
    public string HtmlTemplate { get; set; } = "Szczegółowy";
    public bool ShowGroupCount { get; set; } = true;
    /// <summary>Automatic numbering (like the AMC option). When false, adding a game shows the "Choose game number" prompt.</summary>
    public bool AutoNumbering { get; set; } = false;
    /// <summary>Title format in the list: Both (original / localized), Original, Localized.</summary>
    public string TitleFormat { get; set; } = "Both";
    public bool NaturalSort { get; set; } = true;
    public bool ForceFullRefresh { get; set; } = false;
    public bool ReopenLastFile { get; set; } = false;
    public bool FullRowSelect { get; set; } = true;
    public bool ShowGridLines { get; set; } = false;
    public bool ShowMenuIcons { get; set; } = true;
    public bool CoverVisible { get; set; } = true;
    public bool ShowExtraCategory { get; set; } = true;
    /// <summary>CSV separator: ";", "," or "TAB".</summary>
    public string CsvDelimiter { get; set; } = ";";
    public bool BackupOnSave { get; set; } = false;
    /// <summary>EXE file patterns used when importing from a folder, separated by semicolons.</summary>
    public string MediaPatterns { get; set; } = "*.exe";
    /// <summary>Maximum width of a loaded cover in px (0 = unchanged).</summary>
    public int MaxCoverWidth { get; set; } = 0;
    /// <summary>Script time limit in seconds.</summary>
    public int ScriptTimeoutSec { get; set; } = 60;
    /// <summary>Custom drop-down list values (field tag to values).</summary>
    public Dictionary<string, List<string>> CustomLists { get; set; } = new();
    /// <summary>Custom search sites (empty = defaults).</summary>
    public List<SearchSite> CustomSearchSites { get; set; } = new();

    public static List<SearchSite> DefaultSearchSites() => new()
    {
        new("Google", "https://www.google.com/search?q={q}"),
        new("Steam", "https://store.steampowered.com/search/?term={q}"),
        new("MobyGames", "https://www.mobygames.com/search/?q={q}"),
        new("HowLongToBeat", "https://howlongtobeat.com/?q={q}"),
    };

    public char CsvSeparator() => CsvDelimiter == "TAB" ? '\t' : string.IsNullOrEmpty(CsvDelimiter) ? ';' : CsvDelimiter[0];

    public static string PrefsPath() => Path.Combine(AppPaths.DataDirectory, "prefs.json");

    public static PrefsStore Load()
    {
        try
        {
            var p = PrefsPath();
            if (File.Exists(p))
                return JsonSerializer.Deserialize<PrefsStore>(File.ReadAllText(p)) ?? new PrefsStore();
        }
        catch { }
        return new PrefsStore();
    }

    public void Save()
    {
        try { File.WriteAllText(PrefsPath(), JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true })); }
        catch { }
    }

    public void PushRecent(string path)
    {
        RecentFiles.RemoveAll(x => x.Equals(path, StringComparison.OrdinalIgnoreCase));
        RecentFiles.Insert(0, path);
        while (RecentFiles.Count > 5) RecentFiles.RemoveAt(RecentFiles.Count - 1);
    }
}
