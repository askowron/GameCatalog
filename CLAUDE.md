# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

GameCatalog is a Windows Forms desktop app (.NET 10) for cataloguing video games. It is a port of **Ant Movie Catalog (AMC)**, a Delphi movie cataloguer, adapted from movies to games, and it aims to mirror AMC visually and functionally. Comments often reference the original AMC Pascal units (`main.pas`, `fields.pas`, `getscript.pas`, `options.pas`, `languageselect.pas`, …). When you add a feature, check how AMC behaves first; most features are meant to match an AMC counterpart.

**Language rules** (guarded by tests, see below):
- Code comments and XML docs are in **English**.
- User-visible text comes from `src/GameCatalog.App/Languages/*.json` (`pl` is the default language, `en` is the second one). Designer files hold the Polish text only as a fallback. Never hard-code UI text in `.cs` files.

## Commands

```
dotnet build GameCatalog.slnx
dotnet test GameCatalog.slnx
dotnet test GameCatalog.slnx --filter "FullyQualifiedName~DialogSmokeTests"
dotnet run --project src/GameCatalog.App
```

`GameCatalog.slnx` is the XML solution format (needs a recent SDK / VS 2022 17.13+). Projects target `net10.0` (no UI) or `net10.0-windows` (WinForms). Shared MSBuild settings (Nullable, ImplicitUsings, LangVersion, `Version` as `Major.Minor`, author, copyright) live only in `Directory.Build.props`; `.csproj` files must not repeat them. `Directory.Build.targets` drops the WebView2 WPF assembly so the build stays warning-free.

## Architecture

Dependencies (A → B: A depends on B):

```
Localization                  → (none)
Core                          → (none)
Plugins.Abstractions          → Core, Localization
Plugins.Abstractions.WinForms → Plugins.Abstractions
Plugins.Hosting               → Plugins.Abstractions
Storage                       → Core, Plugins.Abstractions
Scripting                     → Core, Plugins.Abstractions
UI                            → Core, Localization
App                           → all of the above
```

- **GameCatalog.Core**: domain model (`Models/`: `Game`, `GameExtra`, `LoanRecord`, `CustomFieldDefinition`, `Enums`), `Catalog/GameList` (in-memory list with filter/sort/group, mirrors AMC `TMovieList`; `NaturalStringComparer`), and `Fields/`:
  - `GameFields`: the master field table (Tag, SqlTag, label key, AMC origin). Tags are the canonical field identifiers used everywhere: sorting, grouping, `GameFieldValues.Get(game, tag)`, `GameFieldWriter.Set(game, tag, value)`, script `SetField/GetField`, custom fields (an unknown tag falls through to `Game.CustomValues`). SqlTags keep the old AMC names (e.g. `DIRECTOR` → Developer) so AMC data still imports.
  - Core knows no language: labels are **keys** (`Field.Developer`, `Status.Backlog`) resolved through `CatalogLabels.Current` (`ICatalogLabels`), which the App points at the active language (`TranslatorCatalogLabels`).
- **GameCatalog.Localization**: `LanguageManager` (`ITranslator`) loads `Languages/*.json` (`language` header, `forms` section per form/control, `strings` section), falls back to the default language and then to the key, never throws on broken files (logs instead), switches language live (`LanguageChanged`), merges plugin translations as `Plugin.<Id>.<Key>`, and exports a translation template.
- **GameCatalog.Storage** (Microsoft.Data.Sqlite):
  - `SqliteGameCatalogStore`: a catalog is one SQLite file (`.gc`; `.db` is the legacy extension). Schema created with `CREATE TABLE IF NOT EXISTS` in `Initialize()`; there is no migration system. `SaveAll` writes the whole model in one transaction and refuses duplicate game numbers; `LoadAll` loads everything.
  - `CatalogTransfer` (CSV/HTML/folder) and `AmcXmlImporter`; `Exchange/` wraps them as `IGameImporter`/`IGameExporter` (`BuiltInExchange.Create`), the same contracts plugins use.
