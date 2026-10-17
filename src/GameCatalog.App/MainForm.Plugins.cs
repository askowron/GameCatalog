using GameCatalog.App.Diagnostics;
using GameCatalog.App.Forms;
using GameCatalog.App.Plugins;
using GameCatalog.App.Services;
using GameCatalog.Core.Models;
using GameCatalog.Plugins.Abstractions;
using GameCatalog.Plugins.Abstractions.WinForms;
using GameCatalog.Plugins.Hosting;
using GameCatalog.Scripting;
using GameCatalog.Storage.Exchange;
using GameCatalog.UI.Components;
using GameCatalog.UI.Icons;

namespace GameCatalog.App;

/// <summary>Plugins: loading, catalog/selection facades and placing extensions in menus, toolbar and tabs.</summary>
public partial class MainForm : IGameCatalog, IGameSelection
{
    private readonly ExtensionRegistry _extensions = new();
    private PluginManager? _plugins;
    private readonly List<(string PluginId, IGamePanelPlugin Panel, TabPage Tab)> _panels = new();
    private readonly List<(string PluginId, ICatalogCommand Command, UIAction Action)> _commands = new();

    /// <summary>Called from OnLoad after the catalog is open.</summary>
    private void LoadExtensions()
    {
        var (importers, exporters) = BuiltInExchange.Create(L);
        foreach (var i in importers) _extensions.Add(ExtensionRegistry.BuiltInId, i);
        foreach (var e in exporters) _extensions.Add(ExtensionRegistry.BuiltInId, e);

        _plugins = new PluginManager(_extensions, CreateHost, AppLog.Write);
        _plugins.LoadFrom(AppPaths.PluginsDirectory, _prefs.DisabledPlugins);

        AddPluginCommands();
        AddPluginPanels();
        NotifyPanels();
        foreach (var failed in _plugins.Plugins.Where(p => p.State == PluginState.Failed))
            SetHint(L.Get("Plugin.LoadFailed", failed.Name, failed.Error ?? ""));
    }

    private IPluginHost CreateHost(PluginDescriptor plugin)
    {
        var languages = Path.Combine(Path.GetDirectoryName(plugin.AssemblyPath)!, "Languages");
        AppServices.Languages.AddPluginLanguages(plugin.Id, languages);
        return CreateHost(plugin.Id);
    }

    private IPluginHost CreateHost(string pluginId) =>
        new AppPluginHost(pluginId, _extensions, AppPaths.PluginDataDirectory(pluginId), AppLog.Write, this, this, L);

