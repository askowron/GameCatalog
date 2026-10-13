using GameCatalog.Core.Models;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace GameCatalog.Scripting;

/// <summary>Globals skryptu — w kodzie skryptu dostępny jest obiekt Api.</summary>
public sealed class ScriptGlobals
{
    public ScriptApi Api { get; }
    public ScriptGlobals(ScriptApi api) => Api = api;
}

/// <summary>Wynik wykonania skryptu.</summary>
public sealed class ScriptResult
{
    public bool Success { get; set; }
    public string Error { get; set; } = "";
    public List<string> Log { get; set; } = new();
    public Dictionary<string, string> Fields { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> CustomFields { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<GameExtra> Extras { get; set; } = new();
    public byte[]? Cover { get; set; }
}

/// <summary>
/// Uruchamianie skryptów C# (odpowiednik silnika IFPS w AMC getscript.pas).
/// Skrypty pobierają dane o grach z internetu i zwracają pola do zastosowania.
/// </summary>
public static class ScriptRunner
{
    public static TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);

    public static async Task<ScriptResult> RunAsync(
        string code, Game game,
        Func<string, IList<string>, int>? picker = null,
        CancellationToken ct = default)
    {
        var log = new List<string>();
        var api = new ScriptApi(game, log, picker);
        var result = new ScriptResult { Log = log };
        try
        {
            var options = ScriptOptions.Default
                .AddReferences(typeof(Game).Assembly, typeof(ScriptApi).Assembly,
                    typeof(System.Text.Json.JsonDocument).Assembly,
                    typeof(System.Text.RegularExpressions.Regex).Assembly,
                    typeof(System.Net.Http.HttpClient).Assembly)
                .WithImports("System", "System.Collections.Generic", "System.Linq",
                    "System.Text.RegularExpressions", "System.Text.Json", "System.Threading.Tasks");
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(Timeout);
            var script = CSharpScript.Create(code, options, typeof(ScriptGlobals));
            var diagnostics = script.Compile();
            var errors = diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error).ToList();
            if (errors.Count > 0)
            {
                result.Error = "Błędy kompilacji:\n" + string.Join("\n",
                    errors.Select(e => $"linia {e.Location.GetLineSpan().StartLinePosition.Line + 1}: {e.GetMessage()}"));
                return result;
            }
            var state = await script.RunAsync(new ScriptGlobals(api), null, cts.Token).ConfigureAwait(false);
            if (state.Exception is not null)
            {
                result.Error = "Błąd wykonania: " + state.Exception.Message;
                return result;
            }
            result.Success = true;
            result.Fields = api.Fields;
            result.CustomFields = api.CustomFields;
            result.Extras = api.Extras;
            result.Cover = api.Cover;
            if (result.Fields.Count == 0 && result.Extras.Count == 0 && result.Cover is null)
                log.Add("Skrypt nie zwrócił żadnych danych.");
        }
        catch (OperationCanceledException)
        {
            result.Error = "Przekroczono limit czasu (60 s).";
        }
        catch (Exception ex)
        {
            result.Error = "Błąd: " + ex.Message;
        }
        return result;
    }

    public static Task<ScriptResult> RunAsync(GameScript script, Game game,
        Func<string, IList<string>, int>? picker = null, CancellationToken ct = default) =>
        RunAsync(script.Code, game, picker, ct);
}