- **GameCatalog.Scripting** (Roslyn): C# scripts (`*.gcsx` in `Documents\GameCatalog\Scripts`) replace AMC's Pascal scripts. Header lines `// Name:`, `// Description:`, `// Version:`, `// Author:`, `// Site:`. Scripts run against `ScriptApi`; `ScriptRunner.RunAsync` returns a `GameInfoResult` and the user picks what to apply in `ScriptResultForm`. The built-in samples (Steam, GRY-Online.pl, IGDB, Wikipedia) are `src/GameCatalog.Scripting/Samples/*.gcsx`, embedded as resources (`WithCulture="false"`, or `.pl` in a name is taken for a culture). `ScriptSamples.EnsureSamples` writes missing ones and upgrades an installed sample with `Author: GameCatalog` when the shipped `Version` is higher, keeping the old file as `<name>.gcsx.v<version>.bak` — **bump `// Version:` whenever you change a sample**. `ScriptSamplesTests` compiles every sample (`ScriptRunner.CompileErrors`); check changes to the Steam/GRY-Online scrapers against the live sites.
- **GameCatalog.Plugins.\***: plugin contracts (`IPlugin`, `IPluginHost`, `IGameCatalog`, `IGameSelection`, `IGameImporter`/`IGameExporter`, `IGameInfoProvider`, `ICatalogCommand`; WinForms: `IGamePanelPlugin`, `IWinFormsPluginHost`) versioned by `PluginApi.Version`. `PluginManager` loads `Plugins\<Name>\<Name>.dll` into its own `AssemblyLoadContext` (contract assemblies are always shared with the app), checks `[GameCatalogPlugin(ApiVersion)]`, and reports disabled/incompatible/failed/duplicate plugins without stopping the others; a failing plugin's registrations are rolled back. `ExtensionRegistry` holds every extension with its owner (`builtin` for the app's own). `samples/GameCatalog.Plugin.Sample` demonstrates every extension point and its `README.md` explains how to build a plugin (Debug builds deploy it next to the app).
- **GameCatalog.UI** (control library, Designer-friendly):
  - `Components/`: `UIAction` + `ActionList` (AMC `TActionList`: one action drives menu items, toolbar buttons and shortcuts; `Update` runs on idle), `Localizer` (translates its container from the `forms` section on handle creation and on language change), `IconThemeImageList` (AMC toolbar icon themes from `Resources/*.bmp`; the `AmcIcon` enum order must match the strip order — never reorder it).
  - `Controls/`: `GameListView`, `ThumbnailView`, `HtmlPreview` (WebView2 with a fallback message), `PictureToolWindow`, `FindBar`, `StatsCanvas`.
  - `Frames/`: `GameFrame`, `GameCustomFrame`, `GameExtrasFrame`, `GameExtraFrame`, `GameAllFieldsFrame` (AMC frame*.pas); they `Bind(game)` and `Commit()` edits back.
- **GameCatalog.App**: `Program` (logging, global error handlers, start language = `PrefsStore.Language` or, when empty, the system language via `StartupLanguage`), `MainForm` split into partial files by menu (`MainForm.File/Game/View/Tools/Plugins.cs`), dialogs in `Forms/`, and `Services/`: `CatalogSession` (open/save/dirty/undo, commit-after-success), `UndoService`, `NumberingService`, `GameListPresenter`, `GameClipboard`, `CoverImages`, `PrefsStore` (JSON at `Documents\GameCatalog\prefs.json`), `AppPaths`, `HtmlTemplates`. Logs go to `Documents\GameCatalog\Logs`.

## Designer conventions

