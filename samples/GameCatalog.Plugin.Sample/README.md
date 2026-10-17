# GameCatalog sample plugin

This project shows every extension point a GameCatalog plugin can use:

| Extension | Class | Where it appears |
|---|---|---|
| `IGameImporter` | `JsonGameImporter` | File → Import |
| `IGameExporter` | `JsonGameExporter` | File → Export |
| `IGameInfoProvider` | `SampleInfoProvider` | Game → Get information |
| `ICatalogCommand` | `MarkGamesWithoutCoverCommand` | Tools menu |
| `IGamePanelPlugin` | `GameSummaryPanel` | a tab in the game info panel |

## Building a plugin

1. **Target framework.** Use `net10.0`, or `net10.0-windows` with `<UseWindowsForms>true</UseWindowsForms>`
   when the plugin has a user interface (`IGamePanelPlugin`).
2. **`<EnableDynamicLoading>true</EnableDynamicLoading>`.** The build then writes a `.deps.json`,
   which lets the application resolve the plugin's own NuGet dependencies from its folder.
3. **Reference the contracts without copying them.** The application provides
   `GameCatalog.Core`, `GameCatalog.Localization`, `GameCatalog.Plugins.Abstractions` and
   `GameCatalog.Plugins.Abstractions.WinForms` at run time, so the plugin must use the same copies:

   ```xml
   <ProjectReference Include="...\GameCatalog.Plugins.Abstractions.csproj" Private="false" ExcludeAssets="runtime" />
   ```

   (or a `PackageReference` with `ExcludeAssets="runtime"`). Copies of these assemblies in the plugin
   folder are ignored.
4. **Folder layout.** Every plugin lives in its own folder whose name matches the main assembly:

   ```
   GameCatalog.exe
   Plugins\
     GameCatalog.Plugin.Sample\
       GameCatalog.Plugin.Sample.dll
       GameCatalog.Plugin.Sample.deps.json
       Languages\pl.json
       Languages\en.json
   ```

   Debug builds of this project copy themselves there automatically (the `DeployToApp` target).
5. **Entry class.** Mark a public class that implements `IPlugin` and has a parameterless constructor:

   ```csharp
   [GameCatalogPlugin("sample", "Sample plugin", "1.0", Author = "...", Description = "...")]
   public sealed class SamplePlugin : IPlugin
   {
       public void Initialize(IPluginHost host) { host.Register(...); }
   }
   ```

   The id must be unique and stable: it identifies the plugin in the preferences (enabled/disabled)
   and prefixes its translation keys. A plugin that throws from `Initialize` is reported as failed and
   everything it registered is removed; the application keeps running.
6. **Translations.** Put `Languages\<code>.json` files next to the dll, with a `language` header and a
   `strings` section. Each key `K` is available to the plugin as `Plugin.<Id>.K`, for example
   `host.Translator.Get("Plugin.sample.ImportJson")`. Languages the application does not have are ignored.
7. **API version.** `GameCatalogPluginAttribute.ApiVersion` defaults to the `PluginApi.Version` the
   plugin was compiled against. The application loads only plugins whose version equals its own and
   lists the others as incompatible, so rebuild the plugin against the new contracts after an API change.
