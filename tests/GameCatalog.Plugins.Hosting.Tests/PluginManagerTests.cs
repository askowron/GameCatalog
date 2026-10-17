using GameCatalog.Localization;
using GameCatalog.Plugins.Abstractions;
using GameCatalog.Plugins.Abstractions.WinForms;

namespace GameCatalog.Plugins.Hosting.Tests;

public sealed class PluginManagerTests : IDisposable
{
    private readonly string _plugins = Directory.CreateTempSubdirectory("gc-plugins-").FullName;
    private readonly ExtensionRegistry _registry = new();
    private readonly List<string> _log = new();

    public void Dispose()
    {
        try { Directory.Delete(_plugins, recursive: true); }
        catch (UnauthorizedAccessException) { /* assemblies stay locked until the test process exits */ }
    }

    /// <summary>Build output folder of a sibling project, same configuration and TFM as this test run.</summary>
    private static string OutputOf(string relativeProjectDir, string tfm)
    {
        var testOut = new DirectoryInfo(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar));
        string configuration = testOut.Parent!.Name;   // …\bin\<Configuration>\<tfm>
        var root = testOut;
        while (!File.Exists(Path.Combine(root.FullName, "GameCatalog.slnx"))) root = root.Parent!;
        return Path.Combine(root.FullName, relativeProjectDir, "bin", configuration, tfm);
    }

    private void Install(string name, string sourceDir)
    {
        var target = Directory.CreateDirectory(Path.Combine(_plugins, name)).FullName;
        foreach (var file in Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            var dest = Path.Combine(target, Path.GetRelativePath(sourceDir, file));
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            File.Copy(file, dest, overwrite: true);
        }
    }

    private PluginManager CreateManager() =>
        new(_registry, d => new TestHost(d.Id, _registry, new LanguageManager(Path.Combine(_plugins, "none"))), _log.Add);

    [Fact]
    public void SamplePlugin_LoadsAndRegistersEveryExtensionPoint()
    {
        Install("GameCatalog.Plugin.Sample", OutputOf(Path.Combine("samples", "GameCatalog.Plugin.Sample"), "net10.0-windows"));
        var manager = CreateManager();

        manager.LoadFrom(_plugins, Array.Empty<string>());

        var sample = Assert.Single(manager.Plugins);
        Assert.Equal(PluginState.Loaded, sample.State);
        Assert.Equal("sample", sample.Id);
        Assert.Single(_registry.All<IGameImporter>());
        Assert.Single(_registry.All<IGameExporter>());
        Assert.Single(_registry.All<IGameInfoProvider>());
        Assert.Single(_registry.All<ICatalogCommand>());
        Assert.Single(_registry.All<IGamePanelPlugin>());
    }

    [Fact]
    public void DisabledPlugin_IsListedButNotInitialized()
    {
        Install("GameCatalog.Plugin.Sample", OutputOf(Path.Combine("samples", "GameCatalog.Plugin.Sample"), "net10.0-windows"));
        var manager = CreateManager();

        manager.LoadFrom(_plugins, new[] { "sample" });

        Assert.Equal(PluginState.Disabled, Assert.Single(manager.Plugins).State);
        Assert.Empty(_registry.All<IGameImporter>());
    }

    [Fact]
    public void Fixtures_FailingAndIncompatibleAreReported_RegistrationsRolledBack()
    {
        Install("GameCatalog.Plugin.TestFixtures", OutputOf(Path.Combine("tests", "Fixtures", "GameCatalog.Plugin.TestFixtures"), "net10.0"));
        var manager = CreateManager();

        manager.LoadFrom(_plugins, Array.Empty<string>());

        var throws = manager.Plugins.Single(p => p.Id == "fixture.throws");
        var future = manager.Plugins.Single(p => p.Id == "fixture.future");
        Assert.Equal(PluginState.Failed, throws.State);
        Assert.Contains("on purpose", throws.Error);
        Assert.Equal(PluginState.Incompatible, future.State);
        Assert.DoesNotContain(manager.Plugins, p => p.Name.Contains("Unmarked"));
        Assert.Empty(_registry.All<IGameImporter>());
    }

    [Fact]
    public void DuplicateId_SecondCopyFails_FirstKeepsItsExtensions()
    {
        Install("GameCatalog.Plugin.Sample", OutputOf(Path.Combine("samples", "GameCatalog.Plugin.Sample"), "net10.0-windows"));
        var dll = Path.Combine(_plugins, "GameCatalog.Plugin.Sample", "GameCatalog.Plugin.Sample.dll");
        var manager = CreateManager();

        manager.LoadAssembly(dll, Array.Empty<string>());
        manager.LoadAssembly(dll, Array.Empty<string>());

        Assert.Equal(new[] { PluginState.Loaded, PluginState.Failed }, manager.Plugins.Select(p => p.State));
        Assert.Contains("already uses", manager.Plugins[1].Error);
        Assert.Single(_registry.All<IGameImporter>());
    }

    [Fact]
    public void NotAnAssembly_IsReportedAsFailed()
    {
        var dir = Directory.CreateDirectory(Path.Combine(_plugins, "Broken")).FullName;
        File.WriteAllText(Path.Combine(dir, "Broken.dll"), "not a dll");
        var manager = CreateManager();

        manager.LoadFrom(_plugins, Array.Empty<string>());

        var broken = Assert.Single(manager.Plugins);
        Assert.Equal(PluginState.Failed, broken.State);
        Assert.Equal("Broken", broken.Id);
    }

    [Fact]
    public void MissingFolderOrDll_IsIgnored()
    {
        Directory.CreateDirectory(Path.Combine(_plugins, "Empty"));
        var manager = CreateManager();
        manager.LoadFrom(_plugins, Array.Empty<string>());
        manager.LoadFrom(Path.Combine(_plugins, "does-not-exist"), Array.Empty<string>());
        Assert.Empty(manager.Plugins);
    }

    [Fact]
    public void Registry_RemoveAll_DropsOnlyThatPlugin()
    {
        var registry = new ExtensionRegistry();
        registry.Add("a", "x");
        registry.Add("b", "y");
        registry.RemoveAll("a");
        Assert.Equal(new[] { "y" }, registry.All<string>());
    }
}
