# Contributing to GameCatalog

Thanks for helping! Bug reports, translations, scripts, plugins and code are all welcome.

## Before you start

- GameCatalog is a port of [Ant Movie Catalog](https://www.antp.be/software/moviecatalog/) and aims to look and
  work like it. Before adding a feature, check how AMC does it.
- For bigger changes, open an issue first so we can agree on the approach.

## Building

```
dotnet build GameCatalog.slnx
dotnet test GameCatalog.slnx
dotnet run --project src/GameCatalog.App
```

Requires the .NET 10 SDK on Windows. The build must stay warning-free.

## Rules (checked by tests)

- **Comments and XML docs are in English.** `SourceRulesTests` rejects Polish comments and Polish string
  literals in `src/` (outside `.Designer.cs` files).
- **No UI text in `.cs` files.** Every user-visible text comes from `src/GameCatalog.App/Languages/*.json`.
  `en.json` must have exactly the keys of `pl.json` (`LanguageFilesTests`), so add each key to both files.
- **Designer first.** Every form and control is a `partial class` with a `.Designer.cs` file; layout lives in
  `InitializeComponent`. Control names are translation keys, so renaming a control means renaming its keys.
- **New dialogs** go into the list in `DialogSmokeTests`.
- **Script samples** (`src/GameCatalog.Scripting/Samples/*.gcsx`): bump `// Version:` whenever you change one.
- **New `Game` fields** touch several places that must stay in sync; see *Conventions and gotchas* in
  [CLAUDE.md](CLAUDE.md).

[CLAUDE.md](CLAUDE.md) describes the architecture and conventions in more detail.

## Commits and pull requests

- Commit messages follow [Conventional Commits](https://www.conventionalcommits.org/):
  `feat(scripts): ...`, `fix(app): ...`, `docs: ...`.
- Check UI changes manually in both Polish and English.
- Fill in the pull request checklist.

## Translations

Use *Help → Export translation template* in the app, translate the file and either open a pull request adding
it to `src/GameCatalog.App/Languages` or attach it to a
[translation issue](https://github.com/askowron/GameCatalog/issues/new?template=translation.yml).

## Plugins and scripts

- Plugins: see [samples/GameCatalog.Plugin.Sample/README.md](samples/GameCatalog.Plugin.Sample/README.md).
- Scripts: C# files (`*.gcsx`) in `Documents\GameCatalog\Scripts`; the built-in samples are a good starting point.

## License

By contributing you agree that your work is released under the [GNU GPL v3](LICENSE).
