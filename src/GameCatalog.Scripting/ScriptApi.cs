using System.Net;
using System.Text.RegularExpressions;
using GameCatalog.Core.Catalog;
using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;

namespace GameCatalog.Scripting;

/// <summary>
/// API dostępne w skryptach (obiekt Api) — odpowiednik funkcji rejestrowanych
/// w AMC getscript.pas (grupy HTML, pól, dodatków, wyborów i komunikatów),
/// z polami filmowymi przeportowanymi na growe (tagi jak w GameFields).
/// </summary>
public sealed class ScriptApi
{
    private static readonly HttpClient Http = new(new HttpClientHandler
    {
        AutomaticDecompression = DecompressionMethods.All,
        UseCookies = true,
        CookieContainer = new CookieContainer(),
    });

    static ScriptApi()
    {
        Http.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) GameCatalog/1.5");
        Http.Timeout = TimeSpan.FromSeconds(30);
    }

    private readonly Game _game;
    private readonly List<string> _log;
    private readonly Func<string, IList<string>, int>? _picker;
    private readonly Dictionary<string, string> _headers = new(StringComparer.OrdinalIgnoreCase);

    public ScriptApi(Game game, List<string> log, Func<string, IList<string>, int>? picker = null)
    {
        _game = game;
        _log = log;
        _picker = picker;
    }

    public Dictionary<string, string> Fields { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> CustomFields { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<GameExtra> Extras { get; } = new();
    public byte[]? Cover { get; private set; }

    /// <summary>Tytuł bieżącej gry (punkt startowy skryptu).</summary>
    public string Title => _game.Title;

    // ---- HTTP (jak GetPage/PostPage/GetPicture) ----
    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage req)
    {
        foreach (var h in _headers)
        {
            if (h.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)) continue;
            req.Headers.TryAddWithoutValidation(h.Key, h.Value);
        }
        return await Http.SendAsync(req).ConfigureAwait(false);
    }

    /// <summary>Dodatkowy nagłówek HTTP (np. Client-ID, Authorization dla API IGDB).</summary>
    public void SetRequestHeader(string name, string value) => _headers[name] = value;
    public void ClearRequestHeaders() => _headers.Clear();

    public async Task<string> GetPage(string url, string? referer = null)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        if (!string.IsNullOrEmpty(referer)) req.Headers.Referrer = new Uri(referer);
        using var res = await SendAsync(req).ConfigureAwait(false);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadAsStringAsync().ConfigureAwait(false);
    }

    public async Task<string> PostPage(string url, string formData)
    {
        using var content = new StringContent(formData, System.Text.Encoding.UTF8,
            "application/x-www-form-urlencoded");
        using var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
        using var res = await SendAsync(req).ConfigureAwait(false);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadAsStringAsync().ConfigureAwait(false);
    }

    /// <summary>Pobiera obrazek i ustawia jako okładkę (jak GetPicture).</summary>
    public async Task<bool> GetPicture(string url)
    {
        try
        {
            var bytes = await Http.GetByteArrayAsync(url).ConfigureAwait(false);
            if (bytes.Length < 100) return false;
            Cover = bytes;
            Log($"Pobrano okładkę ({bytes.Length / 1024} KB).");
            return true;
        }
        catch (Exception ex)
        {
            Log("Błąd pobierania okładki: " + ex.Message);
            return false;
        }
    }

    // ---- HTML (jak HTMLDecode/HTMLRemoveTags/URLEncode) ----
    public string HtmlDecode(string s) => WebUtility.HtmlDecode(s);
    public string HtmlRemoveTags(string s) => Regex.Replace(s ?? "", "<.*?>", "");
    public string UrlEncode(string s) => Uri.EscapeDataString(s ?? "");
    public string UrlDecode(string s) => Uri.UnescapeDataString(s ?? "");

    public string RegexFirst(string input, string pattern)
    {
        var m = Regex.Match(input ?? "", pattern, RegexOptions.Singleline);
        return m.Success ? (m.Groups.Count > 1 ? m.Groups[1].Value : m.Value) : "";
    }

    public List<string> RegexAll(string input, string pattern)
    {
        return Regex.Matches(input ?? "", pattern, RegexOptions.Singleline)
            .Select(m => m.Groups.Count > 1 ? m.Groups[1].Value : m.Value).ToList();
    }

    public string Between(string input, string start, string end)
    {
        if (input is null) return "";
        int i = input.IndexOf(start, StringComparison.OrdinalIgnoreCase);
        if (i < 0) return "";
        i += start.Length;
        int j = input.IndexOf(end, i, StringComparison.OrdinalIgnoreCase);
        return j < 0 ? "" : input.Substring(i, j - i);
    }

    // ---- Pola (jak SetField/GetField/SetCustomField) ----
    public void SetField(string tag, string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        Fields[tag] = value.Trim();
    }

    public string GetField(string tag) =>
        Fields.TryGetValue(tag, out var v) ? v : GameFieldValues.Get(_game, tag);

    public void SetCustomField(string tag, string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        CustomFields[tag] = value.Trim();
    }

    public string GetCustomField(string tag) =>
        CustomFields.TryGetValue(tag, out var v) ? v
        : _game.CustomValues.TryGetValue(tag, out var g) ? g : "";

    // ---- Dodatki (jak AddExtra) ----
    public void AddExtra(string title, string category = "", string description = "")
    {
        if (string.IsNullOrWhiteSpace(title)) return;
        Extras.Add(new GameExtra
        {
            Title = title.Trim(),
            Category = category?.Trim() ?? "",
            Description = description?.Trim() ?? "",
            CreatedBy = "skrypt",
        });
    }

    /// <summary>Parsuje JSON i zwraca root (dokument do zamknięcia przez Dispose).</summary>
    public System.Text.Json.JsonElement JsonRoot(string json,
        out System.Text.Json.JsonDocument doc)
    {
        doc = System.Text.Json.JsonDocument.Parse(json);
        return doc.RootElement;
    }

    /// <summary>Linki &lt;a class=...&gt; ze strony (np. wyniki DuckDuckGo).</summary>
    public List<(string Href, string Text)> FindLinks(string html, string cssClass)
    {
        var list = new List<(string, string)>();
        if (string.IsNullOrEmpty(html)) return list;
        foreach (Match m in Regex.Matches(html,
                     "<a[^>]*class=\"" + Regex.Escape(cssClass) + "\"[^>]*href=\"([^\"]+)\"[^>]*>(.*?)</a>",
                     RegexOptions.Singleline))
            list.Add((m.Groups[1].Value, m.Groups[2].Value));
        return list;
    }

    /// <summary>Wartość meta tagu (property/content w dowolnej kolejności).</summary>
    public string MetaContent(string page, string prop)
    {
        if (string.IsNullOrEmpty(page)) return "";
        var m = Regex.Match(page, "<meta[^>]*property=\"" + prop + "\"[^>]*content=\"([^\"]+)\"");
        if (m.Success) return m.Groups[1].Value;
        m = Regex.Match(page, "<meta[^>]*content=\"([^\"]+)\"[^>]*property=\"" + prop + "\"");
        return m.Success ? m.Groups[1].Value : "";
    }

    /// <summary>Blok JSON-LD zawierający wskazany tekst (np. VideoGame).</summary>
    public string ScriptJson(string page, string mustContain)
    {
        if (string.IsNullOrEmpty(page)) return "";
        foreach (Match m in Regex.Matches(page,
                     "<script[^>]*type=\"application/ld\\+json\"[^>]*>(.*?)</script>",
                     RegexOptions.Singleline))
            if (m.Groups[1].Value.Contains(mustContain)) return m.Groups[1].Value;
        return "";
    }

    /// <summary>
    /// Wyszukiwanie linków (DuckDuckGo, awaryjnie Bing RSS) — odpowiednik
    /// wyszukiwania w skryptach AMC. Zwraca unikalne (Url, Tytuł) z hosta.
    /// </summary>
    public async Task<List<(string Url, string Name)>> SearchLinks(string query, string hostPart)
    {
        var found = new List<(string Url, string Name)>();
        void Add(string url, string name)
        {
            url = (url ?? "").Trim();
            name = WebUtility.HtmlDecode(Regex.Replace(name ?? "", "<.*?>", "")).Trim();
            if (url.Length == 0 || !url.Contains(hostPart, StringComparison.OrdinalIgnoreCase)) return;
            if (found.Any(x => x.Url == url)) return;
            found.Add((url, name));
        }
        try
        {
            var ddg = await GetPage("https://html.duckduckgo.com/html/?q=" + Uri.EscapeDataString(query))
                .ConfigureAwait(false);
            if (!ddg.Contains("anomaly"))
            {
                foreach (var (href, text) in FindLinks(ddg, "result__a"))
                {
                    var uddg = Between(href + "&", "uddg=", "&");
                    var url = uddg.Length > 0 ? Uri.UnescapeDataString(uddg) : href;
                    if (url.StartsWith("//")) url = "https:" + url;
                    Add(url, text);
                }
                if (found.Count > 0) return found;
            }
            else Log("DuckDuckGo ogranicza zapytania, próbuję Bing.");
        }
        catch (Exception ex) { Log("Błąd DuckDuckGo: " + ex.Message); }
        try
        {
            var rss = await GetPage("https://www.bing.com/search?format=rss&q=" + Uri.EscapeDataString(query))
                .ConfigureAwait(false);
            foreach (Match m in Regex.Matches(rss, "<item>(.*?)</item>", RegexOptions.Singleline))
            {
                var item = m.Groups[1].Value;
                Add(Between(item, "<link>", "</link>").Trim(), Between(item, "<title>", "</title>"));
            }
        }
        catch (Exception ex) { Log("Błąd Bing: " + ex.Message); }
        return found;
    }

    // ---- Wybór i komunikaty (jak PickList/Input/ShowMessage) ----
    public int PickList(string title, IList<string> options)
    {
        if (_picker is null || options.Count == 0) return 0;
        try { return Math.Clamp(_picker(title, options), -1, options.Count - 1); }
        catch { return -1; }
    }

    public void ShowMessage(string message) => Log("[info] " + message);
    public void Log(string message) => _log.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
    public Task Sleep(int ms) => Task.Delay(ms);
}
