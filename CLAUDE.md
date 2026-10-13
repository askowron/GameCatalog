# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

GameCatalog is a Windows Forms desktop app (.NET 6) for cataloguing video games. It is a port of **Ant Movie Catalog (AMC)**, a Delphi movie cataloguer, adapted from movies to games. Many comments reference the original AMC Pascal units (`main.pas`, `fields.pas`, `getscript.pas`, `options.pas`, `export.pas`, `ConstValues.pas`). When you add a feature, check how AMC behaves first; most features are meant to match an AMC counterpart.

The UI text, comments and identifiers in user-facing strings are in **Polish**. Keep new UI strings and code comments in Polish to match.

## Commands

The solution file is `GameCatalog.slnx` (the new XML solution format; it needs a recent SDK or VS 2022 17.13+/VS 18). The projects target `net6.0` / `net6.0-windows`.

```
dotnet build GameCatalog.slnx
dotnet run --project src/GameCatalog.App
```

There are no test projects and no lint configuration.

## Architecture

Four projects, with dependencies pointing toward Core:

- **GameCatalog.Core**: has no dependencies. Contains the domain model (`Models/Game.cs`, `GameExtra`, `LoanRecord`, `CustomFieldDefinition`, `Enums`) and `Catalog/`:
  - `GameCatalogModel`: the in-memory list of games, plus filter, sort (with an optional natural comparer) and group-by. It mirrors AMC's `TMovieList`. The UI works on this model; persistence is separate.
  - `GameFields`: the master field table (Id, Tag, SqlTag, DisplayName, AMC origin). Tags are the canonical field identifiers used throughout: sorting, grouping, `GameFieldValues.Get(game, tag)`, script `SetField/GetField`, and custom fields (an unknown tag falls through to `Game.CustomValues`). SqlTags keep the old AMC names (e.g. `DIRECTOR` → Developer) so old AMC data can still be imported.
- **GameCatalog.Storage** (Microsoft.Data.Sqlite):
  - `SqliteGameCatalogStore`: a catalog is a single SQLite file (`.gc` extension; `.db` is the legacy extension). The schema is created with `CREATE TABLE IF NOT EXISTS` in `Initialize()`, and there is no migration system. `SaveAll` writes the **whole model** in one transaction: it upserts every game and deletes rows whose numbers are gone. `LoadAll` loads everything. There is no incremental persistence.
  - `CatalogTransfer`: CSV/HTML export, CSV import, and import from a folder.
  - `AmcXmlImporter`: imports AMC `.xml` catalogs, mapping movie fields to game fields.
- **GameCatalog.Scripting** (Roslyn `Microsoft.CodeAnalysis.CSharp.Scripting`): replaces AMC's IFPS (Pascal) scripts with C# scripts.
  - Script files are `*.gcsx` in `Documents\GameCatalog\Scripts`. The metadata header is made of `// Name:`, `// Description:`, `// Version:`, `// Author:` and `// Site:` lines (`GameScript.Load`).
  - Scripts run against the `ScriptGlobals.Api` (`ScriptApi`) object: HTTP fetches (a shared static `HttpClient`), HTML/regex/JSON helpers, `SetField(tag, value)`, `AddExtra`, `GetPicture`, and `PickList` (a UI callback).
  - A script does not change the `Game` directly. `ScriptRunner.RunAsync` returns a `ScriptResult` (Fields/CustomFields/Extras/Cover), and the UI (`ScriptResultForm`) lets the user choose which values to apply.
  - `ScriptSamples` writes the built-in sample scripts (Steam, Wikipedia PL, IGDB, GRY-Online) on first use. It does not overwrite existing files, so changing a sample constant does not update installed copies.
- **GameCatalog.App** (WinForms):
  - The UI is **built in code**; there are no `.Designer.cs` files or `.resx` forms.
  - `Forms/MainForm.cs` (~2.4k lines) is the hub. It holds `_model`, `_store`, `_loans` and the `_dirty` flag, plus the menus, toolbar, list view, find bar, cover panel, undo stack and every file action.
  - `Controls/GameFieldsControl` is the field editor. Edits are written back to the `Game` through `CommitAllFields()` before saving.
  - `PrefsStore` holds the preferences, serialized as JSON to `Documents\GameCatalog\prefs.json`. Add new options as properties with defaults. They also need wiring into `PrefsForm` and the code in `MainForm` that applies prefs.
  - `AmcIcons`: the `AmcIcon` enum order must match the icon strip order in `Resources/*.bmp` (original AMC toolbar themes, copied to the output directory). Do not reorder it.
  - `HtmlTemplates`: game detail/preview templates expanded per game.

## Conventions and gotchas

- **Ratings** are stored as ints from 0 to 100, with -1 meaning "none", and displayed as `/10.0` with a `0.0` format. `ReleaseYear`, `DiskCount` and `PlaytimeHours` use 0 to mean "unset".
- **Game identity** is `Game.Number`, which is the SQLite primary key. Extras, loans and custom values reference it by number, so renumbering has to keep these references in sync.
- `GameCatalogModel.ReplaceAll` takes a snapshot of its input before clearing. Call it with LINQ built over `model.Games`.
- **Adding a field to `Game`** means updating several places that must stay in sync:
  - `GameFields.All`
  - `GameFieldValues.Get`
  - The SQLite schema and the upsert/load code in `SqliteGameCatalogStore`. Existing databases won't gain new columns through `CREATE TABLE IF NOT EXISTS`, so add an `ALTER TABLE` path.
  - The code that applies a `ScriptResult.Fields` tag to a `Game` property (`ScriptResultForm` / `MainForm`)
  - `GameFieldsControl`
  - `Sort`/`GroupBy` in `GameCatalogModel` (if the field is sortable or groupable)
- `AssemblyVersion` is set by hand in each `.csproj`, using the `YY.M.D.build` format.
