using GameCatalog.Core.Models;
using GameCatalog.Plugins.Abstractions;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace GameCatalog.Scripting;

/// <summary>Script globals — the <c>Api</c> object is available in script code.</summary>
public sealed class ScriptGlobals
{
    public ScriptApi Api { get; }
    public ScriptGlobals(ScriptApi api) => Api = api;
}

/// <summary>
/// Runs C# scripts (counterpart of the IFPS engine in AMC getscript.pas).
/// Scripts fetch game data from the internet and return fields to apply.
/// </summary>
public static class ScriptRunner
{
    public static TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);

    public static async Task<GameInfoResult> RunAsync(
        string code, Game game,
        Func<string, IList<string>, int>? picker = null,
        CancellationToken ct = default)
    {
        var log = new List<string>();
        var api = new ScriptApi(game, log, picker);
        var result = new GameInfoResult { Log = log };
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
                result.Error = ScriptMessages.CompileErrors(string.Join("\n",
                    errors.Select(e => ScriptMessages.CompileErrorLine(
                        e.Location.GetLineSpan().StartLinePosition.Line + 1, e.GetMessage()))));
                return result;
            }
            var state = await script.RunAsync(new ScriptGlobals(api), null, cts.Token).ConfigureAwait(false);
            if (state.Exception is not null)
            {
                result.Error = ScriptMessages.RuntimeError(state.Exception.Message);
                return result;
            }
            result.Success = true;
            result.Fields = api.Fields;
            result.CustomFields = api.CustomFields;
            result.Extras = api.Extras;
            result.Cover = api.Cover;
            if (!result.HasData) log.Add(ScriptMessages.NoData);
        }
        catch (OperationCanceledException)
        {
            result.Error = ScriptMessages.Timeout((int)Timeout.TotalSeconds);
        }
        catch (Exception ex)
        {
            result.Error = ScriptMessages.Error(ex.Message);
        }
        return result;
    }

    public static Task<GameInfoResult> RunAsync(GameScript script, Game game,
        Func<string, IList<string>, int>? picker = null, CancellationToken ct = default) =>
        RunAsync(script.Code, game, picker, ct);
}
