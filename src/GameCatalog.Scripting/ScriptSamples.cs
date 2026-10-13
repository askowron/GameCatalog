namespace GameCatalog.Scripting;

/// <summary>Przykładowe skrypty instalowane przy pierwszym użyciu (jak wbudowane skrypty AMC).</summary>
public static class ScriptSamples
{
    public static void EnsureSamples()
    {
        var dir = GameScript.ScriptsDir();
        var steam = Path.Combine(dir, "Steam.gcsx");
        if (!File.Exists(steam)) File.WriteAllText(steam, Steam);
        var wiki = Path.Combine(dir, "Wikipedia PL.gcsx");
        if (!File.Exists(wiki)) File.WriteAllText(wiki, Wikipedia);
        var igdb = Path.Combine(dir, "IGDB.gcsx");
        if (!File.Exists(igdb)) File.WriteAllText(igdb, Igdb);
        var gol = Path.Combine(dir, "GRY-Online.pl.gcsx");
        if (!File.Exists(gol)) File.WriteAllText(gol, GryOnline);
    }

    public const string Steam =
@"// Name: Steam
// Description: Pobiera dane ze sklepu Steam (developer, gatunek, oceny, opis, okładka).
// Version: 1.0
// Author: GameCatalog
// Site: https://store.steampowered.com
var q = Api.UrlEncode(Api.Title);
Api.Log(""Szukam: "" + Api.Title);
var suggest = await Api.GetPage(""https://store.steampowered.com/search/suggest?term="" + q + ""&f=games&cc=PL&l=polish"");
var pairs = System.Text.RegularExpressions.Regex.Matches(suggest, ""data-ds-appid=\""(\\d+)\"".*?match_name\"">([^<]+)"", System.Text.RegularExpressions.RegexOptions.Singleline)
    .Select(m => (Id: m.Groups[1].Value, Name: Api.HtmlDecode(m.Groups[2].Value).Trim()))
    .GroupBy(x => x.Id).Select(g => g.First()).ToList();
if (pairs.Count == 0) { Api.ShowMessage(""Nie znaleziono gry na Steam.""); return; }
int pick = 0;
if (pairs.Count > 1) {
    pick = Api.PickList(""Wybierz grę:"", pairs.Select(p => p.Name).ToList());
    if (pick < 0) return;
}
string appid = pairs[pick].Id;
Api.Log(""AppID: "" + appid);
var json = await Api.GetPage(""https://store.steampowered.com/api/appdetails?appids="" + appid + ""&cc=PL&l=polish"");
var root0 = Api.JsonRoot(json, out var doc);
if (!root0.TryGetProperty(appid, out var root) || !root.GetProperty(""success"").GetBoolean()) { doc.Dispose(); Api.ShowMessage(""Brak szczegółów.""); return; }
var data = root.GetProperty(""data"");
string S(JsonElement e, string p) => e.TryGetProperty(p, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString()! : """";
string[] SA(JsonElement e, string p, string sub) => e.TryGetProperty(p, out var v) && v.ValueKind == JsonValueKind.Array ? v.EnumerateArray().Select(x => sub == """" ? x.GetString()! : (x.TryGetProperty(sub, out var d) ? d.GetString()! : """")).Where(x => x != null).ToArray()! : Array.Empty<string>();
var devs = SA(data, ""developers"", """");
var pubs = SA(data, ""publishers"", """");
var genres = SA(data, ""genres"", ""description"");
if (devs.Length > 0) Api.SetField(""Developer"", string.Join("", "", devs));
if (pubs.Length > 0) Api.SetField(""Publisher"", string.Join("", "", pubs));
if (genres.Length > 0) Api.SetField(""Genre"", string.Join("", "", genres.Take(3)));
if (data.TryGetProperty(""metacritic"", out var mc) && mc.TryGetProperty(""score"", out var score)) Api.SetField(""CriticRating"", (score.GetInt32() / 10.0).ToString(""0.0""));
if (data.TryGetProperty(""release_date"", out var rdo) && rdo.TryGetProperty(""date"", out var rdd)) { var m = System.Text.RegularExpressions.Regex.Match(rdd.GetString() ?? """", ""\\d{4}""); if (m.Success) Api.SetField(""ReleaseYear"", m.Value); }
var desc = Api.HtmlRemoveTags(S(data, ""short_description"")).Trim();
if (desc.Length > 0) Api.SetField(""Description"", Api.HtmlDecode(desc));
Api.SetField(""URL"", ""https://store.steampowered.com/app/"" + appid + ""/"");
Api.SetField(""Store"", ""Steam"");
Api.SetField(""Platform"", ""PC"");
var img = S(data, ""header_image"");
if (img.Length > 0) await Api.GetPicture(img);
doc.Dispose();
Api.Log(""Gotowe."");
";

    public const string Igdb =
@"// Name: IGDB (API)
// Description: Pobiera dane z oficjalnego API IGDB. Wymaga darmowych kluczy Twitch - załóż aplikację na https://dev.twitch.tv/console , skopiuj Identyfikator klienta i Tajny klucz klienta, wklej je poniżej i uruchom ponownie.
// Version: 2.0
// Author: GameCatalog
// Site: https://api.igdb.com
string clientId = """";
string clientSecret = """";
if (clientId.Length == 0 || clientSecret.Length == 0) {
    Api.ShowMessage(""Skrypt IGDB wymaga darmowych kluczy Twitch. Załóż aplikację na dev.twitch.tv/console, skopiuj Identyfikator klienta i Tajny klucz klienta, wklej je na początku tego skryptu (Narzędzia > Skrypty) i uruchom ponownie."");
    return;
}
string Q = ((char)34).ToString();
string MapPlatformApi(string p) {
    p = (p ?? """").ToLowerInvariant();
    if (p.Contains(""windows"") || p == ""pc"") return ""PC"";
    if (p.Contains(""playstation 5"")) return ""PS5"";
    if (p.Contains(""playstation 4"")) return ""PS4"";
    if (p.Contains(""playstation 3"")) return ""PS3"";
    if (p.Contains(""playstation vita"")) return ""PS Vita"";
    if (p.Contains(""xbox series"")) return ""Xbox Series"";
    if (p.Contains(""xbox one"")) return ""Xbox One"";
    if (p.Contains(""xbox 360"")) return ""Xbox 360"";
    if (p.Contains(""switch"")) return ""Switch"";
    if (p.Contains(""wii u"")) return ""Wii U"";
    if (p.StartsWith(""wii"")) return ""Wii"";
    if (p.Contains(""3ds"")) return ""3DS"";
    if (p.Contains(""mac"")) return ""Mac"";
    if (p.Contains(""linux"")) return ""Linux"";
    if (p.Contains(""ios"") || p.Contains(""android"")) return ""Mobile"";
    return """";
}
string EName(JsonElement e) {
    if (e.TryGetProperty(""name"", out var n) && n.ValueKind == JsonValueKind.String) return n.GetString()!;
    return """";
}
int EYear(JsonElement e) {
    if (e.TryGetProperty(""first_release_date"", out var d) && d.ValueKind == JsonValueKind.Number) {
        try { return System.DateTimeOffset.FromUnixTimeSeconds(d.GetInt64()).Year; } catch { }
    }
    return 0;
}
List<string> EList(JsonElement e, string p) {
    var list = new List<string>();
    if (e.TryGetProperty(p, out var v) && v.ValueKind == JsonValueKind.Array) {
        foreach (var x in v.EnumerateArray()) {
            var n = EName(x);
            if (n.Length > 0) list.Add(n);
        }
    }
    return list;
}
Api.Log(""Loguję do Twitch..."");
var tok = await Api.PostPage(""https://id.twitch.tv/oauth2/token?client_id="" + Api.UrlEncode(clientId) + ""&client_secret="" + Api.UrlEncode(clientSecret) + ""&grant_type=client_credentials"", """");
var token = Api.Between(tok, Q + ""access_token"" + Q + "":"" + Q, Q);
if (token.Length == 0) { Api.ShowMessage(""Błąd autoryzacji Twitch (sprawdź klucze).""); return; }
Api.SetRequestHeader(""Client-ID"", clientId);
Api.SetRequestHeader(""Authorization"", ""Bearer "" + token);
string q = Api.Title.Replace(Q, """");
Api.Log(""Szukam: "" + Api.Title);
var searchBody = ""search "" + Q + q + Q + ""; fields name,first_release_date,platforms.name; limit 10; where version_parent = null;"";
var searchJson = await Api.PostPage(""https://api.igdb.com/v4/games"", searchBody);
var sroot = Api.JsonRoot(searchJson, out var sdoc);
var picks = new List<(int Id, string Label)>();
if (sroot.ValueKind == JsonValueKind.Array) {
    foreach (var g in sroot.EnumerateArray()) {
        int id = g.TryGetProperty(""id"", out var gid) ? gid.GetInt32() : 0;
        int y = EYear(g);
        if (id > 0) picks.Add((id, EName(g) + (y > 0 ? "" ("" + y + "")"" : """")));
    }
}
sdoc.Dispose();
if (picks.Count == 0) { Api.ShowMessage(""Nie znaleziono gry w IGDB.""); return; }
int pick = 0;
if (picks.Count > 1) {
    pick = Api.PickList(""Wybierz grę (IGDB):"", picks.Select(x => x.Label).ToList());
    if (pick < 0) return;
}
int appid = picks[pick].Id;
Api.Log(""Pobieram szczegóły..."");
var detBody = ""fields name,summary,developers.name,publishers.name,genres.name,platforms.name,first_release_date,rating,cover.image_id,url; where id = "" + appid + "";"";
var detJson = await Api.PostPage(""https://api.igdb.com/v4/games"", detBody);
var droot = Api.JsonRoot(detJson, out var ddoc);
if (droot.ValueKind != JsonValueKind.Array || droot.GetArrayLength() == 0) { ddoc.Dispose(); Api.ShowMessage(""Brak szczegółów.""); return; }
var data = droot.EnumerateArray().First();
if (data.TryGetProperty(""summary"", out var su) && su.ValueKind == JsonValueKind.String) {
    var summary = su.GetString()!;
    if (summary.Length > 0) Api.SetField(""Description"", summary);
}
var devs = EList(data, ""developers"");
if (devs.Count > 0) Api.SetField(""Developer"", string.Join("", "", devs));
var pubs = EList(data, ""publishers"");
if (pubs.Count > 0) Api.SetField(""Publisher"", string.Join("", "", pubs));
var genres = EList(data, ""genres"");
if (genres.Count > 0) Api.SetField(""Genre"", string.Join("", "", genres.Take(3)));
var plats = EList(data, ""platforms"").Select(x => MapPlatformApi(x)).Where(x => x.Length > 0).ToList();
if (plats.Count > 0) Api.SetField(""Platform"", plats[0]);
int year = EYear(data);
if (year > 0) Api.SetField(""ReleaseYear"", year.ToString());
if (data.TryGetProperty(""rating"", out var rt) && rt.ValueKind == JsonValueKind.Number) {
    double r = rt.GetDouble();
    if (r > 0) Api.SetField(""CriticRating"", (r / 10).ToString(""0.0""));
}
if (data.TryGetProperty(""cover"", out var co) && co.TryGetProperty(""image_id"", out var iid)) {
    var img = iid.GetString() ?? """";
    if (img.Length > 0) await Api.GetPicture(""https://images.igdb.com/igdb/image/upload/t_cover_big/"" + img + "".jpg"");
}
if (data.TryGetProperty(""url"", out var u) && u.ValueKind == JsonValueKind.String) Api.SetField(""URL"", u.GetString() ?? """");
ddoc.Dispose();
Api.Log(""Gotowe."");
";
    public const string Wikipedia =
@"// Name: Wikipedia PL
// Description: Pobiera opis i miniaturę z polskiej Wikipedii.
// Version: 1.0
// Author: GameCatalog
// Site: https://pl.wikipedia.org
var title = Api.Title.Trim().Replace(' ', '_');
var url = ""https://pl.wikipedia.org/api/rest_v1/page/summary/"" + Api.UrlEncode(title);
Api.Log(""Pobieram: "" + url);
string json;
try { json = await Api.GetPage(url); }
catch { Api.ShowMessage(""Nie znaleziono artykułu.""); return; }
var root = Api.JsonRoot(json, out var doc);
string S(string p) => root.TryGetProperty(p, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString()! : """";
if (root.TryGetProperty(""type"", out var t) && t.GetString() == ""disambiguation"") { Api.ShowMessage(""Strona ujednoznaczniająca — doprecyzuj tytuł.""); return; }
var desc = S(""extract"");
if (desc.Length > 0) Api.SetField(""Description"", desc);
if (root.TryGetProperty(""content_urls"", out var cu) && cu.TryGetProperty(""desktop"", out var d) && d.TryGetProperty(""page"", out var pg)) Api.SetField(""URL"", pg.GetString() ?? """");
if (root.TryGetProperty(""thumbnail"", out var th) && th.TryGetProperty(""source"", out var src)) await Api.GetPicture(src.GetString() ?? """");
doc.Dispose();
Api.Log(""Gotowe."");
";

    public const string GryOnline =
@"// Name: GRY-Online.pl
// Description: Pobiera dane z GRY-Online.pl (tytuły PL/EN, developer, gatunek, platformy, rok, ocena, opis, okładka).
// Version: 1.0
// Author: GameCatalog
// Site: https://www.gry-online.pl
string MapPlatformGol(string p) {
    p = (p ?? """").ToLowerInvariant();
    if (p.Contains(""pc windows"") || p == ""pc"") return ""PC"";
    if (p.Contains(""playstation 5"")) return ""PS5"";
    if (p.Contains(""playstation 4"")) return ""PS4"";
    if (p.Contains(""playstation 3"")) return ""PS3"";
    if (p.Contains(""playstation vita"")) return ""PS Vita"";
    if (p.Contains(""xbox series"")) return ""Xbox Series"";
    if (p.Contains(""xbox one"")) return ""Xbox One"";
    if (p.Contains(""xbox 360"")) return ""Xbox 360"";
    if (p.Contains(""switch"")) return ""Switch"";
    if (p.Contains(""wii u"")) return ""Wii U"";
    if (p.StartsWith(""wii"")) return ""Wii"";
    if (p.Contains(""3ds"")) return ""3DS"";
    if (p.Contains(""mac"")) return ""Mac"";
    if (p.Contains(""linux"")) return ""Linux"";
    if (p.Contains(""ios"") || p.Contains(""android"")) return ""Mobile"";
    return """";
}
string PathGol(string u) {
    var m = System.Text.RegularExpressions.Regex.Match(u ?? """", ""gry-online\\.pl(/gry/[A-Za-z0-9_\\-]+/z[a-z0-9]+/?)(?:[?#]|$)"");
    return m.Success ? m.Groups[1].Value : """";
}
Api.Log(""Szukam: "" + Api.Title);
var found = new List<(string Url, string Name)>();
foreach (var x in await Api.SearchLinks(""site:gry-online.pl/gry "" + Api.Title, ""gry-online.pl"")) {
    var path = PathGol(x.Url);
    if (path.Length == 0) continue;
    var name = System.Text.RegularExpressions.Regex.Replace(x.Name, ""\\s*-\\s*Wielka Encyklopedia Gier\\s*-\\s*GRY-?Online\\.pl\\s*$"", """");
    var full = ""https://www.gry-online.pl"" + path;
    if (found.Any(y => y.Url == full)) continue;
    found.Add((full, name));
}
if (found.Count == 0) { Api.ShowMessage(""Nie znaleziono gry na GRY-Online.pl.""); return; }
int pick = 0;
if (found.Count > 1) {
    pick = Api.PickList(""Wybierz grę (GRY-Online.pl):"", found.Select(x => x.Name).ToList());
    if (pick < 0) return;
}
string gameUrl = found[pick].Url;
Api.Log(""Pobieram: "" + gameUrl);
string page;
try { page = await Api.GetPage(gameUrl); }
catch { Api.ShowMessage(""Nie udało się pobrać strony gry.""); return; }
string ld = Api.ScriptJson(page, ""VideoGame"");
if (ld.Length == 0) { Api.ShowMessage(""Brak danych gry na stronie.""); return; }
var rootAll = Api.JsonRoot(ld, out var doc);
if (!rootAll.TryGetProperty(""about"", out var root)) { doc.Dispose(); Api.ShowMessage(""Brak danych gry na stronie.""); return; }
string S(JsonElement e, string p) => e.TryGetProperty(p, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString()! : """";
string[] SA(JsonElement e, string p) => e.TryGetProperty(p, out var v) && v.ValueKind == JsonValueKind.Array ? v.EnumerateArray().Select(x => x.ValueKind == JsonValueKind.String ? x.GetString()! : """").Where(x => x.Length > 0).ToArray() : Array.Empty<string>();
string ON(JsonElement e, string p) { if (e.TryGetProperty(p, out var v) && v.ValueKind == JsonValueKind.Object && v.TryGetProperty(""name"", out var n)) return n.GetString() ?? """"; return """"; }
var plTitle = S(root, ""name"");
var enTitle = S(root, ""alternateName"");
if (enTitle.Length > 0) Api.SetField(""Title"", enTitle);
if (plTitle.Length > 0) Api.SetField(""LocalizedTitle"", plTitle);
var dev = ON(root, ""author"");
if (dev.Length > 0) Api.SetField(""Developer"", dev);
var pub = ON(root, ""publisher"");
if (pub.Length > 0) Api.SetField(""Publisher"", pub);
var genres = SA(root, ""genre"");
if (genres.Length > 0) Api.SetField(""Genre"", string.Join("", "", genres.Take(3)));
var plats = SA(root, ""gamePlatform"").Select(x => MapPlatformGol(x)).Where(x => x.Length > 0).ToList();
if (plats.Count > 0) Api.SetField(""Platform"", plats[0]);
var pm = S(root, ""playMode"");
if (pm.EndsWith(""/SinglePlayer"")) Api.SetField(""GameMode"", ""Single-player"");
else if (pm.EndsWith(""/MultiPlayer"")) Api.SetField(""GameMode"", ""Multiplayer"");
else if (pm.EndsWith(""/CoOp"")) Api.SetField(""GameMode"", ""Co-op"");
if (root.TryGetProperty(""aggregateRating"", out var ar) && ar.TryGetProperty(""ratingValue"", out var rv)) {
    double score = rv.ValueKind == JsonValueKind.Number ? rv.GetDouble() : 0;
    if (score > 0) Api.SetField(""CriticRating"", (score * 10).ToString(""0""));
}
var desc = S(root, ""description"");
if (desc.Length > 0) Api.SetField(""Description"", Api.HtmlDecode(desc));
var rel = Api.RegexFirst(page, ""Data wydania:\\s*<b>([^<]+)</b>"");
if (rel.Length == 0) rel = Api.RegexFirst(page, ""Data premiery[^<]*<[^>]+>([^<]+)<"");
if (rel.Length > 0) {
    var my = System.Text.RegularExpressions.Regex.Match(rel, ""(\\d{4})"");
    if (my.Success) Api.SetField(""ReleaseYear"", my.Groups[1].Value);
}
Api.SetField(""URL"", S(root, ""url"").Length > 0 ? S(root, ""url"") : gameUrl);
var img = Api.MetaContent(page, ""og:image"");
if (img.Length > 0) await Api.GetPicture(img);
if (page.Contains(""Polskie napisy"")) Api.SetField(""Subtitles"", ""polskie"");
if (page.Contains(""Polskie dialogi"") || page.Contains(""Polski dubbing"") || page.Contains(""polska wersja językowa"")) Api.SetField(""AudioLanguages"", ""polski"");
doc.Dispose();
Api.Log(""Gotowe: "" + plTitle);
";
}
