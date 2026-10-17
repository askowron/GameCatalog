using System.Reflection;

namespace GameCatalog.Scripting;

/// <summary>
/// Sample scripts installed on first use (like the built-in AMC scripts). The bodies live in
/// <c>Samples/*.gcsx</c>, embedded in this assembly.
/// </summary>
public static class ScriptSamples
{
    private const string ResourcePrefix = "GameCatalog.Scripting.Samples.";
    private const string BuiltInAuthor = "GameCatalog";

    /// <summary>File names of the built-in samples (e.g. <c>Steam.gcsx</c>).</summary>
    public static IReadOnlyList<string> FileNames { get; } = typeof(ScriptSamples).Assembly.GetManifestResourceNames()
        .Where(n => n.StartsWith(ResourcePrefix, StringComparison.Ordinal))
        .Select(n => n[ResourcePrefix.Length..])
        .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
        .ToList();

    public static string Read(string fileName)
    {
        using var stream = typeof(ScriptSamples).Assembly.GetManifestResourceStream(ResourcePrefix + fileName)
                           ?? throw new FileNotFoundException(fileName);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public static void EnsureSamples() => EnsureSamples(GameScript.ScriptsDir());

    /// <summary>
    /// Writes missing samples and upgrades built-in ones that have an older version. The previous
    /// file is kept as <c>&lt;name&gt;.gcsx.v&lt;version&gt;.bak</c>, so local edits are never lost.
    /// Scripts written by someone else, or already up to date, are left alone.
    /// </summary>
    public static void EnsureSamples(string dir)
    {
        foreach (var fileName in FileNames)
        {
            var path = Path.Combine(dir, fileName);
            var shipped = Read(fileName);
            if (!File.Exists(path))
            {
                File.WriteAllText(path, shipped);
                continue;
            }
            var installed = GameScript.Load(path);
            if (!installed.Author.Equals(BuiltInAuthor, StringComparison.OrdinalIgnoreCase)) continue;
            if (ParseVersion(GameScript.Parse(shipped).Version) <= ParseVersion(installed.Version)) continue;
            File.Copy(path, $"{path}.v{installed.Version}.bak", overwrite: true);
            File.WriteAllText(path, shipped);
        }
    }

    private static Version ParseVersion(string text) =>
        Version.TryParse(text.Contains('.') ? text : text + ".0", out var v) ? v : new Version(0, 0);
}