    /// <summary>
    /// Runs extension code; failures are logged and shown, never propagated. Built-in formats report with
    /// <paramref name="builtInErrorKey"/>, plugins with their id.
    /// </summary>
    private T? Guard<T>(string pluginId, Func<T> action, T? fallback = default, string builtInErrorKey = "Plugin.Error")
    {
        try
        {
            return action();
        }
        catch (Exception ex)   // isolation boundary for extension code
        {
            AppLog.Error(ex, $"Extension {pluginId}");
            string message = pluginId == ExtensionRegistry.BuiltInId && builtInErrorKey != "Plugin.Error"
                ? L.Get(builtInErrorKey, ex.Message)
                : L.Get("Plugin.Error", pluginId, ex.Message);
            MessageBox.Show(this, message, L.Get("Common.Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return fallback;
        }
    }

    private void Guard(string pluginId, Action action) => Guard(pluginId, () => { action(); return true; });

    // --- Import / export entries from the registry ---

    private IReadOnlyList<ExchangeEntry> ImportEntries() =>
        _extensions.AllWithOwner<IGameImporter>()
            .Select(x => new ExchangeEntry(x.Extension.Name, ImporterIcon(x.Extension), () => RunImporter(x.PluginId, x.Extension)))
            .ToList();

    private IReadOnlyList<ExchangeEntry> ExportEntries() =>
        _extensions.AllWithOwner<IGameExporter>()
            .Select(x => new ExchangeEntry(x.Extension.Name, x.Extension is HtmlGameExporter ? AmcIcon.Html : AmcIcon.FileExport,
                () => RunExporter(x.PluginId, x.Extension)))
            .ToList();

    private static AmcIcon ImporterIcon(IGameImporter importer) => importer switch
    {
        FolderGameImporter => AmcIcon.MovieImportFiles,
        AmcXmlGameImporter => AmcIcon.MovieImport,
        _ => AmcIcon.FileImport,
    };

    private Dictionary<string, string> ExchangeOptionsFromPrefs() => new()
    {
        [ExchangeOptions.Delimiter] = _prefs.CsvDelimiter,
        [ExchangeOptions.FilePatterns] = _prefs.MediaPatterns,
        [ExchangeOptions.Title] = L.Get("Export.HtmlTitle"),
    };

    /// <summary>Game → Get information → From files: the built-in folder importer.</summary>
    private void ImportFromFolder() =>
        RunImporter(ExtensionRegistry.BuiltInId, _extensions.All<IGameImporter>().OfType<FolderGameImporter>().First());

    private void RunImporter(string pluginId, IGameImporter importer)
    {
        string? path = null;
        if (importer.SourceKind == ImportSourceKind.Folder)
        {
            string description = importer is FolderGameImporter ? L.Get("Folder.SelectGames") : importer.Name;
            using var dialog = new FolderBrowserDialog { Description = description, UseDescriptionForTitle = true };
            if (dialog.ShowDialog(this) == DialogResult.OK) path = dialog.SelectedPath;
        }
        else
        {
            using var dialog = new OpenFileDialog { Filter = importer.FileFilter, Title = importer.Name };
            if (dialog.ShowDialog(this) == DialogResult.OK) path = dialog.FileName;
        }
        if (path is null) return;
        var context = new ImportContext { Path = path, StartNumber = _session.Games.NextNumber(), Options = ExchangeOptionsFromPrefs() };
        var games = Guard(pluginId, () => importer.Import(context).ToList(), builtInErrorKey: "Msg.ImportError");
        if (games is not null) AddImported(games);
    }

    private void RunExporter(string pluginId, IGameExporter exporter)
    {
        string fileName = exporter switch
        {
            CsvGameExporter => L.Get("File.DefaultCsv"),
            HtmlGameExporter => L.Get("File.DefaultHtml"),
            _ => "",
        };
        using var dialog = new SaveFileDialog { Filter = exporter.FileFilter, Title = exporter.Name, FileName = fileName };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        var context = new ExportContext { Path = dialog.FileName, Options = ExchangeOptionsFromPrefs() };
        var games = _presenter.CurrentView();
        if (Guard(pluginId, () => { exporter.Export(games, context); return true; }, builtInErrorKey: "Msg.ExportError"))
            SetHint(L.Get("Status.Exported", dialog.FileName));
    }

    // --- Info providers next to scripts in Game → Get information ---

    private IEnumerable<ToolStripItem> InfoProviderItems()
    {
        var providers = _extensions.AllWithOwner<IGameInfoProvider>();
        if (providers.Count == 0) yield break;
        yield return new ToolStripSeparator();
        foreach (var (pluginId, provider) in providers)
            yield return MenuItem(Guard(pluginId, () => provider.Name, pluginId)!, false,
                () => _ = RunProviderAsync(pluginId, provider), AmcIcon.MovieImportScript);
    }

    /// <summary>Runs a provider for the selected game; never throws (callers fire and forget).</summary>
    private async Task RunProviderAsync(string pluginId, IGameInfoProvider provider)
    {
        try
        {
            if (_current is not { } game) return;
            CommitEditors();
            var ui = new DialogInfoProviderUi(PickFromList, SetHint);
            UseWaitCursor = true;
            GameInfoResult result;
            try
            {
                using var timeout = new CancellationTokenSource(ScriptRunner.Timeout);
                result = await provider.FetchAsync(game.Clone(), ui, timeout.Token);
            }
            catch (Exception ex)   // isolation boundary for third-party code
            {
                AppLog.Error(ex, $"Plugin {pluginId}");
                result = GameInfoResult.Failed(ex.Message);
            }
            finally { UseWaitCursor = false; }
            if (IsDisposed) return;
            ApplyInfoResult(game, result, Guard(pluginId, () => provider.Name, pluginId)!);
        }
        catch (Exception ex)
        {
            ErrorReporter.Show(ex, this);
        }
    }

    // --- Commands ---

    private void AddPluginCommands()
    {
        foreach (var (pluginId, command) in _extensions.AllWithOwner<ICatalogCommand>())
        {
            var host = CreateHost(pluginId);
            var action = new UIAction(components)
            {
                ShortcutKeys = PluginShortcut.TryParse(Guard(pluginId, () => command.Shortcut, null), out var keys) ? keys : Keys.None,
            };
            _commands.Add((pluginId, command, action));
            ApplyCommandTexts(pluginId, command, action);
            action.Execute += (_, _) => Guard(pluginId, () => command.Execute(host));
            // Update runs on idle: a failing CanExecute only disables the command (no message box loop).
            action.Update += (_, _) => action.Enabled = TryCanExecute(pluginId, command, host);
            Image? icon = Guard(pluginId, () => command.Icon, null) is { Length: > 0 } bytes ? CoverImages.TryLoad(bytes) : null;

            var (root, submenus) = PluginMenuPath.Parse(Guard(pluginId, () => command.MenuPath, ""));
            var menu = submenus.Aggregate(RootMenu(root), FindOrAddSubmenu);
            var item = new ToolStripMenuItem { Image = icon };
            menu.DropDownItems.Add(item);
            actionList.SetAction(item, action);
            if (icon is not null)
            {
                var button = new ToolStripButton { DisplayStyle = ToolStripItemDisplayStyle.Image, Image = icon };
                tsPlugins.Items.Add(button);
                actionList.SetAction(button, action);
                if (!tsPlugins.Visible)
                {
                    // Hidden, it waits on its own row (on the main row it would push tsMain to the right);
                    // shown, it joins the main toolbar's row.
                    tsPlugins.Visible = true;
                    tsPlugins.Location = new Point(tsMain.Right, tsMain.Top);
                }
            }
        }
    }

    /// <summary>Command captions come from the plugin's translations; read again after a language change.</summary>
    private void ApplyCommandTexts(string pluginId, ICatalogCommand command, UIAction action)
    {
        action.Text = Guard(pluginId, () => command.Text, command.Id)!;
        action.ToolTipText = Guard(pluginId, () => command.Hint, null) ?? "";
    }

    /// <summary>Re-reads every plugin caption (commands, panel tabs) in the current language.</summary>
    private void RefreshPluginCaptions()
    {
        foreach (var (pluginId, command, action) in _commands) ApplyCommandTexts(pluginId, command, action);
        foreach (var (pluginId, panel, tab) in _panels) tab.Text = Guard(pluginId, () => panel.Title, pluginId)!;
    }

    private static bool TryCanExecute(string pluginId, ICatalogCommand command, IPluginHost host)
    {
        try
        {
            return command.CanExecute(host);
        }
        catch (Exception ex)   // isolation boundary for third-party code
        {
            AppLog.Error(ex, $"Plugin {pluginId} CanExecute");
            return false;
        }
    }

    private ToolStripMenuItem RootMenu(string root) => root switch
    {
        "File" => mnuFile,
        "Game" => mnuGame,
        "View" => mnuView,
        "Help" => mnuHelp,
        _ => mnuTools,
    };

    private static ToolStripMenuItem FindOrAddSubmenu(ToolStripMenuItem parent, string text)
    {
        var existing = parent.DropDownItems.OfType<ToolStripMenuItem>().FirstOrDefault(i => i.Text == text);
        if (existing is not null) return existing;
        var created = new ToolStripMenuItem(text);
        parent.DropDownItems.Add(created);
        return created;
    }

    // --- Panels ---

    private void AddPluginPanels()
    {
        foreach (var (pluginId, panel) in _extensions.AllWithOwner<IGamePanelPlugin>())
        {
            var host = CreateHost(pluginId);
            var control = Guard(pluginId, () => panel.CreatePanel(host));
            if (control is null) continue;
            control.Dock = DockStyle.Fill;
            var tab = new TabPage(Guard(pluginId, () => panel.Title, pluginId)) { Tag = pluginId };
            tab.Controls.Add(control);
            tabsGame.TabPages.Add(tab);
            _panels.Add((pluginId, panel, tab));
        }
    }

    private void NotifyPanels()
    {
        foreach (var (pluginId, panel, _) in _panels) Guard(pluginId, () => panel.OnCurrentGameChanged(_current));
        CurrentChanged?.Invoke(this, EventArgs.Empty);
    }

    // --- Plugins window ---

    private void actPlugins_Execute(object? sender, EventArgs e)
    {
        if (_plugins is null) return;
        using var form = new PluginsForm(_plugins.Plugins, _prefs.DisabledPlugins);
        if (form.ShowDialog(this) != DialogResult.OK) return;
        _prefs.DisabledPlugins = form.DisabledIds.ToList();
        _prefs.Save();
    }

    // --- IGameCatalog (facade for plugins) ---

    string IGameCatalog.FilePath => _session.FilePath;
    IReadOnlyList<Game> IGameCatalog.Games => _session.Games.Games;
    IReadOnlyList<CustomFieldDefinition> IGameCatalog.CustomFieldDefinitions => _session.Games.CustomFieldDefs;
    Game? IGameCatalog.Find(int number) => _session.Games.Find(number);
    int IGameCatalog.NextNumber() => _session.Games.NextNumber();

    void IGameCatalog.Add(Game game)
    {
        _session.Games.Add(game);   // assigns a free number when Number is not positive
        _session.Undo.Push(() =>
        {
            _session.Games.Remove(game);
            _session.MarkDirty();
        });
        _session.MarkDirty();
    }

    void IGameCatalog.Remove(Game game)
    {
        if (!_session.Games.Remove(game)) return;
        _session.Undo.Push(() =>
        {
            _session.Games.Add(game);
            _session.MarkDirty();
        });
        _session.MarkDirty();
    }

    void IGameCatalog.NotifyChanged(Game? game)
    {
        _session.MarkDirty();   // raises Changed: the list refreshes
        RebindCurrent();
    }

    // --- IGameSelection ---

    Game? IGameSelection.Current => _current;
    IReadOnlyList<Game> IGameSelection.Visible => _presenter.CurrentView();
    IReadOnlyList<Game> IGameSelection.Checked => _session.Games.Games.Where(g => g.Checked).ToList();
    void IGameSelection.Select(int number) => SelectGame(number);
    public event EventHandler? CurrentChanged;
}
