using GameCatalog.Core.Models;

namespace GameCatalog.Plugins.Abstractions;

public enum ImportSourceKind { File, Folder }

/// <summary>Well-known option keys passed to importers and exporters (values come from the preferences).</summary>
public static class ExchangeOptions
{
    /// <summary>CSV delimiter: ";", "," or "\t".</summary>
    public const string Delimiter = "Delimiter";
    /// <summary>File patterns for folder imports, separated by ';'.</summary>
    public const string FilePatterns = "FilePatterns";
    /// <summary>Document title for exports.</summary>
    public const string Title = "Title";
}

public sealed class ImportContext
{
    public required string Path { get; init; }
    /// <summary>First number to give to imported games.</summary>
    public required int StartNumber { get; init; }
    public IReadOnlyDictionary<string, string> Options { get; init; } = new Dictionary<string, string>();
}

public sealed class ExportContext
{
    public required string Path { get; init; }
    public IReadOnlyDictionary<string, string> Options { get; init; } = new Dictionary<string, string>();
}

/// <summary>An import format shown in File → Import (AMC import2 engines).</summary>
public interface IGameImporter
{
    /// <summary>Caption in the current language.</summary>
    string Name { get; }
    ImportSourceKind SourceKind { get; }
    /// <summary>Open-file dialog filter (ignored for folders).</summary>
    string FileFilter { get; }
    IReadOnlyList<Game> Import(ImportContext context);
}

/// <summary>An export format shown in File → Export (AMC export.pas).</summary>
public interface IGameExporter
{
    string Name { get; }
    string FileFilter { get; }
    void Export(IReadOnlyList<Game> games, ExportContext context);
}