- Every Form/UserControl is a `partial class` with `X.Designer.cs`; layout lives in `InitializeComponent`, never in constructors. The constructor only calls `InitializeComponent()`, binds data and wires logic. Rows generated from data are the only controls created in code.
- Every form and frame has a `components` container and a `Localizer` (`this.localizer = new Localizer(this.components); this.localizer.ContainerControl = this;`).
- Control names are translation keys (`<control>.Text`, `.HeaderText`, `.ToolTip`, `.ToolTipText`; `$this.Text` is the caption) in the form's section of `pl.json`/`en.json`. Prefixes: `lbl`, `txt`, `cbx`, `chk`, `rb`, `num`, `dtp`, `btn`, `lv`/`lst`/`clb`, `col`, `grd`, `tlp`, `pnl`, `grp`, `split`, `ts`, `mnu`, `tab`, `act`. Never rename a control without renaming its keys.
- Texts computed at runtime (captions with values, labels showing data) get **no** `forms` key — the Localizer would overwrite them; read them from `strings` instead (`AppServices.Languages.Get("<Form>.<Purpose>", args)`).
- Main window commands are `UIAction`s bound to items through `actionList.SetAction`; their texts come from `Action.<Key>` / `Action.<Key>.Hint` strings.

## Tests

xUnit v2 projects in `tests/`: `Core`, `Storage`, `Localization`, `Plugins.Hosting` (installs the sample plugin and `tests/Fixtures/GameCatalog.Plugin.TestFixtures` into a temp folder), `Scripting`, `UI`, `App`. UI and App tests run sequentially (static `Localizer.Translator` / `AppServices`). Rule tests to keep green:
- `LanguageFilesTests`: `en.json` has exactly the keys of `pl.json` — add every key to both files in the same commit.
- `SourceRulesTests`: no Polish comments anywhere in `src/`, and no Polish string literals (diacritics or a list of common words without them) outside `.Designer.cs` files and the listed data files.
- Texts set in code must follow a language switch: `MainForm.OnLanguageChanged` re-reads everything built at runtime (plugin captions, recent-files menu, find bar fields). Built-in HTML templates use `{L:Key}` caption placeholders.
- `DialogSmokeTests`: every dialog constructs and translates in both languages — add new dialogs to its list.

UI is verified manually (both languages).

## Conventions and gotchas

- **Ratings** are ints 0–100, -1 = none, shown as `/10.0` with a `0.0` format. `ReleaseYear`, `DiskCount`, `PlaytimeHours` use 0 for "unset".
- **Game identity** is `Game.Number` (the SQLite primary key). Extras, loans and custom values reference it by number; renumbering goes through `NumberingService` so references stay in sync.
- `GameList.ReplaceAll` snapshots its input before clearing; LINQ over `games.Games` is fine.
- Plugin and extension code runs behind isolation boundaries (`Guard` in `MainForm.Plugins.cs`): catch, log, show — never let it crash the app.
- The program version is `Version` in `Directory.Build.props` (`Major.Minor`, currently 1.0); never add `AssemblyVersion` to a `.csproj`. A release is tagged `v<Version>` on GitHub (https://github.com/askowron/GameCatalog). License: GPL-3.0 (`LICENSE`).
- **Adding a field to `Game`** touches several places that must stay in sync:
  - `GameFields.All`, `GameFieldValues.Get`, `GameFieldWriter.Set`
  - the SQLite schema and the upsert/load code in `SqliteGameCatalogStore` — existing databases do not gain columns through `CREATE TABLE IF NOT EXISTS`, so add an `ALTER TABLE` path (none exists yet)
  - `GameFrame` (Designer editor + the `_editors` map) and `Field.<Tag>` in `pl.json`/`en.json`
  - `GameList` sort/group if the field is sortable or groupable

## Roadmap

The port is staged; each stage has a spec in `docs/superpowers/specs/` and a plan in `docs/superpowers/plans/`:

1. Foundation (done): .NET 10 projects, localization, Designer UI, plugins
2. Fields, extras (DLC), pictures
3. Search, expression filter, advanced sorting, group formats
4. Import (the `import2` wizard)
5. Export (HTML templates, CSV, XML, SQL, pictures)
6. Scripts (properties, debugger, picktree, extras results)
7. Program options and default values
8. Printing and reports (FreeReport counterpart)
9. Loans, statistics, numbering, catalog properties

Out of scope for the whole port: AMCExchange, AMCReport.
