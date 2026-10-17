using GameCatalog.Core.Models;
using GameCatalog.Localization;
using GameCatalog.Plugins.Abstractions;
using GameCatalog.Storage.Exchange;

namespace GameCatalog.Storage.Tests;

public sealed class BuiltInExchangeTests : IDisposable
{
    private readonly string _dir = Directory.CreateTempSubdirectory("gc-exchange-").FullName;
    public void Dispose() => Directory.Delete(_dir, recursive: true);

    private static (IReadOnlyList<IGameImporter> Importers, IReadOnlyList<IGameExporter> Exporters) Create() =>
        BuiltInExchange.Create(new LanguageManager(Path.Combine(Path.GetTempPath(), "no-languages")));

    [Fact]
    public void Create_ProvidesFolderCsvAmcImportersAndCsvHtmlExporters()
    {
        var (importers, exporters) = Create();
        Assert.Equal(3, importers.Count);
        Assert.Equal(2, exporters.Count);
        Assert.Contains(importers, i => i.SourceKind == ImportSourceKind.Folder);
        Assert.All(importers, i => Assert.False(string.IsNullOrWhiteSpace(i.Name)));
    }

    [Fact]
    public void CsvExporterAndImporter_RoundTripThroughContracts()
    {
        var (importers, exporters) = Create();
        var csvOut = exporters.Single(e => e is CsvGameExporter);
        var csvIn = importers.Single(i => i is CsvGameImporter);
        var path = Path.Combine(_dir, "a.csv");
        var options = new Dictionary<string, string> { [ExchangeOptions.Delimiter] = "," };

        csvOut.Export(new[] { new Game { Number = 1, Title = "Doom, the game", Platform = "PC" } }, new ExportContext { Path = path, Options = options });
        var games = csvIn.Import(new ImportContext { Path = path, StartNumber = 5, Options = options });

        Assert.Equal("Doom, the game", Assert.Single(games).Title);
        Assert.Equal(5, games[0].Number);
    }

    [Fact]
    public void FolderImporter_UsesPatternsOption()
    {
        var gameDir = Directory.CreateDirectory(Path.Combine(_dir, "Quake")).FullName;
        File.WriteAllText(Path.Combine(gameDir, "quake.exe"), "");
        var folder = Create().Importers.Single(i => i.SourceKind == ImportSourceKind.Folder);

        var games = folder.Import(new ImportContext
        {
            Path = _dir, StartNumber = 1,
            Options = new Dictionary<string, string> { [ExchangeOptions.FilePatterns] = "*.exe" },
        });

        Assert.Equal("Quake", Assert.Single(games).Title);
        Assert.EndsWith("quake.exe", games[0].InstallPath);
    }

    [Fact]
    public void HtmlExporter_WritesTranslatedHeaders()
    {
        var html = Create().Exporters.Single(e => e is HtmlGameExporter);
        var path = Path.Combine(_dir, "a.html");

        html.Export(new[] { new Game { Number = 1, Title = "Doom" } }, new ExportContext { Path = path });

        var text = File.ReadAllText(path);
        Assert.Contains("<th>Field.Title</th>", text);   // no language files loaded: captions fall back to their keys
        Assert.Contains("Doom", text);
    }
}
