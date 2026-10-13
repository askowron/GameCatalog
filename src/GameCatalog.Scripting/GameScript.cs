namespace GameCatalog.Scripting;

/// <summary>
/// Plik skryptu (.gcsx) — odpowiednik skryptów .ifs w AMC.
/// Nagłówek z metadanymi jak GetScriptTitle/Version/Authors w getscript.pas:
/// // Name: ..., // Description: ..., // Version: ..., // Author: ..., // Site: ...
/// </summary>
public sealed class GameScript
{
    public string FilePath { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Version { get; set; } = "";
    public string Author { get; set; } = "";
    public string Site { get; set; } = "";
    public string Code { get; set; } = "";

    public override string ToString() => string.IsNullOrWhiteSpace(Version) ? Name : $"{Name} ({Version})";

    public static string ScriptsDir()
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "GameCatalog", "Scripts");
        Directory.CreateDirectory(dir);
        return dir;
    }

    public static GameScript Load(string path)
    {
        var code = File.ReadAllText(path);
        var s = new GameScript { FilePath = path, Code = code, Name = Path.GetFileNameWithoutExtension(path) };
        foreach (var line in code.Split('\n').Take(30))
        {
            var t = line.Trim().TrimStart('/').Trim();
            int colon = t.IndexOf(':');
            if (colon < 0) continue;
            string key = t[..colon].Trim().ToLowerInvariant();
            string val = t[(colon + 1)..].Trim();
            switch (key)
            {
                case "name": s.Name = val; break;
                case "description": s.Description = val; break;
                case "version": s.Version = val; break;
                case "author": s.Author = val; break;
                case "site": s.Site = val; break;
            }
        }
        return s;
    }

    public void Save() => File.WriteAllText(FilePath, Code);

    public static List<GameScript> LoadAll()
    {
        ScriptSamples.EnsureSamples();
        return Directory.GetFiles(ScriptsDir(), "*.gcsx")
            .OrderBy(p => p)
            .Select(p => { try { return Load(p); } catch { return null; } })
            .Where(s => s is not null)
            .Cast<GameScript>()
            .ToList();
    }
}
