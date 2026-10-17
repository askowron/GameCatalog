namespace GameCatalog.Scripting;

/// <summary>
/// Script file (.gcsx) — counterpart of the .ifs scripts in AMC.
/// Metadata header like GetScriptTitle/Version/Authors in getscript.pas:
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
        var s = Parse(File.ReadAllText(path));
        s.FilePath = path;
        if (s.Name.Length == 0) s.Name = Path.GetFileNameWithoutExtension(path);
        return s;
    }

    /// <summary>Reads the header metadata of script code.</summary>
    public static GameScript Parse(string code)
    {
        var s = new GameScript { Code = code };
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
