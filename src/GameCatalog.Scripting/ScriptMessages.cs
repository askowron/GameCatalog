namespace GameCatalog.Scripting;

/// <summary>Texts reported back to the user; replaced with translations by the App (Task 6).</summary>
public static class ScriptMessages
{
    public static Func<string, string> CompileErrors { get; set; } = details => "Compilation errors:\n" + details;
    public static Func<int, string, string> CompileErrorLine { get; set; } = (line, msg) => $"line {line}: {msg}";
    public static Func<string, string> RuntimeError { get; set; } = msg => "Runtime error: " + msg;
    public static Func<int, string> Timeout { get; set; } = seconds => $"Time limit exceeded ({seconds} s).";
    public static Func<string, string> Error { get; set; } = msg => "Error: " + msg;
    public static string NoData { get; set; } = "Script returned no data.";
}
