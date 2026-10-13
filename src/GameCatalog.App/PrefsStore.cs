using System.Text.Json;

namespace GameCatalog.App;

/// <summary>Własna wyszukiwarka internetowa — szablon URL z miejscem na zapytanie {q}.</summary>
public sealed record SearchSite(string Name, string Url);

/// <summary>Preferencje programu — odpowiednik opcji AMC (options.pas).</summary>
public sealed class PrefsStore
{
    public string Theme { get; set; } = AmcIcons.DefaultTheme;
    public List<string> RecentFiles { get; set; } = new();
    public string HtmlTemplate { get; set; } = "Szczegółowy";
    public bool ShowGroupCount { get; set; } = true;
    /// <summary>Automatyczne numerowanie (jak opcja AMC). Gdy false — przy dodawaniu pokazuj „Wybór numeru gry”.</summary>
    public bool AutoNumbering { get; set; } = false;
    /// <summary>Format tytułu na liście: Both (Oryginalny / Polski), Original, Localized.</summary>
    public string TitleFormat { get; set; } = "Both";
    public bool NaturalSort { get; set; } = true;
    public bool ForceFullRefresh { get; set; } = false;
    public bool ReopenLastFile { get; set; } = false;
    public bool FullRowSelect { get; set; } = true;
    public bool ShowGridLines { get; set; } = false;
    public bool ShowMenuIcons { get; set; } = true;
    public bool CoverVisible { get; set; } = true;
    public bool ShowExtraCategory { get; set; } = true;
    /// <summary>Separator CSV: ";" "," lub "TAB".</summary>
    public string CsvDelimiter { get; set; } = ";";
    public bool BackupOnSave { get; set; } = false;
    /// <summary>Wzorce plików EXE przy imporcie z folderu, rozdzielone średnikiem.</summary>
    public string MediaPatterns { get; set; } = "*.exe";
    /// <summary>Maks. szerokość wczytywanej okładki w px (0 = bez zmian).</summary>
    public int MaxCoverWidth { get; set; } = 0;
    /// <summary>Limit czasu skryptu w sekundach.</summary>
    public int ScriptTimeoutSec { get; set; } = 60;
    /// <summary>Własne wartości list rozwijanych (tag pola → wartości).</summary>
    public Dictionary<string, List<string>> CustomLists { get; set; } = new();
    /// <summary>Własne wyszukiwarki (puste = domyślne).</summary>
    public List<SearchSite> CustomSearchSites { get; set; } = new();

    public static List<SearchSite> DefaultSearchSites() => new()
    {
        new("Google", "https://www.google.com/search?q={q}"),
        new("Steam", "https://store.steampowered.com/search/?term={q}"),
        new("MobyGames", "https://www.mobygames.com/search/?q={q}"),
        new("HowLongToBeat", "https://howlongtobeat.com/?q={q}"),
    };

    public char CsvSeparator() => CsvDelimiter == "TAB" ? '\t' : string.IsNullOrEmpty(CsvDelimiter) ? ';' : CsvDelimiter[0];

    public static string PrefsPath()
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GameCatalog");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "prefs.json");
    }

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
