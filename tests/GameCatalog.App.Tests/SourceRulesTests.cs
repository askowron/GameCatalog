using System.Text.RegularExpressions;

namespace GameCatalog.App.Tests;

/// <summary>
/// Project rules from the spec, checked on the source tree:
/// comments are written in English, and UI text is not hard-coded outside Designer files.
/// Polish diacritics are used as the detector.
/// </summary>
public class SourceRulesTests
{
    private static readonly Regex PolishLetters = new("[ąćęłńóśźżĄĆĘŁŃÓŚŹŻ]", RegexOptions.Compiled);
    /// <summary>Common Polish UI words written without diacritics, which the letter check cannot see.</summary>
    private static readonly Regex PolishWords = new(
        @"\b(Brak|brak|danych|Tak|Nie|Inne|Opcje|Zamknij|Dodaj|Edytuj|Anuluj|Zapisz|Pomoc|Nazwa|Wersja|Katalog|Plik|Pliki|Gra|Gry|gry|Wydawca|Gatunek|Platforma|Okladka|Numer|Wybierz|Szukaj|Kategoria|Wszystkie|Wybrane|Zaznaczone|Widoczne)\b",
        RegexOptions.Compiled);
    private static readonly Regex StringLiteral = new("\"(?:[^\"\\\\]|\\\\.)*\"", RegexOptions.Compiled);

    /// <summary>Files whose Polish literals are catalog data or user-editable content, not UI text.</summary>
    private static readonly string[] DataFiles =
    {
        "GameCatalog.Core/Models/Enums.cs",            // stored list values (stage 7 localizes defaults)
        "GameCatalog.Core/Models/Game.cs",             // default MediaType value
        "GameCatalog.Core/Services/SampleDataFactory.cs",
        "GameCatalog.App/Services/HtmlTemplates.cs",   // user-editable templates (stage 5)
        "GameCatalog.Storage/AmcXmlImporter.cs",       // stored media type values
        "GameCatalog.UI/Components/IconThemeImageList.cs", // legacy theme names read from old prefs.json
        "GameCatalog.Core/Fields/YesNo.cs",            // legacy stored value "Tak"
    };

    private static string SrcRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "GameCatalog.slnx"))) dir = dir.Parent;
        Assert.NotNull(dir);
        return Path.Combine(dir!.FullName, "src");
    }

    private static IEnumerable<(string Relative, string[] Lines)> SourceFiles() =>
        Directory.EnumerateFiles(SrcRoot(), "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .Select(p => (Path.GetRelativePath(SrcRoot(), p).Replace('\\', '/'), File.ReadAllLines(p)));

    [Fact]
    public void Comments_AreInEnglish()
    {
        var offenders = new List<string>();
        foreach (var (file, lines) in SourceFiles())
        {
            for (int i = 0; i < lines.Length; i++)
            {
                int c = lines[i].IndexOf("//", StringComparison.Ordinal);
                if (c < 0) continue;
                if (lines[i][..c].Count(ch => ch == '"') % 2 == 1) continue;   // "//" inside a string literal
                var comment = lines[i][c..];
                if (PolishLetters.IsMatch(comment)) offenders.Add($"{file}:{i + 1}: {comment.Trim()}");
            }
        }
        Assert.True(offenders.Count == 0, "Polish comments:\n" + string.Join("\n", offenders));
    }

    [Fact]
    public void UiText_IsNotHardCoded()
    {
        var offenders = new List<string>();
        foreach (var (file, lines) in SourceFiles())
        {
            if (file.EndsWith(".Designer.cs", StringComparison.Ordinal) || DataFiles.Contains(file)) continue;
            for (int i = 0; i < lines.Length; i++)
            {
                var code = lines[i].Split("//")[0];
                foreach (Match m in StringLiteral.Matches(code))
                    if (PolishLetters.IsMatch(m.Value) || PolishWords.IsMatch(m.Value)) offenders.Add($"{file}:{i + 1}: {m.Value}");
            }
        }
        Assert.True(offenders.Count == 0, "Hard-coded Polish text (move it to Languages/*.json):\n" + string.Join("\n", offenders));
    }
}
