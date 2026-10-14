namespace GameCatalog.App.Diagnostics;

/// <summary>Plain-text application log (Documents\GameCatalog\Logs\gamecatalog.log), rotated at 1 MB.</summary>
public static class AppLog
{
    private const long MaxSize = 1024 * 1024;
    private static readonly object Gate = new();

    public static string? FilePath { get; private set; }

    public static void Initialize(string directory)
    {
        Directory.CreateDirectory(directory);
        FilePath = Path.Combine(directory, "gamecatalog.log");
        try
        {
            var info = new FileInfo(FilePath);
            if (info.Exists && info.Length > MaxSize)
                File.Move(FilePath, FilePath + ".old", overwrite: true);
        }
        catch (IOException) { /* rotation is best effort */ }
    }

    public static void Write(string message)
    {
        if (FilePath is null) return;
        lock (Gate)
        {
            try { File.AppendAllText(FilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}"); }
            catch (IOException) { /* logging must never crash the app */ }
            catch (UnauthorizedAccessException) { }
        }
    }

    public static void Error(Exception ex, string context) => Write($"ERROR {context}: {ex}");
}
