# Stage 1 — Foundation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Restructure the GameCatalog port (AMC → games) into a layered, Designer-based, localizable and plugin-ready .NET 10 solution without losing any existing behaviour.

**Architecture:** Incremental refactor (approach A from the spec). Non-UI layers (Core, Localization, Storage, Scripting, Plugins.*) are extracted and covered by xUnit tests first; then reusable WinForms components (`ActionList`/`UIAction`, `Localizer`, `IconThemeImageList`) and frames are built in a `GameCatalog.UI` control library; finally `MainForm` and all dialogs are rebuilt in the VS Designer on top of App services, and the plugin host is wired in. After every task the solution builds, tests pass and the app starts.

**Tech Stack:** .NET 10 (`net10.0`, `net10.0-windows`), WinForms + VS Designer, Microsoft.Data.Sqlite, Microsoft.CodeAnalysis.CSharp.Scripting (Roslyn), Microsoft.Web.WebView2, System.Text.Json, `AssemblyLoadContext`, xUnit.

**Spec:** `docs/superpowers/specs/2026-10-14-stage1-foundation-design.md`

## Global Constraints

- Target frameworks: `net10.0` for UI-free projects, `net10.0-windows` for WinForms projects. No other TFMs.
- Shared MSBuild settings live only in `Directory.Build.props` (Nullable, ImplicitUsings, LangVersion, Version). `.csproj` files must not repeat them.
- All code comments and XML docs in **English**. All user-visible text comes from `Languages\*.json` (default `pl`, also `en`); designer text is only the fallback.
- Every Form/UserControl is a `partial class` with `.Designer.cs` (and `.resx` when the Designer creates one); layout lives in `InitializeComponent`, never in constructors.
- Native catalog format stays SQLite `.gc`; an existing `catalog.gc` must open without data loss.
- Projects reference only along the spec's dependency graph (spec §2). `GameCatalog.Core` and `GameCatalog.Localization` reference nothing in the solution.
- Plugin contracts carry `PluginApi.Version = 1`.
- Commit after every task with a message ending in:
  `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`

## Deviations from the spec (additive, agreed scope)

- Three extra test projects: `GameCatalog.Scripting.Tests` (Roslyn upgrade smoke test), `GameCatalog.UI.Tests` (components without showing windows), `GameCatalog.App.Tests` (session, numbering, presenter — the most bug-prone logic moved out of `MainForm`).
- Two pre-existing bugs fixed because the new tests expose them: AMC XML import reads child elements instead of attributes (AMC stores fields as attributes of `<Movie>`), and CSV import splits quoted fields.
- `GameCatalogModel.Filter(...)` and `StatsService` are unused today and are deleted (YAGNI); stage 3/9 will design them properly.

## Review Focus

1. **Existing user data:** a `catalog.gc` written by the current version (Polish `"Tak"` in YES/NO custom fields, Polish theme names like `"Klasyczny"` in `prefs.json`) must load and display correctly → tests in Task 3 (YES/NO values) and Task 7 (theme name migration).
2. **Missing or broken language file:** deleting `en.json` or corrupting `pl.json` must not crash the app; missing keys show the key text, form texts keep their designer values → tests in Task 5.
3. **Faulty plugin:** a plugin that throws in `Initialize`, references an incompatible API version, or is not a .NET assembly must be reported in the Plugins window while all other plugins and the app keep working → tests in Task 14.
4. **CSV round trip with delimiter/quotes/newlines inside fields:** export→import must preserve the title → test in Task 3.
5. **Opening an intentionally empty catalog:** sample games must be added only when the default catalog file did not exist (first run), never when the user opens an empty file → test in Task 8.

---

## File Structure

```
GameCatalog.slnx                                   modified: all projects
Directory.Build.props                              new
src/GameCatalog.Core/
  GameCatalog.Core.csproj                          modified
  Models/Game.cs, GameExtra.cs, LoanRecord.cs,
         CustomFieldDefinition.cs, Enums.cs         English comments
  Fields/GameFields.cs                             moved from Catalog/, FieldDef reshaped
  Fields/GameFieldValues.cs                        moved, uses ICatalogLabels
  Fields/GameFieldWriter.cs                        new (from MainForm.ApplyScriptResult)
  Fields/ICatalogLabels.cs                         new (+ InvariantCatalogLabels, CatalogLabels)
  Fields/YesNo.cs                                  new (YES/NO value parsing/storage)
  Catalog/GameList.cs                              renamed from GameCatalogModel.cs
  Catalog/NaturalStringComparer.cs                 split out
  Services/SampleDataFactory.cs                    English comments
  Services/StatsService.cs                         deleted (unused)
src/GameCatalog.Localization/
  LanguageInfo.cs, LanguageFile.cs, ITranslator.cs,
  LanguageManager.cs, FormKey.cs                   new
src/GameCatalog.Plugins.Abstractions/
  PluginApi.cs, GameCatalogPluginAttribute.cs, IPlugin.cs, IPluginHost.cs,
  IGameCatalog.cs, IGameSelection.cs, IGameImporter.cs, IGameExporter.cs,
  ImportContext.cs, ExportContext.cs, IGameInfoProvider.cs, IInfoProviderUi.cs,
  GameInfoResult.cs, ICatalogCommand.cs           new
src/GameCatalog.Plugins.Abstractions.WinForms/
  IGamePanelPlugin.cs, IWinFormsPluginHost.cs      new
src/GameCatalog.Plugins.Hosting/
  PluginLoadContext.cs, PluginDescriptor.cs, ExtensionRegistry.cs,
  PluginHostBase.cs, PluginManager.cs              new
src/GameCatalog.Storage/
  SqliteGameCatalogStore.cs                        CatalogData load, English comments
  CatalogData.cs                                   new
  Csv/CsvReader.cs                                 new (RFC-4180 reader)
  CatalogTransfer.cs                               uses CsvReader, labels
  AmcXmlImporter.cs                                reads attributes
  Exchange/CsvGameImporter.cs, AmcXmlGameImporter.cs, FolderGameImporter.cs,
           CsvGameExporter.cs, HtmlGameExporter.cs, BuiltInExchange.cs   new (Task 15)
src/GameCatalog.Scripting/
  ScriptRunner.cs                                  returns GameInfoResult
  ScriptApi.cs, GameScript.cs, ScriptSamples.cs    English comments
src/GameCatalog.UI/
  GameCatalog.UI.csproj                            new
  Icons/AmcIcon.cs                                 moved from App/AmcIcons.cs
  Components/UIAction.cs, ActionList.cs, Localizer.cs, IconThemeImageList.cs
  Controls/GameListView.cs, ThumbnailView.cs, HtmlPreview.cs (+Designer),
           PictureToolWindow.cs (+Designer), FindBar.cs (+Designer), StatsCanvas.cs
  Frames/GameFrame.cs, GameCustomFrame.cs, GameExtrasFrame.cs, GameExtraFrame.cs (+Designer each)
  Frames/IGameFrame.cs
src/GameCatalog.App/
  Program.cs                                       language, logging, plugins bootstrap
  MainForm.cs / MainForm.Designer.cs / MainForm.resx   rebuilt
  Forms/*.cs (+Designer)                           each dialog converted; LanguageSelectForm, PluginsForm new
  Services/CatalogSession.cs, UndoService.cs, GameListPresenter.cs,
           NumberingService.cs, GameClipboard.cs, CoverImages.cs, PrefsStore.cs (moved),
           TranslatorCatalogLabels.cs, HtmlTemplates.cs (moved)
  Plugins/AppPluginHost.cs, PluginCommandAdapter.cs
  Diagnostics/AppLog.cs
  Languages/pl.json, en.json
  Resources/*.bmp                                  unchanged
  (deleted) AmcIcons.cs, FieldValues.cs, Controls/GameFieldsControl.cs, Controls/ControlLayout.cs
tests/
  GameCatalog.Core.Tests/, GameCatalog.Storage.Tests/, GameCatalog.Scripting.Tests/,
  GameCatalog.Localization.Tests/, GameCatalog.UI.Tests/, GameCatalog.App.Tests/,
  GameCatalog.Plugins.Hosting.Tests/
samples/
  GameCatalog.Plugin.Sample/
```

Commands used throughout (run from the repository root):

- Build: `dotnet build GameCatalog.slnx`
- All tests: `dotnet test GameCatalog.slnx`
- One test: `dotnet test tests/<Project> --filter "FullyQualifiedName~<Name>"`
- Run app: `dotnet run --project src/GameCatalog.App`

---

### Task 1: .NET 10 solution scaffolding

**Files:**
- Create: `Directory.Build.props`
- Modify: `src/GameCatalog.Core/GameCatalog.Core.csproj`, `src/GameCatalog.Storage/GameCatalog.Storage.csproj`, `src/GameCatalog.Scripting/GameCatalog.Scripting.csproj`, `src/GameCatalog.App/GameCatalog.App.csproj`, `GameCatalog.slnx`
- Create: `src/GameCatalog.Localization/GameCatalog.Localization.csproj`, `src/GameCatalog.Plugins.Abstractions/GameCatalog.Plugins.Abstractions.csproj`, `src/GameCatalog.Plugins.Abstractions.WinForms/GameCatalog.Plugins.Abstractions.WinForms.csproj`, `src/GameCatalog.Plugins.Hosting/GameCatalog.Plugins.Hosting.csproj`, `src/GameCatalog.UI/GameCatalog.UI.csproj`
- Create: seven test projects under `tests/` (see Step 6)

**Interfaces:**
- Consumes: nothing.
- Produces: the project graph every later task builds in; project names exactly as listed.

- [ ] **Step 1: Create `Directory.Build.props`**

```xml
<Project>
  <!-- Settings shared by every project in the solution. -->
  <PropertyGroup>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <!-- Version scheme kept from the original port: YY.M.D -->
    <Version>26.10.14</Version>
    <Product>GameCatalog</Product>
    <Company>GameCatalog</Company>
    <Copyright>Port of Ant Movie Catalog (GPL) — https://www.antp.be/software/moviecatalog/</Copyright>
  </PropertyGroup>
</Project>
```

- [ ] **Step 2: Rewrite the four existing `.csproj` files**

`src/GameCatalog.Core/GameCatalog.Core.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
</Project>
```

`src/GameCatalog.Storage/GameCatalog.Storage.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\GameCatalog.Core\GameCatalog.Core.csproj" />
  </ItemGroup>
</Project>
```

`src/GameCatalog.Scripting/GameCatalog.Scripting.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\GameCatalog.Core\GameCatalog.Core.csproj" />
  </ItemGroup>
</Project>
```

`src/GameCatalog.App/GameCatalog.App.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
    <RootNamespace>GameCatalog.App</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\GameCatalog.Core\GameCatalog.Core.csproj" />
    <ProjectReference Include="..\GameCatalog.Storage\GameCatalog.Storage.csproj" />
    <ProjectReference Include="..\GameCatalog.Scripting\GameCatalog.Scripting.csproj" />
  </ItemGroup>
  <ItemGroup>
    <None Include="Resources\*.bmp" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>
</Project>
```

Delete `src/GameCatalog.App/GameCatalog.App.csproj.user` if it only contains Designer sub-type hints (it is ignored by git anyway).

- [ ] **Step 3: Add the latest stable packages**

```bash
dotnet add src/GameCatalog.Storage package Microsoft.Data.Sqlite
dotnet add src/GameCatalog.Scripting package Microsoft.CodeAnalysis.CSharp.Scripting
```
Expected: both commands print `info : PackageReference for package '...' version '...' added`.

- [ ] **Step 4: Create the five new library projects**

```bash
dotnet new classlib -f net10.0 -o src/GameCatalog.Localization
dotnet new classlib -f net10.0 -o src/GameCatalog.Plugins.Abstractions
dotnet new classlib -f net10.0 -o src/GameCatalog.Plugins.Hosting
dotnet new classlib -f net10.0-windows -o src/GameCatalog.Plugins.Abstractions.WinForms
dotnet new winformslib -f net10.0-windows -o src/GameCatalog.UI
```
Delete the generated `Class1.cs` / `UserControl1.cs` / `UserControl1.Designer.cs` files. Remove the `Nullable`, `ImplicitUsings` and `LangVersion` elements the templates emit (they come from `Directory.Build.props`).

Set references (edit the `.csproj` files so they contain exactly these `ProjectReference`s):

| Project | References |
|---|---|
| Plugins.Abstractions | Core, Localization |
| Plugins.Abstractions.WinForms | Plugins.Abstractions (add `<UseWindowsForms>true</UseWindowsForms>`) |
| Plugins.Hosting | Plugins.Abstractions |
| UI | Core, Localization |
| Storage | Core (Plugins.Abstractions is added in Task 15) |
| Scripting | Core, Plugins.Abstractions |
| App | Core, Localization, Storage, Scripting, Plugins.Abstractions, Plugins.Abstractions.WinForms, Plugins.Hosting, UI |

- [ ] **Step 5: Add WebView2 to the UI library**

```bash
dotnet add src/GameCatalog.UI package Microsoft.Web.WebView2
```

- [ ] **Step 6: Create the test projects**

```bash
dotnet new xunit -f net10.0 -o tests/GameCatalog.Core.Tests
dotnet new xunit -f net10.0 -o tests/GameCatalog.Storage.Tests
dotnet new xunit -f net10.0 -o tests/GameCatalog.Scripting.Tests
dotnet new xunit -f net10.0 -o tests/GameCatalog.Localization.Tests
dotnet new xunit -f net10.0-windows -o tests/GameCatalog.UI.Tests
dotnet new xunit -f net10.0-windows -o tests/GameCatalog.App.Tests
dotnet new xunit -f net10.0-windows -o tests/GameCatalog.Plugins.Hosting.Tests
```
Delete each generated `UnitTest1.cs`. In the three `net10.0-windows` test projects add `<UseWindowsForms>true</UseWindowsForms>`. Add project references: each test project references the project it is named after; `Storage.Tests` also references Core; `App.Tests` references App; `Plugins.Hosting.Tests` references Plugins.Hosting and (in Task 14) the sample plugin and test fixtures.

- [ ] **Step 7: Register everything in `GameCatalog.slnx`**

```xml
<Solution>
  <Folder Name="/src/">
    <Project Path="src/GameCatalog.App/GameCatalog.App.csproj" />
    <Project Path="src/GameCatalog.Core/GameCatalog.Core.csproj" />
    <Project Path="src/GameCatalog.Localization/GameCatalog.Localization.csproj" />
    <Project Path="src/GameCatalog.Plugins.Abstractions/GameCatalog.Plugins.Abstractions.csproj" />
    <Project Path="src/GameCatalog.Plugins.Abstractions.WinForms/GameCatalog.Plugins.Abstractions.WinForms.csproj" />
    <Project Path="src/GameCatalog.Plugins.Hosting/GameCatalog.Plugins.Hosting.csproj" />
    <Project Path="src/GameCatalog.Scripting/GameCatalog.Scripting.csproj" />
    <Project Path="src/GameCatalog.Storage/GameCatalog.Storage.csproj" />
    <Project Path="src/GameCatalog.UI/GameCatalog.UI.csproj" />
  </Folder>
  <Folder Name="/tests/">
    <Project Path="tests/GameCatalog.App.Tests/GameCatalog.App.Tests.csproj" />
    <Project Path="tests/GameCatalog.Core.Tests/GameCatalog.Core.Tests.csproj" />
    <Project Path="tests/GameCatalog.Localization.Tests/GameCatalog.Localization.Tests.csproj" />
    <Project Path="tests/GameCatalog.Plugins.Hosting.Tests/GameCatalog.Plugins.Hosting.Tests.csproj" />
    <Project Path="tests/GameCatalog.Scripting.Tests/GameCatalog.Scripting.Tests.csproj" />
    <Project Path="tests/GameCatalog.Storage.Tests/GameCatalog.Storage.Tests.csproj" />
    <Project Path="tests/GameCatalog.UI.Tests/GameCatalog.UI.Tests.csproj" />
  </Folder>
  <Folder Name="/samples/" />
</Solution>
```

- [ ] **Step 8: Build and fix .NET 10 breakages**

Run: `dotnet build GameCatalog.slnx`
Expected: `Build succeeded` with 0 errors. Known .NET 9/10 WinForms changes to fix if they appear:
- `WebBrowser` is still available — leave `MainForm` untouched here.
- `Clipboard.GetImage()` / `SetImage` still exist; if analyzer warning WFDEV005 (obsolete `Clipboard.GetData`) appears, ignore — no `GetData` calls exist.
- Nullable warnings are not errors; do not fix them in this task.

- [ ] **Step 9: Run tests and the app**

Run: `dotnet test GameCatalog.slnx` → Expected: all test projects report `No test is available` or `Passed: 0` without failure.
Run: `dotnet run --project src/GameCatalog.App` → the main window opens with the existing catalog; close it.

- [ ] **Step 10: Commit**

```bash
git add -A
git commit -m "build: move to .NET 10 and scaffold layered solution with test projects

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 2: Core — field catalogue, labels and `GameList`

**Files:**
- Create: `src/GameCatalog.Core/Fields/ICatalogLabels.cs`, `src/GameCatalog.Core/Fields/YesNo.cs`, `src/GameCatalog.Core/Fields/GameFieldWriter.cs`, `src/GameCatalog.Core/Catalog/NaturalStringComparer.cs`
- Move/rewrite: `Catalog/GameFields.cs` → `Fields/GameFields.cs`, `Catalog/GameFieldValues.cs` → `Fields/GameFieldValues.cs`, `Catalog/GameCatalogModel.cs` → `Catalog/GameList.cs`
- Delete: `src/GameCatalog.Core/Services/StatsService.cs`
- Modify (English comments only): `Models/*.cs`, `Services/SampleDataFactory.cs`
- Modify (compile fixes): every consumer in Storage, Scripting, App (`GameCatalogModel` → `GameList`, `GameFields.DisplayNameOf(tag)` → `CatalogLabels.Current.Field(tag)`, `GameCatalogModel.StatusLabel(s)` → `CatalogLabels.Current.Status(s)`, `Core.Catalog` → `Core.Fields` usings)
- Test: `tests/GameCatalog.Core.Tests/GameListTests.cs`, `GameFieldValuesTests.cs`, `GameFieldWriterTests.cs`, `NaturalStringComparerTests.cs`, `YesNoTests.cs`

**Interfaces:**
- Consumes: Task 1 projects.
- Produces:
  - `namespace GameCatalog.Core.Fields`: `ICatalogLabels { string Field(string tag); string Status(CompletionStatus s); string Yes {get;} string No {get;} string NoValue {get;} }`, `InvariantCatalogLabels : ICatalogLabels`, `static class CatalogLabels { ICatalogLabels Current {get;set;} }`
  - `GameFields.FieldDef(int Id, string Tag, string SqlTag, string DefaultCaption, string AmcOrigin)` with `string LabelKey => "Field." + Tag`; `GameFields.All`, `GameFields.GroupableTags`, `GameFields.SortableTags`, `GameFields.Searchable`, `GameFields.Find(string tag)`
  - `GameFieldValues.Get(Game g, string tag) : string`
  - `GameFieldWriter.Set(Game g, string tag, string value) : bool`
  - `YesNo.Parse(string? value) : bool`, `YesNo.StoredTrue = "1"`
  - `namespace GameCatalog.Core.Catalog`: `GameList` (`Games`, `CustomFieldDefs`, `NextNumber()`, `Add(Game)`, `Remove(int)`, `Find(int)`, `Clear()`, `ReplaceAll(IEnumerable<Game>)`, `DistinctValues(Func<Game,string>)`, static `Sort(...)`, static `GroupKeyOf(Game, string groupTag)`, static `GroupBy(IEnumerable<Game>, string groupTag)`, static `GroupCaption(string groupTag, string key, ICatalogLabels labels)`), `NaturalStringComparer.Instance`

- [ ] **Step 1: Write failing tests**

`tests/GameCatalog.Core.Tests/GameListTests.cs`:
```csharp
using GameCatalog.Core.Catalog;
using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;

namespace GameCatalog.Core.Tests;

public class GameListTests
{
    private static GameList Sample()
    {
        var list = new GameList();
        list.Add(new Game { Title = "Doom 2", Genre = "FPS", ReleaseYear = 1994 });
        list.Add(new Game { Title = "Doom 10", Genre = "", ReleaseYear = 0 });
        list.Add(new Game { Title = "Abe", Genre = "Platformer", CompletionStatus = CompletionStatus.Completed });
        return list;
    }

    [Fact]
    public void Add_AssignsSequentialNumbersAndDateAdded()
    {
        var list = Sample();
        Assert.Equal(new[] { 1, 2, 3 }, list.Games.Select(g => g.Number));
        Assert.All(list.Games, g => Assert.Equal(DateTime.Today, g.DateAdded));
        Assert.Equal(4, list.NextNumber());
    }

    [Fact]
    public void Add_KeepsExplicitNumber()
    {
        var list = new GameList();
        list.Add(new Game { Number = 42, Title = "X" });
        Assert.Equal(42, list.Find(42)!.Number);
        Assert.Equal(43, list.NextNumber());
    }

    [Fact]
    public void ReplaceAll_WithQueryOverOwnGames_DoesNotLoseItems()
    {
        var list = Sample();
        list.ReplaceAll(list.Games.OrderByDescending(g => g.Number));
        Assert.Equal(new[] { 3, 2, 1 }, list.Games.Select(g => g.Number));
    }

    [Fact]
    public void Sort_Natural_OrdersNumbersByValue()
    {
        var sorted = GameList.Sort(Sample().Games, "Title", ascending: true, natural: true).Select(g => g.Title);
        Assert.Equal(new[] { "Abe", "Doom 2", "Doom 10" }, sorted);
    }

    [Fact]
    public void Sort_Descending_ByYear()
    {
        var sorted = GameList.Sort(Sample().Games, "ReleaseYear", ascending: false).Select(g => g.ReleaseYear);
        Assert.Equal(new[] { 1994, 0, 0 }, sorted);
    }

    [Fact]
    public void GroupBy_EmptyValuesUseEmptyKey_StatusUsesEnumName()
    {
        var byGenre = GameList.GroupBy(Sample().Games, "Genre").Select(g => g.Key).ToList();
        Assert.Contains(string.Empty, byGenre);
        var byStatus = GameList.GroupBy(Sample().Games, "CompletionStatus").Select(g => g.Key).ToList();
        Assert.Contains("Completed", byStatus);
        Assert.Contains("Backlog", byStatus);
    }

    [Fact]
    public void GroupCaption_TranslatesEmptyAndStatus()
    {
        var labels = new InvariantCatalogLabels();
        Assert.Equal(labels.NoValue, GameList.GroupCaption("Genre", "", labels));
        Assert.Equal("Completed", GameList.GroupCaption("CompletionStatus", "Completed", labels));
        Assert.Equal("FPS", GameList.GroupCaption("Genre", "FPS", labels));
    }
}
```

`tests/GameCatalog.Core.Tests/NaturalStringComparerTests.cs`:
```csharp
using GameCatalog.Core.Catalog;

namespace GameCatalog.Core.Tests;

public class NaturalStringComparerTests
{
    [Theory]
    [InlineData("a2", "a10", -1)]
    [InlineData("a10", "a2", 1)]
    [InlineData("A1", "a1", 0)]
    [InlineData("a01", "a1", 1)]
    [InlineData(null, "a", -1)]
    public void Compare(string? x, string? y, int expectedSign)
        => Assert.Equal(expectedSign, Math.Sign(NaturalStringComparer.Instance.Compare(x, y)));
}
```

`tests/GameCatalog.Core.Tests/GameFieldValuesTests.cs`:
```csharp
using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;

namespace GameCatalog.Core.Tests;

public class GameFieldValuesTests
{
    [Fact]
    public void Get_FormatsRatingsYearsAndLabels()
    {
        CatalogLabels.Current = new InvariantCatalogLabels();
        var g = new Game { UserRating = 85, CriticRating = -1, ReleaseYear = 0, Checked = true,
                           CompletionStatus = CompletionStatus.Playing };
        Assert.Equal("8.5", GameFieldValues.Get(g, "UserRating").Replace(',', '.'));
        Assert.Equal("", GameFieldValues.Get(g, "CriticRating"));
        Assert.Equal("", GameFieldValues.Get(g, "ReleaseYear"));
        Assert.Equal("Yes", GameFieldValues.Get(g, "Checked"));
        Assert.Equal("Playing", GameFieldValues.Get(g, "CompletionStatus"));
    }

    [Fact]
    public void Get_UnknownTag_ReadsCustomValue()
    {
        var g = new Game();
        g.CustomValues["Disc"] = "Blue";
        Assert.Equal("Blue", GameFieldValues.Get(g, "disc"));
        Assert.Equal("", GameFieldValues.Get(g, "Missing"));
    }

    [Fact]
    public void EveryFieldTag_IsReadable()
    {
        var g = new Game();
        foreach (var f in GameFields.All) _ = GameFieldValues.Get(g, f.Tag);
    }

    [Fact]
    public void Searchable_ExcludesVirtualFields()
    {
        Assert.DoesNotContain(GameFields.Searchable, f => f.Tag is "DisplayTitle" or "NbExtras");
        Assert.Contains(GameFields.Searchable, f => f.Tag == "Title");
    }
}
```

`tests/GameCatalog.Core.Tests/GameFieldWriterTests.cs`:
```csharp
using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;

namespace GameCatalog.Core.Tests;

public class GameFieldWriterTests
{
    [Theory]
    [InlineData("8.5", 85)]
    [InlineData("8,5", 85)]
    [InlineData("10", 100)]
    public void Set_Rating_AcceptsBothDecimalSeparators(string value, int expected)
    {
        var g = new Game();
        Assert.True(GameFieldWriter.Set(g, "UserRating", value));
        Assert.Equal(expected, g.UserRating);
    }

    [Fact]
    public void Set_InvalidNumber_ReturnsFalseAndKeepsValue()
    {
        var g = new Game { ReleaseYear = 2000 };
        Assert.False(GameFieldWriter.Set(g, "ReleaseYear", "soon"));
        Assert.Equal(2000, g.ReleaseYear);
    }

    [Fact]
    public void Set_StringDatesStatusChecked()
    {
        CatalogLabels.Current = new InvariantCatalogLabels();
        var g = new Game();
        Assert.True(GameFieldWriter.Set(g, "Developer", "id Software"));
        Assert.True(GameFieldWriter.Set(g, "LastPlayed", "2024-05-01"));
        Assert.True(GameFieldWriter.Set(g, "CompletionStatus", "Completed"));
        Assert.True(GameFieldWriter.Set(g, "Checked", "tak"));
        Assert.True(GameFieldWriter.Set(g, "PlaytimeHours", "12,5"));
        Assert.Equal("id Software", g.Developer);
        Assert.Equal(new DateTime(2024, 5, 1), g.LastPlayed);
        Assert.Equal(CompletionStatus.Completed, g.CompletionStatus);
        Assert.True(g.Checked);
        Assert.Equal(12.5, g.PlaytimeHours);
    }

    [Fact]
    public void Set_CustomPrefixAndUnknownTags_GoToCustomValues_EmptyRemoves()
    {
        var g = new Game();
        GameFieldWriter.Set(g, "custom:Disc", "Blue");
        GameFieldWriter.Set(g, "Mood", "Calm");
        Assert.Equal("Blue", g.CustomValues["Disc"]);
        Assert.Equal("Calm", g.CustomValues["Mood"]);
        GameFieldWriter.Set(g, "custom:Disc", "");
        Assert.False(g.CustomValues.ContainsKey("Disc"));
    }

    [Fact]
    public void Set_VirtualFields_AreIgnored()
    {
        var g = new Game { Title = "A" };
        Assert.False(GameFieldWriter.Set(g, "DisplayTitle", "B"));
        Assert.False(GameFieldWriter.Set(g, "NbExtras", "3"));
        Assert.Equal("A", g.Title);
        Assert.Empty(g.CustomValues);
    }
}
```

`tests/GameCatalog.Core.Tests/YesNoTests.cs`:
```csharp
using GameCatalog.Core.Fields;

namespace GameCatalog.Core.Tests;

public class YesNoTests
{
    [Theory]
    [InlineData("1", true)]
    [InlineData("Tak", true)]
    [InlineData("tak", true)]
    [InlineData("true", true)]
    [InlineData("Yes", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("0", false)]
    [InlineData("Nie", false)]
    public void Parse_AcceptsLegacyPolishAndNeutralValues(string? value, bool expected)
        => Assert.Equal(expected, YesNo.Parse(value));
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test tests/GameCatalog.Core.Tests`
Expected: build FAIL — `GameList`, `ICatalogLabels`, `GameFieldWriter`, `YesNo` do not exist.

- [ ] **Step 3: Implement `ICatalogLabels.cs`**

```csharp
using GameCatalog.Core.Models;

namespace GameCatalog.Core.Fields;

/// <summary>
/// Language-dependent captions the core needs when it renders values as text
/// (field names, completion statuses, yes/no, the "(none)" bucket).
/// The UI layer supplies a translated implementation; the core never hard-codes a language.
/// </summary>
public interface ICatalogLabels
{
    /// <summary>Caption of the field identified by <paramref name="tag"/>.</summary>
    string Field(string tag);
    /// <summary>Caption of a completion status.</summary>
    string Status(CompletionStatus status);
    string Yes { get; }
    string No { get; }
    /// <summary>Caption of the group/statistics bucket for empty values (AMC "(none)").</summary>
    string NoValue { get; }
}

/// <summary>English captions taken from the field catalogue; used by tests and as a safe default.</summary>
public sealed class InvariantCatalogLabels : ICatalogLabels
{
    public string Field(string tag) => GameFields.Find(tag)?.DefaultCaption ?? tag;

    public string Status(CompletionStatus status) => status switch
    {
        CompletionStatus.Completed100 => "Completed 100%",
        _ => status.ToString(),
    };

    public string Yes => "Yes";
    public string No => "No";
    public string NoValue => "(none)";
}

/// <summary>Process-wide labels used by value formatting; replaced by the App at start-up.</summary>
public static class CatalogLabels
{
    public static ICatalogLabels Current { get; set; } = new InvariantCatalogLabels();
}
```

- [ ] **Step 4: Implement `YesNo.cs`**

```csharp
namespace GameCatalog.Core.Fields;

/// <summary>
/// Storage format of YES/NO custom fields. New values are stored as "1"; legacy catalogs
/// written by the Polish-only version stored "Tak", which must still read as true.
/// </summary>
public static class YesNo
{
    public const string StoredTrue = "1";

    private static readonly string[] TrueValues = { "1", "tak", "true", "yes" };

    public static bool Parse(string? value) =>
        value is not null && TrueValues.Contains(value.Trim(), StringComparer.OrdinalIgnoreCase);
}
```

- [ ] **Step 5: Implement `Fields/GameFields.cs`** (delete `Catalog/GameFields.cs`)

```csharp
namespace GameCatalog.Core.Fields;

/// <summary>
/// Field catalogue of a game record — port of AMC fields.pas.
/// Tags are the canonical identifiers used everywhere (sorting, grouping, scripts, plugins).
/// SqlTags keep the original AMC names so legacy AMC data maps onto game fields.
/// </summary>
public static class GameFields
{
    /// <param name="Id">Stable index, same order as AMC field ids.</param>
    /// <param name="Tag">Canonical field identifier.</param>
    /// <param name="SqlTag">Original AMC tag.</param>
    /// <param name="DefaultCaption">English caption, used when no translation is available.</param>
    /// <param name="AmcOrigin">How the AMC movie field was ported (documentation only).</param>
    public sealed record FieldDef(int Id, string Tag, string SqlTag, string DefaultCaption, string AmcOrigin)
    {
        /// <summary>Translation key of the caption, e.g. <c>Field.Developer</c>.</summary>
        public string LabelKey => "Field." + Tag;
    }

    public static readonly IReadOnlyList<FieldDef> All = new List<FieldDef>
    {
        new(0,  "Number",           "NUM",             "Number",              "Number (unchanged)"),
        new(1,  "Checked",          "CHECKED",         "Checked",             "Checked (unchanged)"),
        new(2,  "ColorTag",         "COLORTAG",        "Color tag",           "ColorTag (unchanged)"),
        new(3,  "MediaLabel",       "MEDIA",           "Media label",         "MediaLabel (unchanged)"),
        new(4,  "MediaType",        "MEDIATYPE",       "Media type",          "MediaType: DVD → digital/physical"),
        new(5,  "Store",            "SOURCE",          "Store / source",      "Source → store (Steam, GOG, Epic…)"),
        new(6,  "DateAdded",        "DATEADDED",       "Date added",          "Date (unchanged)"),
        new(7,  "Borrower",         "BORROWER",        "Borrower",            "Borrower (unchanged)"),
        new(8,  "DateFinished",     "DATEWATCHED",     "Date finished",       "DateWatched → date finished"),
        new(9,  "UserRating",       "USERRATING",      "User rating",         "UserRating (unchanged, 0–10)"),
        new(10, "CriticRating",     "RATING",          "Critic rating",       "Rating → Metacritic/OpenCritic"),
        new(11, "Title",            "ORIGINALTITLE",   "Title",               "OriginalTitle → title"),
        new(12, "LocalizedTitle",   "TRANSLATEDTITLE", "Localized title",     "TranslatedTitle → localized title"),
        new(13, "DisplayTitle",     "FORMATTEDTITLE",  "Display title",       "FormattedTitle (virtual)"),
        new(14, "Developer",        "DIRECTOR",        "Developer",           "Director → developer studio"),
        new(15, "Publisher",        "PRODUCER",        "Publisher",           "Producer → publisher"),
        new(16, "Designer",         "WRITER",          "Designer",            "Writer → designer/writer"),
        new(17, "Composer",         "COMPOSER",        "Composer",            "Composer (unchanged)"),
        new(18, "VoiceActors",      "ACTORS",          "Voice actors",        "Actors → voice actors"),
        new(19, "Country",          "COUNTRY",         "Country",             "Country (unchanged)"),
        new(20, "ReleaseYear",      "YEAR",            "Release year",        "Year → release year"),
        new(21, "PlaytimeHours",    "LENGTH",          "Playtime (h)",        "Length (min) → hours played"),
        new(22, "Genre",            "CATEGORY",        "Genre",               "Category → genre"),
        new(23, "Pegi",             "CERTIFICATION",   "PEGI",                "Certification → PEGI"),
        new(24, "URL",              "URL",             "Web page",            "URL (unchanged)"),
        new(25, "Description",      "DESCRIPTION",     "Description",         "Description (unchanged)"),
        new(26, "Comments",         "COMMENTS",        "Comments",            "Comments (unchanged)"),
        new(27, "InstallPath",      "FILEPATH",        "Install path",        "FilePath → install path"),
        new(28, "Engine",           "VIDEOFORMAT",     "Engine",              "VideoFormat → engine (Unity/Unreal…)"),
        new(29, "Version",          "VIDEOBITRATE",    "Version",             "VideoBitrate → game version"),
        new(30, "GameMode",         "AUDIOFORMAT",     "Game mode",           "AudioFormat → single/multi/co-op"),
        new(31, "Platform",         "AUDIOBITRATE",    "Platform",            "NEW: PC/PS5/Xbox/Switch…"),
        new(32, "Resolution",       "RESOLUTION",      "Resolution",          "Resolution (unchanged)"),
        new(33, "Framerate",        "FRAMERATE",       "Framerate",           "Framerate (unchanged)"),
        new(34, "AudioLanguages",   "LANGUAGES",       "Audio languages",     "Languages → dubbing"),
        new(35, "Subtitles",        "SUBTITLES",       "Subtitles",           "Subtitles (unchanged)"),
        new(36, "SizeGb",           "FILESIZE",        "Size (GB)",           "Size → size in GB"),
        new(37, "DiskCount",        "DISKS",           "Disc count",          "Disks (unchanged)"),
        new(38, "CompletionStatus", "COMPLETION",      "Status",              "NEW: backlog/playing/completed…"),
        new(39, "LastPlayed",       "LASTPLAYED",      "Last played",         "NEW: last session"),
        new(40, "NbExtras",         "NBEXTRAS",        "Extras count",        "NbExtras → DLC count (virtual)"),
    };

    /// <summary>Fields that are computed and can be neither edited nor searched.</summary>
    public static readonly IReadOnlySet<string> VirtualTags =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "DisplayTitle", "NbExtras" };

    public static readonly IReadOnlyList<string> GroupableTags = new[]
    {
        "Genre", "Platform", "Developer", "Publisher", "Store",
        "CompletionStatus", "Pegi", "ReleaseYear", "GameMode", "Engine",
    };

    /// <summary>Sort options offered in the View → Sort menu (same set as the original port).</summary>
    public static readonly IReadOnlyList<string> SortableTags = new[]
    {
        "Number", "Title", "Genre", "Platform", "Developer", "Publisher", "ReleaseYear",
        "UserRating", "CriticRating", "PlaytimeHours", "CompletionStatus", "DateAdded", "LastPlayed", "Store",
    };

    /// <summary>Fields offered by the find bar (AMC cbxField).</summary>
    public static readonly IReadOnlyList<FieldDef> Searchable =
        All.Where(f => !VirtualTags.Contains(f.Tag)).ToList();

    public static FieldDef? Find(string tag) =>
        All.FirstOrDefault(f => f.Tag.Equals(tag, StringComparison.OrdinalIgnoreCase));
}
```

- [ ] **Step 6: Implement `Fields/GameFieldValues.cs`** (delete `Catalog/GameFieldValues.cs`)

```csharp
using System.Globalization;
using GameCatalog.Core.Models;

namespace GameCatalog.Core.Fields;

/// <summary>Reads a game field as display text by tag (find bar, columns, scripts, preview).</summary>
public static class GameFieldValues
{
    public static string Get(Game g, string tag)
    {
        var labels = CatalogLabels.Current;
        return tag switch
        {
            "Number" => g.Number.ToString(CultureInfo.InvariantCulture),
            "Checked" => g.Checked ? labels.Yes : labels.No,
            "ColorTag" => g.ColorTag.ToString(CultureInfo.InvariantCulture),
            "MediaLabel" => g.MediaLabel,
            "MediaType" => g.MediaType,
            "Store" => g.Store,
            "DateAdded" => Date(g.DateAdded),
            "Borrower" => g.Borrower,
            "DateFinished" => Date(g.DateFinished),
            "UserRating" => Rating(g.UserRating),
            "CriticRating" => Rating(g.CriticRating),
            "Title" => g.Title,
            "LocalizedTitle" => g.LocalizedTitle,
            "DisplayTitle" => g.DisplayTitle,
            "Developer" => g.Developer,
            "Publisher" => g.Publisher,
            "Designer" => g.Designer,
            "Composer" => g.Composer,
            "VoiceActors" => g.VoiceActors,
            "Country" => g.Country,
            "ReleaseYear" => g.ReleaseYear > 0 ? g.ReleaseYear.ToString(CultureInfo.InvariantCulture) : "",
            "PlaytimeHours" => g.PlaytimeHours > 0 ? g.PlaytimeHours.ToString("0.#", CultureInfo.CurrentCulture) : "",
            "Genre" => g.Genre,
            "Pegi" => g.Pegi,
            "URL" => g.Url,
            "Description" => g.Description,
            "Comments" => g.Comments,
            "InstallPath" => g.InstallPath,
            "Engine" => g.Engine,
            "Version" => g.Version,
            "GameMode" => g.GameMode,
            "Platform" => g.Platform,
            "Resolution" => g.Resolution,
            "Framerate" => g.Framerate,
            "AudioLanguages" => g.AudioLanguages,
            "Subtitles" => g.Subtitles,
            "SizeGb" => g.SizeGb,
            "DiskCount" => g.DiskCount > 0 ? g.DiskCount.ToString(CultureInfo.InvariantCulture) : "",
            "CompletionStatus" => labels.Status(g.CompletionStatus),
            "LastPlayed" => Date(g.LastPlayed),
            "NbExtras" => g.NbExtras.ToString(CultureInfo.InvariantCulture),
            _ => g.CustomValues.TryGetValue(tag, out var v) ? v : "",
        };
    }

    /// <summary>Ratings are stored as 0..100 (-1 = none) and shown as 0.0..10.0.</summary>
    public static string Rating(int rating) =>
        rating < 0 ? "" : (rating / 10.0).ToString("0.0", CultureInfo.CurrentCulture);

    private static string Date(DateTime? d) => d?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "";
}
```

Note: the test `Get_FormatsRatingsYearsAndLabels` normalizes `,` → `.` because `CurrentCulture` may be `pl-PL`.

- [ ] **Step 7: Implement `Fields/GameFieldWriter.cs`**

```csharp
using System.Globalization;
using GameCatalog.Core.Models;

namespace GameCatalog.Core.Fields;

/// <summary>
/// Writes a textual value into the field identified by tag — the inverse of <see cref="GameFieldValues"/>.
/// Used when applying script / info-provider results. Unknown tags and tags prefixed with
/// <c>custom:</c> are written to <see cref="Game.CustomValues"/> (an empty value removes the entry).
/// </summary>
public static class GameFieldWriter
{
    public const string CustomPrefix = "custom:";

    /// <returns>False when the value cannot be parsed or the field is virtual; the game is then unchanged.</returns>
    public static bool Set(Game g, string tag, string value)
    {
        value ??= "";
        if (tag.StartsWith(CustomPrefix, StringComparison.Ordinal))
            return SetCustom(g, tag[CustomPrefix.Length..], value);
        if (GameFields.VirtualTags.Contains(tag)) return false;

        switch (tag)
        {
            case "Number": return TryInt(value, v => g.Number = v);
            case "ColorTag": return TryInt(value, v => g.ColorTag = v);
            case "ReleaseYear": return TryInt(value, v => g.ReleaseYear = v);
            case "DiskCount": return TryInt(value, v => g.DiskCount = v);
            case "UserRating": return TryDouble(value, v => g.UserRating = (int)Math.Round(v * 10));
            case "CriticRating": return TryDouble(value, v => g.CriticRating = (int)Math.Round(v * 10));
            case "PlaytimeHours": return TryDouble(value, v => g.PlaytimeHours = v);
            case "DateAdded": return TryDate(value, v => g.DateAdded = v);
            case "DateFinished": return TryDate(value, v => g.DateFinished = v);
            case "LastPlayed": return TryDate(value, v => g.LastPlayed = v);
            case "Checked": g.Checked = YesNo.Parse(value) || value.Equals(CatalogLabels.Current.Yes, StringComparison.OrdinalIgnoreCase); return true;
            case "CompletionStatus":
                if (!TryParseStatus(value, out var status)) return false;
                g.CompletionStatus = status;
                return true;
            case "MediaLabel": g.MediaLabel = value; return true;
            case "MediaType": g.MediaType = value; return true;
            case "Store": g.Store = value; return true;
            case "Borrower": g.Borrower = value; return true;
            case "Title": g.Title = value; return true;
            case "LocalizedTitle": g.LocalizedTitle = value; return true;
            case "Developer": g.Developer = value; return true;
            case "Publisher": g.Publisher = value; return true;
            case "Designer": g.Designer = value; return true;
            case "Composer": g.Composer = value; return true;
            case "VoiceActors": g.VoiceActors = value; return true;
            case "Country": g.Country = value; return true;
            case "Genre": g.Genre = value; return true;
            case "Pegi": g.Pegi = value; return true;
            case "URL": g.Url = value; return true;
            case "Description": g.Description = value; return true;
            case "Comments": g.Comments = value; return true;
            case "InstallPath": g.InstallPath = value; return true;
            case "Engine": g.Engine = value; return true;
            case "Version": g.Version = value; return true;
            case "GameMode": g.GameMode = value; return true;
            case "Platform": g.Platform = value; return true;
            case "Resolution": g.Resolution = value; return true;
            case "Framerate": g.Framerate = value; return true;
            case "AudioLanguages": g.AudioLanguages = value; return true;
            case "Subtitles": g.Subtitles = value; return true;
            case "SizeGb": g.SizeGb = value; return true;
            default: return SetCustom(g, tag, value);
        }
    }

    /// <summary>Accepts the enum name or the caption in the current language.</summary>
    public static bool TryParseStatus(string value, out CompletionStatus status)
    {
        foreach (var s in Enum.GetValues<CompletionStatus>())
        {
            if (s.ToString().Equals(value, StringComparison.OrdinalIgnoreCase) ||
                CatalogLabels.Current.Status(s).Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                status = s;
                return true;
            }
        }
        status = CompletionStatus.Backlog;
        return false;
    }

    private static bool SetCustom(Game g, string tag, string value)
    {
        if (value.Length == 0) g.CustomValues.Remove(tag);
        else g.CustomValues[tag] = value;
        return true;
    }

    private static bool TryInt(string s, Action<int> apply)
    {
        if (!int.TryParse(s.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)) return false;
        apply(v);
        return true;
    }

    private static bool TryDouble(string s, Action<double> apply)
    {
        if (!double.TryParse(s.Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var v)) return false;
        apply(v);
        return true;
    }

    private static bool TryDate(string s, Action<DateTime?> apply)
    {
        if (!DateTime.TryParse(s.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var v) &&
            !DateTime.TryParse(s.Trim(), CultureInfo.CurrentCulture, DateTimeStyles.None, out v)) return false;
        apply(v.Date);
        return true;
    }
}
```

- [ ] **Step 8: Implement `Catalog/NaturalStringComparer.cs`** — move the class unchanged from `GameCatalogModel.cs` into its own file in namespace `GameCatalog.Core.Catalog`, translating the two comments:
  - class summary: `/// <summary>Natural comparison like AMC: digit runs compare by numeric value, not lexically.</summary>`
  - inline: `// Equal values — fewer leading zeros sorts first (stable).`

- [ ] **Step 9: Implement `Catalog/GameList.cs`** (delete `GameCatalogModel.cs`)

```csharp
using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;

namespace GameCatalog.Core.Catalog;

/// <summary>
/// In-memory catalog — counterpart of AMC TMovieList plus its sort/group operations (main.pas).
/// Persistence is handled by GameCatalog.Storage.
/// </summary>
public sealed class GameList
{
    private readonly List<Game> _games = new();

    public IReadOnlyList<Game> Games => _games;
    public List<CustomFieldDefinition> CustomFieldDefs { get; } = new();

    public int NextNumber() => _games.Count == 0 ? 1 : _games.Max(g => g.Number) + 1;

    /// <summary>Adds a game; assigns the next free number when <see cref="Game.Number"/> is not positive.</summary>
    public void Add(Game game)
    {
        if (game.Number <= 0) game.Number = NextNumber();
        game.DateAdded ??= DateTime.Today;
        _games.Add(game);
    }

    public bool Remove(int number) => _games.RemoveAll(g => g.Number == number) > 0;

    public Game? Find(int number) => _games.FirstOrDefault(g => g.Number == number);

    public void Clear() => _games.Clear();

    public void ReplaceAll(IEnumerable<Game> games)
    {
        // Snapshot before Clear: callers often pass a deferred query over this very list.
        var snapshot = games.ToList();
        _games.Clear();
        _games.AddRange(snapshot);
    }

    /// <summary>Distinct non-empty values of one field, sorted (feeds editor drop-downs like AMC lists).</summary>
    public IEnumerable<string> DistinctValues(Func<Game, string> selector) =>
        _games.Select(selector).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().OrderBy(s => s);

    // --- Sorting (AMC Sort) ---
    public static IEnumerable<Game> Sort(IEnumerable<Game> source, string tag, bool ascending = true, bool natural = false)
    {
        if (natural && tag == "Title")
            return ascending
                ? source.OrderBy(g => g.Title, NaturalStringComparer.Instance)
                : source.OrderByDescending(g => g.Title, NaturalStringComparer.Instance);
        Func<Game, object?> key = tag switch
        {
            "Number" => g => g.Number,
            "Title" => g => g.Title,
            "Genre" => g => g.Genre,
            "Platform" => g => g.Platform,
            "Developer" => g => g.Developer,
            "Publisher" => g => g.Publisher,
            "ReleaseYear" => g => g.ReleaseYear,
            "UserRating" => g => g.UserRating,
            "CriticRating" => g => g.CriticRating,
            "PlaytimeHours" => g => g.PlaytimeHours,
            "CompletionStatus" => g => g.CompletionStatus,
            "DateAdded" => g => g.DateAdded,
            "LastPlayed" => g => g.LastPlayed,
            "Store" => g => g.Store,
            _ => g => g.Number,
        };
        return ascending ? source.OrderBy(key) : source.OrderByDescending(key);
    }

    // --- Grouping (AMC GroupBy) ---

    /// <summary>
    /// Language-neutral group key of a game: empty string for "no value",
    /// enum name for <see cref="CompletionStatus"/>, raw field text otherwise.
    /// </summary>
    public static string GroupKeyOf(Game g, string groupTag) => groupTag switch
    {
        "Genre" => g.Genre?.Trim() ?? "",
        "Platform" => g.Platform?.Trim() ?? "",
        "Developer" => g.Developer?.Trim() ?? "",
        "Publisher" => g.Publisher?.Trim() ?? "",
        "Store" => g.Store?.Trim() ?? "",
        "CompletionStatus" => g.CompletionStatus.ToString(),
        "ReleaseYear" => g.ReleaseYear > 0 ? g.ReleaseYear.ToString() : "",
        "Pegi" => g.Pegi?.Trim() ?? "",
        "GameMode" => g.GameMode?.Trim() ?? "",
        "Engine" => g.Engine?.Trim() ?? "",
        _ => "",
    };

    public static IEnumerable<IGrouping<string, Game>> GroupBy(IEnumerable<Game> source, string groupTag) =>
        source.GroupBy(g => GroupKeyOf(g, groupTag)).OrderBy(g => g.Key, StringComparer.CurrentCulture);

    /// <summary>Caption shown for a group key in the current language.</summary>
    public static string GroupCaption(string groupTag, string key, ICatalogLabels labels)
    {
        if (key.Length == 0) return labels.NoValue;
        if (groupTag == "CompletionStatus" && Enum.TryParse<CompletionStatus>(key, out var s)) return labels.Status(s);
        return key;
    }
}
```

- [ ] **Step 10: Delete `Services/StatsService.cs`; translate comments in `Models/*.cs` and `Services/SampleDataFactory.cs` to English**

Rules: translate every `//` and `///` comment literally into English; keep data values (e.g. `"Cyfrowa"`, `"Pudełko"`) unchanged — they are stored catalog values, localizing them belongs to stage 7. Example for `Enums.cs`:
```csharp
/// <summary>Completion status (AMC "DateWatched" extended with a player workflow).</summary>
public enum CompletionStatus
{
    Backlog = 0,      // not started
    Playing = 1,      // in progress
    Completed = 2,    // finished
    Completed100 = 3, // 100% / platinum
    Paused = 4,       // on hold
    Dropped = 5       // abandoned
}
```

- [ ] **Step 11: Fix consumers so the solution compiles**

Apply these replacements across `src/` (not `tests/`):
- `using GameCatalog.Core.Catalog;` stays where `GameList` is used; add `using GameCatalog.Core.Fields;` where `GameFields`/`GameFieldValues` are used.
- `GameCatalogModel` → `GameList`.
- `GameFields.DisplayNameOf(x)` → `CatalogLabels.Current.Field(x)`.
- `GameCatalogModel.StatusLabel(x)` → `CatalogLabels.Current.Status(x)`.
- `_model.DistinctGenres()` / `DistinctPlatforms()` (if referenced) → `_model.DistinctValues(g => g.Genre)` / `(g => g.Platform)`.
- In `MainForm.GroupKey` replace the body with `GameList.GroupKeyOf(g, _groupTag)` and, where group headers are created (`RefreshList`), use `GameList.GroupCaption(_groupTag, grp.Key, CatalogLabels.Current)` for the visible name while keeping `grp.Key` as the dictionary key.
- In `MainForm.ApplyScriptResult` replace the whole `foreach` body with `GameFieldWriter.Set(g, tag, value);` and delete `ParseStatus`.
- In `MainForm.CommitCustomField` and `LoadGameToFrame`: YES/NO read `YesNo.Parse(v)`, write `c.Checked ? YesNo.StoredTrue : ""`.
- Until Task 6 provides translations, set Polish captions at start-up so the UI does not change: add to `Program.Main` before `Application.Run`:
  ```csharp
  // Temporary until Task 6 wires the language files: keep the Polish UI captions.
  Core.Fields.CatalogLabels.Current = new PolishCatalogLabels();
  ```
  and add `src/GameCatalog.App/PolishCatalogLabels.cs` (deleted again in Task 6):
  ```csharp
  using GameCatalog.Core.Fields;
  using GameCatalog.Core.Models;

  namespace GameCatalog.App;

  /// <summary>Interim Polish captions (replaced by TranslatorCatalogLabels in Task 6).</summary>
  internal sealed class PolishCatalogLabels : ICatalogLabels
  {
      private static readonly Dictionary<string, string> Fields = new()
      {
          ["Number"] = "Numer", ["Checked"] = "Zaznaczona", ["ColorTag"] = "Etykieta koloru",
          ["MediaLabel"] = "Etykieta nośnika", ["MediaType"] = "Typ nośnika", ["Store"] = "Sklep / źródło",
          ["DateAdded"] = "Data dodania", ["Borrower"] = "Pożyczona komu", ["DateFinished"] = "Data ukończenia",
          ["UserRating"] = "Ocena własna", ["CriticRating"] = "Ocena recenzentów", ["Title"] = "Tytuł",
          ["LocalizedTitle"] = "Tytuł polski", ["DisplayTitle"] = "Tytuł wyświetlany", ["Developer"] = "Developer",
          ["Publisher"] = "Wydawca", ["Designer"] = "Projektant", ["Composer"] = "Kompozytor",
          ["VoiceActors"] = "Aktorzy głosowi", ["Country"] = "Kraj", ["ReleaseYear"] = "Rok wydania",
          ["PlaytimeHours"] = "Czas gry (h)", ["Genre"] = "Gatunek", ["Pegi"] = "PEGI", ["URL"] = "Strona WWW",
          ["Description"] = "Opis", ["Comments"] = "Komentarze", ["InstallPath"] = "Ścieżka instalacji",
          ["Engine"] = "Silnik", ["Version"] = "Wersja", ["GameMode"] = "Tryb gry", ["Platform"] = "Platforma",
          ["Resolution"] = "Rozdzielczość", ["Framerate"] = "Klatkaż", ["AudioLanguages"] = "Dubbing",
          ["Subtitles"] = "Napisy", ["SizeGb"] = "Rozmiar (GB)", ["DiskCount"] = "Liczba płyt",
          ["CompletionStatus"] = "Status", ["LastPlayed"] = "Ostatnio grano", ["NbExtras"] = "Liczba dodatków",
      };

      public string Field(string tag) => Fields.TryGetValue(tag, out var s) ? s : tag;

      public string Status(CompletionStatus status) => status switch
      {
          CompletionStatus.Backlog => "Do zagrania",
          CompletionStatus.Playing => "Ogrywana",
          CompletionStatus.Completed => "Ukończona",
          CompletionStatus.Completed100 => "Ukończona 100%",
          CompletionStatus.Paused => "Wstrzymana",
          CompletionStatus.Dropped => "Porzucona",
          _ => status.ToString(),
      };

      public string Yes => "Tak";
      public string No => "Nie";
      public string NoValue => "(brak)";
  }
  ```

- [ ] **Step 12: Run tests and build**

Run: `dotnet test tests/GameCatalog.Core.Tests` → Expected: all PASS.
Run: `dotnet build GameCatalog.slnx` → Expected: `Build succeeded`.

- [ ] **Step 13: Smoke-run the app**

Run: `dotnet run --project src/GameCatalog.App`. Check: list shows games, grouping by Status shows Polish captions, group "(brak)" appears for empty genre, applying a script still fills fields. Close.

- [ ] **Step 14: Commit**

```bash
git add -A
git commit -m "refactor(core): field catalogue with label abstraction, GameList, GameFieldWriter

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 3: Storage — CatalogData, CSV reader, AMC XML attributes, tests

**Files:**
- Create: `src/GameCatalog.Storage/CatalogData.cs`, `src/GameCatalog.Storage/Csv/CsvReader.cs`
- Modify: `src/GameCatalog.Storage/SqliteGameCatalogStore.cs`, `CatalogTransfer.cs`, `AmcXmlImporter.cs`; `src/GameCatalog.App/Forms/MainForm.cs` (`OpenDatabase` call site)
- Test: `tests/GameCatalog.Storage.Tests/SqliteStoreTests.cs`, `CsvTests.cs`, `AmcXmlImporterTests.cs`, `TestData/amc-sample.xml`

**Interfaces:**
- Consumes: `GameList`, `GameFieldValues`, `CatalogLabels` (Task 2).
- Produces:
  - `public sealed record CatalogData(List<Game> Games, List<CustomFieldDefinition> CustomFieldDefs, List<LoanRecord> Loans)`
  - `SqliteGameCatalogStore(string filePath)`, `.FilePath`, `.Initialize()`, `.SaveAll(GameList list, IEnumerable<LoanRecord> loans)`, `.LoadAll() : CatalogData`
  - `CsvReader.ReadRecords(TextReader reader, char delimiter) : IEnumerable<string[]>`
  - `CatalogTransfer.ExportCsv(IEnumerable<Game>, string path, char delimiter = ';')`, `ImportCsv(string path, int startNumber, char delimiter = ';') : List<Game>`, `ExportHtml(IEnumerable<Game>, string path, string title)`, `ImportFromFolder(string folder, int startNumber, string? patterns) : List<Game>`
  - `AmcXmlImporter.Import(string xmlPath, int startNumber) : List<Game>`

- [ ] **Step 1: Write failing tests**

`tests/GameCatalog.Storage.Tests/SqliteStoreTests.cs`:
```csharp
using GameCatalog.Core.Catalog;
using GameCatalog.Core.Models;
using Microsoft.Data.Sqlite;

namespace GameCatalog.Storage.Tests;

public sealed class SqliteStoreTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"gc-test-{Guid.NewGuid():N}.gc");

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_path)) File.Delete(_path);
    }

    [Fact]
    public void SaveAll_ThenLoadAll_RoundTripsEveryTable()
    {
        var list = new GameList();
        var g = new Game
        {
            Title = "Half-Life", LocalizedTitle = "HL", Developer = "Valve", ReleaseYear = 1998,
            PlaytimeHours = 12.5, UserRating = 95, CriticRating = -1, CompletionStatus = CompletionStatus.Completed100,
            LastPlayed = new DateTime(2024, 1, 2), CoverImage = new byte[] { 1, 2, 3 }, Checked = true, ColorTag = 3,
        };
        g.Extras.Add(new GameExtra { Title = "Opposing Force", Category = "Expansion" });
        g.CustomValues["Disc"] = "Blue";
        list.Add(g);
        list.CustomFieldDefs.Add(new CustomFieldDefinition { Tag = "Disc", Name = "Disc", FieldType = "Text" });
        var loans = new List<LoanRecord> { new() { GameNumber = 1, GameTitle = "Half-Life", Borrower = "Ola", LoanDate = new DateTime(2024, 3, 4) } };

        var store = new SqliteGameCatalogStore(_path);
        store.SaveAll(list, loans);
        var data = store.LoadAll();

        var r = Assert.Single(data.Games);
        Assert.Equal("Half-Life", r.Title);
        Assert.Equal("HL", r.LocalizedTitle);
        Assert.Equal(12.5, r.PlaytimeHours);
        Assert.Equal(95, r.UserRating);
        Assert.Equal(-1, r.CriticRating);
        Assert.Equal(CompletionStatus.Completed100, r.CompletionStatus);
        Assert.Equal(new DateTime(2024, 1, 2), r.LastPlayed);
        Assert.Equal(new byte[] { 1, 2, 3 }, r.CoverImage);
        Assert.True(r.Checked);
        Assert.Equal(3, r.ColorTag);
        Assert.Equal("Opposing Force", Assert.Single(r.Extras).Title);
        Assert.Equal("Blue", r.CustomValues["Disc"]);
        Assert.Equal("Disc", Assert.Single(data.CustomFieldDefs).Tag);
        Assert.Equal("Ola", Assert.Single(data.Loans).Borrower);
    }

    [Fact]
    public void SaveAll_RemovesDeletedGamesAndTheirChildren()
    {
        var list = new GameList();
        list.Add(new Game { Title = "A" });
        list.Add(new Game { Title = "B", Extras = { new GameExtra { Title = "DLC" } } });
        var store = new SqliteGameCatalogStore(_path);
        store.SaveAll(list, Array.Empty<LoanRecord>());

        list.Remove(2);
        store.SaveAll(list, Array.Empty<LoanRecord>());

        var data = store.LoadAll();
        Assert.Equal("A", Assert.Single(data.Games).Title);
        Assert.Empty(data.Games[0].Extras);
    }

    [Fact]
    public void LoadAll_LegacyPolishYesValue_IsPreserved()
    {
        var list = new GameList();
        var g = new Game { Title = "A" };
        g.CustomValues["Owned"] = "Tak";
        list.Add(g);
        var store = new SqliteGameCatalogStore(_path);
        store.SaveAll(list, Array.Empty<LoanRecord>());

        var loaded = store.LoadAll().Games[0];
        Assert.True(Core.Fields.YesNo.Parse(loaded.CustomValues["Owned"]));
    }
}
```

`tests/GameCatalog.Storage.Tests/CsvTests.cs`:
```csharp
using GameCatalog.Core.Models;
using GameCatalog.Storage.Csv;

namespace GameCatalog.Storage.Tests;

public sealed class CsvTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"gc-test-{Guid.NewGuid():N}.csv");
    public void Dispose() { if (File.Exists(_path)) File.Delete(_path); }

    [Fact]
    public void ReadRecords_HandlesQuotesDelimitersAndNewlines()
    {
        var text = "a;b;c\n\"x;y\";\"he said \"\"hi\"\"\";\"line1\nline2\"\n";
        var rows = CsvReader.ReadRecords(new StringReader(text), ';').ToList();
        Assert.Equal(2, rows.Count);
        Assert.Equal(new[] { "x;y", "he said \"hi\"", "line1\nline2" }, rows[1]);
    }

    [Fact]
    public void ExportThenImport_PreservesTitleWithDelimiterAndQuotes()
    {
        var games = new[] { new Game { Number = 1, Title = "Warhammer 40,000; \"Dawn\"", Developer = "Relic", ReleaseYear = 2004, Genre = "RTS", Platform = "PC" } };
        CatalogTransfer.ExportCsv(games, _path, ';');

        var imported = CatalogTransfer.ImportCsv(_path, startNumber: 10, ';');

        var g = Assert.Single(imported);
        Assert.Equal(10, g.Number);
        Assert.Equal("Warhammer 40,000; \"Dawn\"", g.Title);
        Assert.Equal("Relic", g.Developer);
        Assert.Equal(2004, g.ReleaseYear);
        Assert.Equal("RTS", g.Genre);
    }

    [Fact]
    public void ImportCsv_SkipsBlankLinesAndShortRows()
    {
        File.WriteAllText(_path, "Number;Title\n\n1\n2;Quake\n");
        var g = Assert.Single(CatalogTransfer.ImportCsv(_path, 1, ';'));
        Assert.Equal("Quake", g.Title);
    }
}
```

`tests/GameCatalog.Storage.Tests/TestData/amc-sample.xml` (mark `CopyToOutputDirectory=PreserveNewest` in the test `.csproj`: `<None Include="TestData\**" CopyToOutputDirectory="PreserveNewest" />`):
```xml
<?xml version="1.0" encoding="UTF-8"?>
<AntMovieCatalog Format="43" Version="4.2.3 (2023-09-18)" Date="2024-01-01">
  <Catalog>
    <Properties Owner="test"/>
    <Contents>
      <Movie Number="7" Checked="True" MediaLabel="A1" MediaType="DVD" Source="Shop" Borrower=""
             UserRating="80" Rating="72" OriginalTitle="Blade Runner" TranslatedTitle="Łowca androidów"
             Director="Ridley Scott" Producer="Warner" Writer="Fancher" Composer="Vangelis"
             Actors="Harrison Ford" Country="USA" Category="Sci-Fi" Year="1982" Length="150"
             URL="http://example.com" Description="Neo-noir" Comments="Classic" FilePath="C:\br.mkv"
             VideoFormat="XviD" Languages="English" Subtitles="Polish" Size="700" DateWatched="2020-05-06"/>
      <Movie Number="8" TranslatedTitle="Tylko polski"/>
    </Contents>
  </Catalog>
</AntMovieCatalog>
```

`tests/GameCatalog.Storage.Tests/AmcXmlImporterTests.cs`:
```csharp
namespace GameCatalog.Storage.Tests;

public class AmcXmlImporterTests
{
    private static string Sample => Path.Combine(AppContext.BaseDirectory, "TestData", "amc-sample.xml");

    [Fact]
    public void Import_ReadsAttributesAndMapsMovieFieldsToGameFields()
    {
        var games = AmcXmlImporter.Import(Sample, startNumber: 100);

        Assert.Equal(2, games.Count);
        var g = games[0];
        Assert.Equal(100, g.Number);
        Assert.Equal("Blade Runner", g.Title);
        Assert.Equal("Łowca androidów", g.LocalizedTitle);
        Assert.Equal("Ridley Scott", g.Developer);
        Assert.Equal("Warner", g.Publisher);
        Assert.Equal("Fancher", g.Designer);
        Assert.Equal("Sci-Fi", g.Genre);
        Assert.Equal(1982, g.ReleaseYear);
        Assert.Equal(2.5, g.PlaytimeHours);      // 150 min → 2.5 h
        Assert.Equal(80, g.UserRating);
        Assert.Equal(72, g.CriticRating);
        Assert.Equal("Fizyczna", g.MediaType);
        Assert.Equal("XviD", g.Engine);
        Assert.Equal(new DateTime(2020, 5, 6), g.DateFinished);
        Assert.True(g.Checked);
    }

    [Fact]
    public void Import_MissingOriginalTitle_FallsBackToTranslatedTitle()
    {
        var g = AmcXmlImporter.Import(Sample, 1)[1];
        Assert.Equal("Tylko polski", g.Title);
        Assert.Equal(-1, g.UserRating);
        Assert.Equal(-1, g.CriticRating);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test tests/GameCatalog.Storage.Tests`
Expected: build FAIL (`CatalogData`, `CsvReader`, `LoadAll()` without arguments missing).

- [ ] **Step 3: Add `CatalogData.cs`**

```csharp
using GameCatalog.Core.Models;

namespace GameCatalog.Storage;

/// <summary>Everything stored in one catalog file.</summary>
public sealed record CatalogData(
    List<Game> Games,
    List<CustomFieldDefinition> CustomFieldDefs,
    List<LoanRecord> Loans);
```

- [ ] **Step 4: Change `SqliteGameCatalogStore`**

1. Signature `public void SaveAll(GameList list, IEnumerable<LoanRecord> loans)` (rename parameter, body uses `list.Games` / `list.CustomFieldDefs`).
2. Replace `LoadAll(GameCatalogModel model)` by `public CatalogData LoadAll()`: build a local `var defs = new List<CustomFieldDefinition>();` instead of `model.CustomFieldDefs`, and `return new CatalogData(games, defs, loans);`.
3. Translate all comments to English; class summary:
```csharp
/// <summary>
/// SQLite catalog store — counterpart of AMC catalog load/save (XML/SQL), game edition:
/// one .gc database with Games / Extras / Loans / CustomFieldDefs / CustomValues tables.
/// SaveAll rewrites the whole catalog in a single transaction.
/// </summary>
```
4. Update `MainForm.OpenDatabase`:
```csharp
var data = _store.LoadAll();
_model.ReplaceAll(data.Games);
_model.CustomFieldDefs.Clear();
_model.CustomFieldDefs.AddRange(data.CustomFieldDefs);
_loans.Clear();
_loans.AddRange(data.Loans);
```

- [ ] **Step 5: Add `Csv/CsvReader.cs`**

```csharp
using System.Text;

namespace GameCatalog.Storage.Csv;

/// <summary>Minimal RFC 4180 reader: quoted fields may contain delimiters, doubled quotes and line breaks.</summary>
public static class CsvReader
{
    public static IEnumerable<string[]> ReadRecords(TextReader reader, char delimiter)
    {
        var fields = new List<string>();
        var field = new StringBuilder();
        bool inQuotes = false, any = false;
        int c;
        while ((c = reader.Read()) != -1)
        {
            char ch = (char)c;
            any = true;
            if (inQuotes)
            {
                if (ch == '"')
                {
                    if (reader.Peek() == '"') { field.Append('"'); reader.Read(); }
                    else inQuotes = false;
                }
                else field.Append(ch);
                continue;
            }
            if (ch == '"' && field.Length == 0) { inQuotes = true; continue; }
            if (ch == delimiter) { fields.Add(field.ToString()); field.Clear(); continue; }
            if (ch == '\r') continue;
            if (ch == '\n')
            {
                fields.Add(field.ToString());
                field.Clear();
                yield return fields.ToArray();
                fields.Clear();
                any = false;
                continue;
            }
            field.Append(ch);
        }
        if (any)
        {
            fields.Add(field.ToString());
            yield return fields.ToArray();
        }
    }
}
```

- [ ] **Step 6: Rewrite `CatalogTransfer.ImportCsv` with the reader; use labels in exports**

```csharp
public static List<Game> ImportCsv(string path, int startNumber, char delimiter = ';')
{
    var result = new List<Game>();
    using var reader = new StreamReader(path, Encoding.UTF8);
    int n = startNumber;
    foreach (var p in CsvReader.ReadRecords(reader, delimiter).Skip(1))
    {
        if (p.Length < 2 || string.IsNullOrWhiteSpace(p[1])) continue;
        string At(int i) => i < p.Length ? p[i].Trim() : "";
        result.Add(new Game
        {
            Number = n++,
            Title = At(1),
            LocalizedTitle = At(2),
            Developer = At(3),
            Publisher = At(4),
            ReleaseYear = int.TryParse(At(5), out var y) ? y : 0,
            Genre = At(6),
            Platform = At(7).Length > 0 ? At(7) : "PC",
            DateAdded = DateTime.Today,
        });
    }
    return result;
}
```
In `ExportCsv` and `ExportHtml` replace `GameCatalogModel.StatusLabel(...)` with `CatalogLabels.Current.Status(...)` and the inline rating formatting with `GameFieldValues.Rating(g.UserRating)` / `GameFieldValues.Rating(g.CriticRating)`. Translate comments to English. (HTML headers stay Polish until Task 15 moves them to translations.)

- [ ] **Step 7: Fix `AmcXmlImporter` to read attributes (child elements as fallback)**

```csharp
using System.Globalization;
using System.Xml.Linq;
using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;

namespace GameCatalog.Storage;

/// <summary>
/// Imports a legacy Ant Movie Catalog .xml file, mapping movie fields to game fields:
/// Director→Developer, Producer→Publisher, Writer→Designer, Actors→VoiceActors,
/// Category→Genre, Certification→PEGI, Length (min)→PlaytimeHours (h), etc.
/// AMC stores field values as attributes of &lt;Movie&gt; (movieclass.pas TMovie.SaveToXML).
/// </summary>
public static class AmcXmlImporter
{
    public static List<Game> Import(string xmlPath, int startNumber)
    {
        var doc = XDocument.Load(xmlPath);
        var result = new List<Game>();
        int n = startNumber;
        foreach (var m in doc.Descendants("Movie"))
        {
            string V(string tag) => m.Attribute(tag)?.Value ?? m.Element(tag)?.Value ?? "";
            int Int(string tag, int fallback) =>
                int.TryParse(V(tag), NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) ? i : fallback;
            var g = new Game
            {
                Number = n++,
                Checked = V("Checked").Equals("True", StringComparison.OrdinalIgnoreCase),
                MediaLabel = V("MediaLabel"),
                MediaType = MapMediaType(V("MediaType")),
                Store = V("Source"),
                Borrower = V("Borrower"),
                UserRating = Int("UserRating", -1),
                CriticRating = Int("Rating", -1),
                Title = string.IsNullOrWhiteSpace(V("OriginalTitle")) ? V("TranslatedTitle") : V("OriginalTitle"),
                LocalizedTitle = V("TranslatedTitle"),
                Developer = V("Director"),
                Publisher = V("Producer"),
                Designer = V("Writer"),
                Composer = V("Composer"),
                VoiceActors = V("Actors"),
                Country = V("Country"),
                ReleaseYear = Int("Year", 0),
                PlaytimeHours = Math.Round(Int("Length", 0) / 60.0, 1),
                Genre = V("Category"),
                Pegi = V("Certification"),
                Url = V("URL"),
                Description = V("Description"),
                Comments = V("Comments"),
                InstallPath = V("FilePath"),
                Engine = V("VideoFormat"),
                GameMode = "Single-player",
                AudioLanguages = V("Languages"),
                Subtitles = V("Subtitles"),
                SizeGb = V("Size"),
                DateAdded = DateTime.Today,
                Platform = "PC",
            };
            if (DateTime.TryParse(V("DateWatched"), CultureInfo.InvariantCulture, DateTimeStyles.None, out var dw))
                g.DateFinished = dw;
            result.Add(g);
        }
        return result;
    }

    private static string MapMediaType(string mediaType) =>
        mediaType.Contains("DVD", StringComparison.OrdinalIgnoreCase) ? "Fizyczna" : "Cyfrowa";
}
```

- [ ] **Step 8: Run tests**

Run: `dotnet test tests/GameCatalog.Storage.Tests` → Expected: all PASS.
Run: `dotnet build GameCatalog.slnx` → Expected: `Build succeeded`.

- [ ] **Step 9: Commit**

```bash
git add -A
git commit -m "fix(storage): RFC 4180 CSV import, AMC XML attributes, CatalogData load; add storage tests

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 4: Scripting — shared `GameInfoResult`, English comments, smoke tests

**Files:**
- Create: `src/GameCatalog.Plugins.Abstractions/GameInfoResult.cs`
- Modify: `src/GameCatalog.Scripting/ScriptRunner.cs`, `ScriptApi.cs`, `GameScript.cs`, `ScriptSamples.cs`; `src/GameCatalog.App/Forms/ScriptResultForm.cs`, `MainForm.cs`
- Test: `tests/GameCatalog.Scripting.Tests/ScriptRunnerTests.cs`, `GameScriptTests.cs`

**Interfaces:**
- Consumes: `GameFieldValues` (Task 2).
- Produces: `namespace GameCatalog.Plugins.Abstractions`: `GameInfoResult { bool Success; string Error; List<string> Log; Dictionary<string,string> Fields; Dictionary<string,string> CustomFields; List<GameExtra> Extras; byte[]? Cover; bool HasData }`. `ScriptRunner.RunAsync(string code, Game game, Func<string, IList<string>, int>? picker = null, CancellationToken ct = default) : Task<GameInfoResult>` and the `GameScript` overload. `ScriptRunner.Timeout`.

- [ ] **Step 1: Write failing tests**

`tests/GameCatalog.Scripting.Tests/ScriptRunnerTests.cs`:
```csharp
using GameCatalog.Core.Models;

namespace GameCatalog.Scripting.Tests;

public class ScriptRunnerTests
{
    [Fact]
    public async Task RunAsync_ScriptSetsFields_ReturnsSuccess()
    {
        var game = new Game { Title = "Quake" };
        var result = await ScriptRunner.RunAsync(
            "Api.SetField(\"Developer\", \"id Software\"); Api.AddExtra(\"Scourge of Armagon\", \"Expansion\"); Api.Log(Api.Title);",
            game);

        Assert.True(result.Success, result.Error);
        Assert.Equal("id Software", result.Fields["Developer"]);
        Assert.Equal("Scourge of Armagon", Assert.Single(result.Extras).Title);
        Assert.Contains(result.Log, l => l.EndsWith("Quake"));
        Assert.True(result.HasData);
    }

    [Fact]
    public async Task RunAsync_CompileError_ReportsLine()
    {
        var result = await ScriptRunner.RunAsync("int x = ;", new Game());
        Assert.False(result.Success);
        Assert.Contains("1", result.Error);
    }

    [Fact]
    public async Task RunAsync_RuntimeException_IsReportedNotThrown()
    {
        var result = await ScriptRunner.RunAsync("throw new System.InvalidOperationException(\"boom\");", new Game());
        Assert.False(result.Success);
        Assert.Contains("boom", result.Error);
    }
}
```

`tests/GameCatalog.Scripting.Tests/GameScriptTests.cs`:
```csharp
namespace GameCatalog.Scripting.Tests;

public class GameScriptTests
{
    [Fact]
    public void Load_ParsesHeaderMetadata()
    {
        var path = Path.Combine(Path.GetTempPath(), $"s-{Guid.NewGuid():N}.gcsx");
        File.WriteAllText(path, "// Name: Test\n// Description: Desc\n// Version: 2.1\n// Author: Me\n// Site: https://x\nApi.Log(\"x\");");
        try
        {
            var s = GameScript.Load(path);
            Assert.Equal("Test", s.Name);
            Assert.Equal("Desc", s.Description);
            Assert.Equal("2.1", s.Version);
            Assert.Equal("Me", s.Author);
            Assert.Equal("https://x", s.Site);
            Assert.Equal("Test (2.1)", s.ToString());
        }
        finally { File.Delete(path); }
    }
}
```
The test project references `GameCatalog.Scripting` (which brings Core and Plugins.Abstractions).

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test tests/GameCatalog.Scripting.Tests`
Expected: build FAIL — `HasData` does not exist (`ScriptResult` type).

- [ ] **Step 3: Create `GameInfoResult.cs` in Plugins.Abstractions**

```csharp
using GameCatalog.Core.Models;

namespace GameCatalog.Plugins.Abstractions;

/// <summary>
/// Result of fetching information about a game — produced by .gcsx scripts and by
/// <see cref="IGameInfoProvider"/> plugins, shown to the user in the same results window
/// (AMC getscript_results). Nothing is applied to the game until the user confirms.
/// </summary>
public sealed class GameInfoResult
{
    public bool Success { get; set; }
    public string Error { get; set; } = "";
    public List<string> Log { get; set; } = new();
    /// <summary>Field values keyed by field tag (see GameFields).</summary>
    public Dictionary<string, string> Fields { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    /// <summary>Custom field values keyed by custom field tag.</summary>
    public Dictionary<string, string> CustomFields { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<GameExtra> Extras { get; set; } = new();
    /// <summary>Cover image bytes (PNG/JPEG) or null.</summary>
    public byte[]? Cover { get; set; }

    public bool HasData => Fields.Count > 0 || CustomFields.Count > 0 || Extras.Count > 0 || Cover is not null;

    public static GameInfoResult Failed(string error, List<string>? log = null) =>
        new() { Success = false, Error = error, Log = log ?? new() };
}
```
(`IGameInfoProvider` is defined in Task 14; until then write `<c>IGameInfoProvider</c>` in this comment, and switch it to `<see cref="IGameInfoProvider"/>` in Task 14.)

- [ ] **Step 4: Switch `ScriptRunner` to `GameInfoResult`**

Delete the `ScriptResult` class. In `RunAsync`, `var result = new GameInfoResult { Log = log };` and replace the "no data" check with `if (!result.HasData) log.Add("Script returned no data.");`. Keep all other logic. Translate the strings that are returned to the user into English *keys-free* plain English for now (Task 6 moves user-facing errors to translations through `ScriptRunner.Messages`, see below):

```csharp
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
```
Put `ScriptMessages` in `src/GameCatalog.Scripting/ScriptMessages.cs` and use it in `ScriptRunner`. The timeout message uses `(int)Timeout.TotalSeconds` instead of the hard-coded 60.

- [ ] **Step 5: Update App consumers**

- `ScriptResultForm(Game game, GameInfoResult result)`; `HasAny(result)` → use `result.HasData` at the call site and delete `HasAny`.
- `MainForm.RunScriptAsync`: `GameInfoResult result;` and `if (rf.ShowDialog(this) != DialogResult.OK || !result.HasData) return;`.

- [ ] **Step 6: Translate comments in Scripting to English**

`ScriptApi` summary:
```csharp
/// <summary>
/// API exposed to scripts as the <c>Api</c> object — counterpart of the functions registered
/// in AMC getscript.pas (HTML, fields, extras, picking and messages), with movie fields ported
/// to game fields (tags as in GameFields).
/// </summary>
```
Translate every other comment literally. Log strings written by `ScriptApi` (e.g. `"Błąd Bing: "`) become English (`"Bing error: "`); they are diagnostic log lines, not UI. `AddExtra` `CreatedBy` becomes `"script"`. The bodies of the sample scripts in `ScriptSamples` stay as they are except their header `// Description:` lines, which become English.

- [ ] **Step 7: Run tests**

Run: `dotnet test tests/GameCatalog.Scripting.Tests` → Expected: PASS (first run compiles Roslyn, may take ~20 s).
Run: `dotnet build GameCatalog.slnx` → `Build succeeded`.

- [ ] **Step 8: Commit**

```bash
git add -A
git commit -m "refactor(scripting): share GameInfoResult with plugins, English comments, runner tests

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 5: Localization library

**Files:**
- Create: `src/GameCatalog.Localization/ITranslator.cs`, `LanguageInfo.cs`, `FormKey.cs`, `LanguageFile.cs`, `LanguageManager.cs`
- Test: `tests/GameCatalog.Localization.Tests/LanguageManagerTests.cs`

**Interfaces:**
- Consumes: nothing (no project references).
- Produces (`namespace GameCatalog.Localization`):
  - `ITranslator { string CurrentLanguage {get;} string Get(string key, params object?[] args); bool TryGet(string key, out string value); string? GetFormValue(string form, string key); event EventHandler? LanguageChanged; }`
  - `LanguageInfo(string Code, string Name, string Author, string Version)`
  - `FormKey(string Form, string Key, string DefaultText)`
  - `LanguageManager : ITranslator` — `LanguageManager(string languagesDirectory, string defaultLanguage = "pl", Action<string>? log = null)`, `const string DefaultLanguageCode = "pl"`, `IReadOnlyList<LanguageInfo> Available`, `string DefaultLanguage`, `void SetLanguage(string code)`, `void AddPluginLanguages(string pluginId, string directory)`, `string ExportTemplate(IEnumerable<FormKey> formKeys)`

- [ ] **Step 1: Write failing tests**

`tests/GameCatalog.Localization.Tests/LanguageManagerTests.cs`:
```csharp
using System.Text.Json;

namespace GameCatalog.Localization.Tests;

public sealed class LanguageManagerTests : IDisposable
{
    private readonly string _dir = Directory.CreateTempSubdirectory("gc-lang-").FullName;
    private readonly List<string> _log = new();

    public void Dispose() => Directory.Delete(_dir, recursive: true);

    private void WriteLang(string code, string name, object forms, object strings)
    {
        var json = JsonSerializer.Serialize(new
        {
            language = new { name, code, author = "t", version = "1" },
            forms,
            strings,
        });
        File.WriteAllText(Path.Combine(_dir, code + ".json"), json);
    }

    private LanguageManager CreateDefault()
    {
        WriteLang("pl", "Polski",
            new { MainForm = new Dictionary<string, string> { ["mnuFile.Text"] = "&Plik", ["$this.Text"] = "Katalog" } },
            new Dictionary<string, string> { ["Msg.Hello"] = "Cześć {0}", ["Only.Pl"] = "tylko pl" });
        WriteLang("en", "English",
            new { MainForm = new Dictionary<string, string> { ["mnuFile.Text"] = "&File" } },
            new Dictionary<string, string> { ["Msg.Hello"] = "Hello {0}" });
        return new LanguageManager(_dir, "pl", _log.Add);
    }

    [Fact]
    public void Available_ListsLanguagesFromHeaders()
    {
        var m = CreateDefault();
        Assert.Equal(new[] { "en", "pl" }, m.Available.Select(l => l.Code).OrderBy(c => c));
        Assert.Equal("Polski", m.Available.Single(l => l.Code == "pl").Name);
    }

    [Fact]
    public void Get_UsesCurrentLanguage_FallsBackToDefault_ThenKey()
    {
        var m = CreateDefault();
        m.SetLanguage("en");
        Assert.Equal("Hello Ann", m.Get("Msg.Hello", "Ann"));
        Assert.Equal("tylko pl", m.Get("Only.Pl"));
        Assert.Equal("Missing.Key", m.Get("Missing.Key"));
        Assert.False(m.TryGet("Missing.Key", out _));
    }

    [Fact]
    public void Get_BadFormatString_ReturnsUnformattedTextAndLogs()
    {
        WriteLang("pl", "Polski", new { }, new Dictionary<string, string> { ["Bad"] = "x {1}" });
        var m = new LanguageManager(_dir, "pl", _log.Add);
        Assert.Equal("x {1}", m.Get("Bad", "only-one"));
        Assert.NotEmpty(_log);
    }

    [Fact]
    public void GetFormValue_FallsBackToDefault_NullWhenUnknown()
    {
        var m = CreateDefault();
        m.SetLanguage("en");
        Assert.Equal("&File", m.GetFormValue("MainForm", "mnuFile.Text"));
        Assert.Equal("Katalog", m.GetFormValue("MainForm", "$this.Text"));
        Assert.Null(m.GetFormValue("MainForm", "btnNope.Text"));
        Assert.Null(m.GetFormValue("NoSuchForm", "x.Text"));
    }

    [Fact]
    public void SetLanguage_RaisesEventOnlyOnChange_UnknownCodeFallsBackToDefault()
    {
        var m = CreateDefault();
        int raised = 0;
        m.LanguageChanged += (_, _) => raised++;
        m.SetLanguage("en");
        m.SetLanguage("en");
        m.SetLanguage("xx");
        Assert.Equal(2, raised);
        Assert.Equal("pl", m.CurrentLanguage);
    }

    [Fact]
    public void CorruptFile_IsSkippedAndLogged()
    {
        CreateDefault();
        File.WriteAllText(Path.Combine(_dir, "de.json"), "{ this is not json");
        var m = new LanguageManager(_dir, "pl", _log.Add);
        Assert.DoesNotContain(m.Available, l => l.Code == "de");
        Assert.Contains(_log, l => l.Contains("de.json"));
    }

    [Fact]
    public void MissingDirectory_DoesNotThrow_ReturnsKeys()
    {
        var m = new LanguageManager(Path.Combine(_dir, "nope"), "pl", _log.Add);
        Assert.Empty(m.Available);
        Assert.Equal("Any.Key", m.Get("Any.Key"));
        Assert.Null(m.GetFormValue("F", "k"));
    }

    [Fact]
    public void AddPluginLanguages_MergesStringsUnderPluginPrefix()
    {
        var m = CreateDefault();
        var pluginDir = Directory.CreateDirectory(Path.Combine(_dir, "plugin")).FullName;
        File.WriteAllText(Path.Combine(pluginDir, "pl.json"),
            "{\"language\":{\"code\":\"pl\",\"name\":\"Polski\"},\"strings\":{\"Hello\":\"Witaj z pluginu\"}}");
        m.AddPluginLanguages("sample", pluginDir);
        Assert.Equal("Witaj z pluginu", m.Get("Plugin.sample.Hello"));
    }

    [Fact]
    public void ExportTemplate_ContainsFormKeysAndAllStrings()
    {
        var m = CreateDefault();
        m.SetLanguage("en");
        var json = m.ExportTemplate(new[]
        {
            new FormKey("MainForm", "mnuFile.Text", "&Plik"),
            new FormKey("AboutForm", "lblVersion.Text", "Wersja"),
        });
        using var doc = JsonDocument.Parse(json);
        var forms = doc.RootElement.GetProperty("forms");
        Assert.Equal("&File", forms.GetProperty("MainForm").GetProperty("mnuFile.Text").GetString());
        Assert.Equal("Wersja", forms.GetProperty("AboutForm").GetProperty("lblVersion.Text").GetString());
        var strings = doc.RootElement.GetProperty("strings");
        Assert.Equal("Hello {0}", strings.GetProperty("Msg.Hello").GetString());
        Assert.Equal("tylko pl", strings.GetProperty("Only.Pl").GetString());
        Assert.Equal("en", doc.RootElement.GetProperty("language").GetProperty("code").GetString());
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test tests/GameCatalog.Localization.Tests`
Expected: build FAIL — `LanguageManager` not found.

- [ ] **Step 3: Implement the contracts**

`ITranslator.cs`:
```csharp
namespace GameCatalog.Localization;

/// <summary>Read access to the current language (AMC TAntTranslator counterpart).</summary>
public interface ITranslator
{
    /// <summary>Code of the active language, e.g. "pl".</summary>
    string CurrentLanguage { get; }

    /// <summary>
    /// Text of a key from the "strings" section formatted with <paramref name="args"/>.
    /// Falls back to the default language, then to the key itself — never throws.
    /// </summary>
    string Get(string key, params object?[] args);

    /// <summary>Unformatted text of a key; false (and the key as value) when no loaded language defines it.</summary>
    bool TryGet(string key, out string value);

    /// <summary>
    /// Text for a control property of a form ("forms" section), or null when no language defines it —
    /// the caller then keeps the text set in the Designer.
    /// </summary>
    string? GetFormValue(string form, string key);

    event EventHandler? LanguageChanged;
}
```

`LanguageInfo.cs`:
```csharp
namespace GameCatalog.Localization;

/// <summary>Header of a language file shown in the language selection window.</summary>
public sealed record LanguageInfo(string Code, string Name, string Author, string Version);
```

`FormKey.cs`:
```csharp
namespace GameCatalog.Localization;

/// <summary>One translatable property of a form, collected for the translation template.</summary>
public sealed record FormKey(string Form, string Key, string DefaultText);
```

`LanguageFile.cs`:
```csharp
using System.Text.Json.Serialization;

namespace GameCatalog.Localization;

/// <summary>JSON model of Languages\&lt;code&gt;.json.</summary>
public sealed class LanguageFile
{
    [JsonPropertyName("language")] public Header Language { get; set; } = new();
    [JsonPropertyName("forms")] public Dictionary<string, Dictionary<string, string>> Forms { get; set; } = new(StringComparer.Ordinal);
    [JsonPropertyName("strings")] public Dictionary<string, string> Strings { get; set; } = new(StringComparer.Ordinal);

    public sealed class Header
    {
        [JsonPropertyName("code")] public string Code { get; set; } = "";
        [JsonPropertyName("name")] public string Name { get; set; } = "";
        [JsonPropertyName("author")] public string Author { get; set; } = "";
        [JsonPropertyName("version")] public string Version { get; set; } = "";
    }
}
```

- [ ] **Step 4: Implement `LanguageManager.cs`**

```csharp
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace GameCatalog.Localization;

/// <summary>
/// Loads Languages\*.json files and serves translations with a fallback chain
/// current language → default language → key. Missing or corrupt files never throw;
/// problems are reported through the log callback.
/// </summary>
public sealed class LanguageManager : ITranslator
{
    public const string DefaultLanguageCode = "pl";

    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private readonly Dictionary<string, LanguageFile> _files = new(StringComparer.OrdinalIgnoreCase);
    private readonly Action<string> _log;
    private LanguageFile _current;
    private readonly LanguageFile _default;

    public LanguageManager(string languagesDirectory, string defaultLanguage = DefaultLanguageCode, Action<string>? log = null)
    {
        _log = log ?? (_ => { });
        DefaultLanguage = defaultLanguage;
        CurrentLanguage = defaultLanguage;
        foreach (var file in ReadFiles(languagesDirectory))
            _files[file.Language.Code] = file;
        _default = _files.GetValueOrDefault(defaultLanguage) ?? new LanguageFile();
        _current = _default;
    }

    public string DefaultLanguage { get; }
    public string CurrentLanguage { get; private set; }

    public IReadOnlyList<LanguageInfo> Available =>
        _files.Values
            .Select(f => new LanguageInfo(f.Language.Code, f.Language.Name, f.Language.Author, f.Language.Version))
            .OrderBy(l => l.Name, StringComparer.CurrentCulture)
            .ToList();

    public event EventHandler? LanguageChanged;

    /// <summary>Switches the language; an unknown code selects the default language.</summary>
    public void SetLanguage(string code)
    {
        var target = code is not null && _files.ContainsKey(code) ? code : DefaultLanguage;
        if (string.Equals(target, CurrentLanguage, StringComparison.OrdinalIgnoreCase)) return;
        CurrentLanguage = target;
        _current = _files.GetValueOrDefault(target) ?? _default;
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool TryGet(string key, out string value)
    {
        if (_current.Strings.TryGetValue(key, out var v) || _default.Strings.TryGetValue(key, out v))
        {
            value = v;
            return true;
        }
        value = key;
        return false;
    }

    public string Get(string key, params object?[] args)
    {
        if (!TryGet(key, out var text) || args is not { Length: > 0 }) return text;
        try
        {
            return string.Format(CultureInfo.CurrentCulture, text, args);
        }
        catch (FormatException)
        {
            _log($"Localization: invalid format string for key '{key}' in language '{CurrentLanguage}'.");
            return text;
        }
    }

    public string? GetFormValue(string form, string key)
    {
        if (_current.Forms.TryGetValue(form, out var f) && f.TryGetValue(key, out var v)) return v;
        if (_default.Forms.TryGetValue(form, out f) && f.TryGetValue(key, out v)) return v;
        return null;
    }

    /// <summary>
    /// Merges a plugin's language files: each "strings" key K becomes "Plugin.&lt;pluginId&gt;.K"
    /// in the language with the same code. Languages the host does not know are ignored.
    /// </summary>
    public void AddPluginLanguages(string pluginId, string directory)
    {
        foreach (var file in ReadFiles(directory))
        {
            if (!_files.TryGetValue(file.Language.Code, out var target)) continue;
            foreach (var (k, v) in file.Strings)
                target.Strings[$"Plugin.{pluginId}.{k}"] = v;
        }
    }

    /// <summary>
    /// Builds a translation template for the current language: every collected form key
    /// (current translation or designer text) plus every string known to the default or current language.
    /// </summary>
    public string ExportTemplate(IEnumerable<FormKey> formKeys)
    {
        var template = new LanguageFile
        {
            Language = new LanguageFile.Header
            {
                Code = CurrentLanguage,
                Name = _current.Language.Name,
                Author = _current.Language.Author,
                Version = _current.Language.Version,
            },
        };
        foreach (var fk in formKeys)
        {
            if (!template.Forms.TryGetValue(fk.Form, out var section))
                template.Forms[fk.Form] = section = new Dictionary<string, string>(StringComparer.Ordinal);
            section[fk.Key] = GetFormValue(fk.Form, fk.Key) ?? fk.DefaultText;
        }
        foreach (var key in _default.Strings.Keys.Union(_current.Strings.Keys).OrderBy(k => k, StringComparer.Ordinal))
            template.Strings[key] = _current.Strings.TryGetValue(key, out var v) ? v : _default.Strings[key];
        return JsonSerializer.Serialize(template, WriteOptions);
    }

    private IEnumerable<LanguageFile> ReadFiles(string directory)
    {
        if (!Directory.Exists(directory))
        {
            _log($"Localization: directory '{directory}' not found.");
            yield break;
        }
        foreach (var path in Directory.EnumerateFiles(directory, "*.json"))
        {
            LanguageFile? file = null;
            try
            {
                file = JsonSerializer.Deserialize<LanguageFile>(File.ReadAllText(path), ReadOptions);
            }
            catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
            {
                _log($"Localization: cannot read '{Path.GetFileName(path)}': {ex.Message}");
            }
            if (file is null) continue;
            if (file.Language.Code.Length == 0)
                file.Language.Code = Path.GetFileNameWithoutExtension(path);
            yield return file;
        }
    }
}
```

- [ ] **Step 5: Run tests**

Run: `dotnet test tests/GameCatalog.Localization.Tests` → Expected: all PASS.

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "feat(localization): JSON language files with fallback chain, plugin merge and template export

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 6: Language files, translated core labels, logging and start-up wiring

**Files:**
- Create: `src/GameCatalog.App/Languages/pl.json`, `src/GameCatalog.App/Languages/en.json`
- Create: `src/GameCatalog.App/Services/TranslatorCatalogLabels.cs`, `Services/AppPaths.cs`, `Services/AppServices.cs`, `Diagnostics/AppLog.cs`, `Diagnostics/ErrorReporter.cs`
- Move: `src/GameCatalog.App/PrefsStore.cs` → `src/GameCatalog.App/Services/PrefsStore.cs` (add `Language`)
- Modify: `src/GameCatalog.App/Program.cs`, `GameCatalog.App.csproj`, `Forms/MainForm.cs` (prefs from `AppServices`)
- Delete: `src/GameCatalog.App/PolishCatalogLabels.cs`
- Test: `tests/GameCatalog.App.Tests/TranslatorCatalogLabelsTests.cs`, `LanguageFilesTests.cs`, `AppLogTests.cs`

**Interfaces:**
- Consumes: `LanguageManager`, `ITranslator` (Task 5); `ICatalogLabels`, `CatalogLabels` (Task 2); `ScriptMessages` (Task 4).
- Produces:
  - `GameCatalog.App.Services.TranslatorCatalogLabels(ITranslator) : ICatalogLabels`
  - `AppPaths` (static): `DataDirectory`, `LogsDirectory`, `TemplatesDirectory`, `PluginDataDirectory(string pluginId)`, `LanguagesDirectory`, `PluginsDirectory`
  - `AppLog` (static, namespace `GameCatalog.App.Diagnostics`): `Initialize(string directory)`, `Write(string)`, `Error(Exception, string context)`, `FilePath`
  - `ErrorReporter.Show(Exception ex, IWin32Window? owner = null)`
  - `AppServices` (static): `LanguageManager Languages`, `PrefsStore Prefs`
  - `PrefsStore.Language` (`""` until first-run selection)
  - Key conventions for all later tasks: `Field.<Tag>`, `Status.<Enum>`, `Common.*`, `Msg.*`, `Script.*`, `Theme.<Id>`, `Action.<Key>` + `Action.<Key>.Hint` (actions), `forms.<TypeName>.<ControlName>.<Property>` (forms/frames).

- [ ] **Step 1: Write failing tests**

Add to `tests/GameCatalog.App.Tests/GameCatalog.App.Tests.csproj` so tests read the shipped files:
```xml
<ItemGroup>
  <None Include="..\..\src\GameCatalog.App\Languages\*.json" Link="Languages\%(Filename)%(Extension)" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

`tests/GameCatalog.App.Tests/TranslatorCatalogLabelsTests.cs`:
```csharp
using GameCatalog.App.Services;
using GameCatalog.Core.Models;
using GameCatalog.Localization;

namespace GameCatalog.App.Tests;

public class TranslatorCatalogLabelsTests
{
    private static LanguageManager Languages(string code)
    {
        var m = new LanguageManager(Path.Combine(AppContext.BaseDirectory, "Languages"), "pl");
        m.SetLanguage(code);
        return m;
    }

    [Fact]
    public void Polish_CaptionsMatchPreviousUi()
    {
        var l = new TranslatorCatalogLabels(Languages("pl"));
        Assert.Equal("Wydawca", l.Field("Publisher"));
        Assert.Equal("Ukończona 100%", l.Status(CompletionStatus.Completed100));
        Assert.Equal("(brak)", l.NoValue);
        Assert.Equal("Tak", l.Yes);
    }

    [Fact]
    public void English_Captions()
    {
        var l = new TranslatorCatalogLabels(Languages("en"));
        Assert.Equal("Publisher", l.Field("Publisher"));
        Assert.Equal("Dropped", l.Status(CompletionStatus.Dropped));
        Assert.Equal("(none)", l.NoValue);
    }

    [Fact]
    public void UnknownTag_ReturnsTag()
        => Assert.Equal("MyCustom", new TranslatorCatalogLabels(Languages("en")).Field("MyCustom"));
}
```

`tests/GameCatalog.App.Tests/LanguageFilesTests.cs`:
```csharp
using System.Text.Json;
using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;

namespace GameCatalog.App.Tests;

/// <summary>Guards that every shipped language defines exactly the keys of the default language.</summary>
public class LanguageFilesTests
{
    private static string Dir => Path.Combine(AppContext.BaseDirectory, "Languages");

    private static (HashSet<string> Strings, HashSet<string> Forms) Keys(string code)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(Dir, code + ".json")));
        var strings = doc.RootElement.GetProperty("strings").EnumerateObject().Select(p => p.Name).ToHashSet();
        var forms = doc.RootElement.GetProperty("forms").EnumerateObject()
            .SelectMany(f => f.Value.EnumerateObject().Select(k => f.Name + "/" + k.Name)).ToHashSet();
        return (strings, forms);
    }

    [Fact]
    public void English_HasExactlyTheKeysOfPolish()
    {
        var pl = Keys("pl");
        var en = Keys("en");
        Assert.Empty(pl.Strings.Except(en.Strings));
        Assert.Empty(en.Strings.Except(pl.Strings));
        Assert.Empty(pl.Forms.Except(en.Forms));
        Assert.Empty(en.Forms.Except(pl.Forms));
    }

    [Fact]
    public void Polish_DefinesEveryFieldAndStatus()
    {
        var pl = Keys("pl").Strings;
        foreach (var f in GameFields.All) Assert.Contains(f.LabelKey, pl);
        foreach (var s in Enum.GetValues<CompletionStatus>()) Assert.Contains("Status." + s, pl);
    }
}
```

`tests/GameCatalog.App.Tests/AppLogTests.cs`:
```csharp
using GameCatalog.App.Diagnostics;

namespace GameCatalog.App.Tests;

public class AppLogTests
{
    [Fact]
    public void Write_AppendsTimestampedLines()
    {
        var dir = Directory.CreateTempSubdirectory("gc-log-").FullName;
        AppLog.Initialize(dir);
        AppLog.Write("hello");
        AppLog.Error(new InvalidOperationException("boom"), "test");
        var text = File.ReadAllText(AppLog.FilePath!);
        Assert.Contains("hello", text);
        Assert.Contains("ERROR test", text);
        Assert.Contains("boom", text);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test tests/GameCatalog.App.Tests`
Expected: build FAIL — `TranslatorCatalogLabels`, `AppLog` not found.

- [ ] **Step 3: Create `Languages/pl.json`**

```json
{
  "language": { "code": "pl", "name": "Polski", "author": "GameCatalog", "version": "1.0" },
  "forms": {},
  "strings": {
    "Common.Yes": "Tak",
    "Common.No": "Nie",
    "Common.None": "(brak)",
    "Common.All": "(wszystkie)",
    "Common.Ok": "OK",
    "Common.Cancel": "Anuluj",
    "Common.Error": "Błąd",
    "Common.AppName": "GameCatalog",

    "Field.Number": "Numer",
    "Field.Checked": "Zaznaczona",
    "Field.ColorTag": "Etykieta koloru",
    "Field.MediaLabel": "Etykieta nośnika",
    "Field.MediaType": "Typ nośnika",
    "Field.Store": "Sklep / źródło",
    "Field.DateAdded": "Data dodania",
    "Field.Borrower": "Pożyczona komu",
    "Field.DateFinished": "Data ukończenia",
    "Field.UserRating": "Ocena własna",
    "Field.CriticRating": "Ocena recenzentów",
    "Field.Title": "Tytuł",
    "Field.LocalizedTitle": "Tytuł polski",
    "Field.DisplayTitle": "Tytuł wyświetlany",
    "Field.Developer": "Developer",
    "Field.Publisher": "Wydawca",
    "Field.Designer": "Projektant",
    "Field.Composer": "Kompozytor",
    "Field.VoiceActors": "Aktorzy głosowi",
    "Field.Country": "Kraj",
    "Field.ReleaseYear": "Rok wydania",
    "Field.PlaytimeHours": "Czas gry (h)",
    "Field.Genre": "Gatunek",
    "Field.Pegi": "PEGI",
    "Field.URL": "Strona WWW",
    "Field.Description": "Opis",
    "Field.Comments": "Komentarze",
    "Field.InstallPath": "Ścieżka instalacji",
    "Field.Engine": "Silnik",
    "Field.Version": "Wersja",
    "Field.GameMode": "Tryb gry",
    "Field.Platform": "Platforma",
    "Field.Resolution": "Rozdzielczość",
    "Field.Framerate": "Klatkaż",
    "Field.AudioLanguages": "Dubbing",
    "Field.Subtitles": "Napisy",
    "Field.SizeGb": "Rozmiar (GB)",
    "Field.DiskCount": "Liczba płyt",
    "Field.CompletionStatus": "Status",
    "Field.LastPlayed": "Ostatnio grano",
    "Field.NbExtras": "Liczba dodatków",

    "Status.Backlog": "Do zagrania",
    "Status.Playing": "Ogrywana",
    "Status.Completed": "Ukończona",
    "Status.Completed100": "Ukończona 100%",
    "Status.Paused": "Wstrzymana",
    "Status.Dropped": "Porzucona",

    "Theme.Classic": "Klasyczny",
    "Theme.Crystal": "Kryształowy",
    "Theme.Fresh": "Świeży",
    "Theme.Gnome": "Gnom",
    "Theme.Scrows": "Scrows",
    "Theme.WindowsXP": "Windows XP",

    "Script.CompileErrors": "Błędy kompilacji:\n{0}",
    "Script.CompileErrorLine": "linia {0}: {1}",
    "Script.RuntimeError": "Błąd wykonania: {0}",
    "Script.Timeout": "Przekroczono limit czasu ({0} s).",
    "Script.Error": "Błąd: {0}",
    "Script.NoData": "Skrypt nie zwrócił żadnych danych.",

    "Msg.UnexpectedError": "Wystąpił nieoczekiwany błąd:\n{0}\n\nSzczegóły zapisano w pliku:\n{1}"
  }
}
```

- [ ] **Step 4: Create `Languages/en.json`**

```json
{
  "language": { "code": "en", "name": "English", "author": "GameCatalog", "version": "1.0" },
  "forms": {},
  "strings": {
    "Common.Yes": "Yes",
    "Common.No": "No",
    "Common.None": "(none)",
    "Common.All": "(all)",
    "Common.Ok": "OK",
    "Common.Cancel": "Cancel",
    "Common.Error": "Error",
    "Common.AppName": "GameCatalog",

    "Field.Number": "Number",
    "Field.Checked": "Checked",
    "Field.ColorTag": "Color tag",
    "Field.MediaLabel": "Media label",
    "Field.MediaType": "Media type",
    "Field.Store": "Store / source",
    "Field.DateAdded": "Date added",
    "Field.Borrower": "Borrower",
    "Field.DateFinished": "Date finished",
    "Field.UserRating": "User rating",
    "Field.CriticRating": "Critic rating",
    "Field.Title": "Title",
    "Field.LocalizedTitle": "Localized title",
    "Field.DisplayTitle": "Display title",
    "Field.Developer": "Developer",
    "Field.Publisher": "Publisher",
    "Field.Designer": "Designer",
    "Field.Composer": "Composer",
    "Field.VoiceActors": "Voice actors",
    "Field.Country": "Country",
    "Field.ReleaseYear": "Release year",
    "Field.PlaytimeHours": "Playtime (h)",
    "Field.Genre": "Genre",
    "Field.Pegi": "PEGI",
    "Field.URL": "Web page",
    "Field.Description": "Description",
    "Field.Comments": "Comments",
    "Field.InstallPath": "Install path",
    "Field.Engine": "Engine",
    "Field.Version": "Version",
    "Field.GameMode": "Game mode",
    "Field.Platform": "Platform",
    "Field.Resolution": "Resolution",
    "Field.Framerate": "Framerate",
    "Field.AudioLanguages": "Audio languages",
    "Field.Subtitles": "Subtitles",
    "Field.SizeGb": "Size (GB)",
    "Field.DiskCount": "Disc count",
    "Field.CompletionStatus": "Status",
    "Field.LastPlayed": "Last played",
    "Field.NbExtras": "Extras count",

    "Status.Backlog": "Backlog",
    "Status.Playing": "Playing",
    "Status.Completed": "Completed",
    "Status.Completed100": "Completed 100%",
    "Status.Paused": "Paused",
    "Status.Dropped": "Dropped",

    "Theme.Classic": "Classic",
    "Theme.Crystal": "Crystal",
    "Theme.Fresh": "Fresh",
    "Theme.Gnome": "Gnome",
    "Theme.Scrows": "Scrows",
    "Theme.WindowsXP": "Windows XP",

    "Script.CompileErrors": "Compilation errors:\n{0}",
    "Script.CompileErrorLine": "line {0}: {1}",
    "Script.RuntimeError": "Runtime error: {0}",
    "Script.Timeout": "Time limit exceeded ({0} s).",
    "Script.Error": "Error: {0}",
    "Script.NoData": "Script returned no data.",

    "Msg.UnexpectedError": "An unexpected error occurred:\n{0}\n\nDetails were written to:\n{1}"
  }
}
```

Add to `GameCatalog.App.csproj`:
```xml
<ItemGroup>
  <None Include="Languages\*.json" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

**Rule for all later tasks:** every key added to `pl.json` is added to `en.json` in the same commit (`LanguageFilesTests` enforces it).

- [ ] **Step 5: Implement `Services/TranslatorCatalogLabels.cs`**

```csharp
using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;
using GameCatalog.Localization;

namespace GameCatalog.App.Services;

/// <summary>Core captions served from the language files.</summary>
public sealed class TranslatorCatalogLabels(ITranslator translator) : ICatalogLabels
{
    public string Field(string tag) => translator.TryGet("Field." + tag, out var text) ? text : tag;
    public string Status(CompletionStatus status) => translator.Get("Status." + status);
    public string Yes => translator.Get("Common.Yes");
    public string No => translator.Get("Common.No");
    public string NoValue => translator.Get("Common.None");
}
```

- [ ] **Step 6: Implement `Services/AppPaths.cs`**

```csharp
namespace GameCatalog.App.Services;

/// <summary>Well-known folders of the application.</summary>
public static class AppPaths
{
    /// <summary>User data root: Documents\GameCatalog.</summary>
    public static string DataDirectory =>
        Ensure(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GameCatalog"));

    public static string LogsDirectory => Ensure(Path.Combine(DataDirectory, "Logs"));
    public static string TemplatesDirectory => Ensure(Path.Combine(DataDirectory, "Templates"));
    public static string PluginDataDirectory(string pluginId) => Ensure(Path.Combine(DataDirectory, "PluginData", pluginId));

    /// <summary>Shipped next to the executable.</summary>
    public static string LanguagesDirectory => Path.Combine(AppContext.BaseDirectory, "Languages");
    public static string PluginsDirectory => Path.Combine(AppContext.BaseDirectory, "Plugins");

    private static string Ensure(string dir)
    {
        Directory.CreateDirectory(dir);
        return dir;
    }
}
```

- [ ] **Step 7: Implement `Diagnostics/AppLog.cs` and `Diagnostics/ErrorReporter.cs`**

```csharp
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
```

```csharp
using GameCatalog.App.Services;

namespace GameCatalog.App.Diagnostics;

/// <summary>Logs an unexpected exception and tells the user where the details are.</summary>
public static class ErrorReporter
{
    public static void Show(Exception ex, IWin32Window? owner = null)
    {
        AppLog.Error(ex, "Unhandled UI exception");
        var t = AppServices.Languages;
        MessageBox.Show(owner,
            t.Get("Msg.UnexpectedError", ex.Message, AppLog.FilePath ?? ""),
            t.Get("Common.AppName"), MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
```

- [ ] **Step 8: `AppServices` and `PrefsStore` move**

Move `PrefsStore.cs` to `Services/PrefsStore.cs` (namespace `GameCatalog.App.Services`, comments in English), add:
```csharp
/// <summary>Language code chosen by the user; empty until the first-run language selection.</summary>
public string Language { get; set; } = "";
```
and make `PrefsPath()` return `Path.Combine(AppPaths.DataDirectory, "prefs.json")`. `SearchSite` record moves with it.

`Services/AppServices.cs`:
```csharp
using GameCatalog.Localization;

namespace GameCatalog.App.Services;

/// <summary>
/// Process-wide services created in Program.Main. Designer-built forms need parameterless
/// constructors, so they read shared services from here.
/// </summary>
public static class AppServices
{
    public static LanguageManager Languages { get; set; } = null!;
    public static PrefsStore Prefs { get; set; } = null!;
}
```
In `MainForm` replace `_prefs = PrefsStore.Load();` with `_prefs = AppServices.Prefs;`.

- [ ] **Step 9: Rewrite `Program.cs`**

```csharp
using GameCatalog.App.Diagnostics;
using GameCatalog.App.Services;
using GameCatalog.Core.Fields;
using GameCatalog.Localization;
using GameCatalog.Scripting;

namespace GameCatalog.App;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        AppLog.Initialize(AppPaths.LogsDirectory);
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => ErrorReporter.Show(e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            AppLog.Error((Exception)e.ExceptionObject, "Unhandled domain exception");
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            AppLog.Error(e.Exception, "Unobserved task exception");
            e.SetObserved();
        };

        AppServices.Prefs = PrefsStore.Load();
        AppServices.Languages = new LanguageManager(AppPaths.LanguagesDirectory, LanguageManager.DefaultLanguageCode, AppLog.Write);
        AppServices.Languages.SetLanguage(AppServices.Prefs.Language);
        WireTranslations(AppServices.Languages);

        Application.Run(new MainForm());
    }

    /// <summary>Points the UI-free layers at the active language.</summary>
    private static void WireTranslations(ITranslator t)
    {
        CatalogLabels.Current = new TranslatorCatalogLabels(t);
        ScriptMessages.CompileErrors = details => t.Get("Script.CompileErrors", details);
        ScriptMessages.CompileErrorLine = (line, msg) => t.Get("Script.CompileErrorLine", line, msg);
        ScriptMessages.RuntimeError = msg => t.Get("Script.RuntimeError", msg);
        ScriptMessages.Timeout = seconds => t.Get("Script.Timeout", seconds);
        ScriptMessages.Error = msg => t.Get("Script.Error", msg);
        ScriptMessages.NoData = t.Get("Script.NoData");
        t.LanguageChanged += (_, _) => ScriptMessages.NoData = t.Get("Script.NoData");
    }
}
```
Delete `PolishCatalogLabels.cs`.

- [ ] **Step 10: Run tests, build, run**

Run: `dotnet test GameCatalog.slnx` → Expected: all PASS.
Run the app: captions unchanged (Polish). Set `"Language": "en"` in `Documents\GameCatalog\prefs.json` and run again: status column and group captions are English, menus still Polish (menus move to translations in Task 11). Restore `"Language": ""`.

- [ ] **Step 11: Commit**

```bash
git add -A
git commit -m "feat(app): language files, translated core labels, logging and global error handling

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 7: UI components — `UIAction`, `ActionList`, `Localizer`, `IconThemeImageList`

**Files:**
- Create: `src/GameCatalog.UI/Icons/AmcIcon.cs` (enum moved from `src/GameCatalog.App/AmcIcons.cs`)
- Create: `src/GameCatalog.UI/Components/UIAction.cs`, `ActionList.cs`, `Localizer.cs`, `IconThemeImageList.cs`
- Delete: `src/GameCatalog.App/AmcIcons.cs`
- Modify: `src/GameCatalog.App/Forms/MainForm.cs`, `Forms/PrefsForm.cs`, `Services/PrefsStore.cs`, `Program.cs`
- Test: `tests/GameCatalog.UI.Tests/AssemblyInfo.cs`, `FakeTranslator.cs`, `ActionListTests.cs`, `LocalizerTests.cs`, `IconThemeImageListTests.cs`

**Interfaces:**
- Consumes: `ITranslator`, `FormKey` (Task 5).
- Produces (`GameCatalog.UI.Icons`, `GameCatalog.UI.Components`):
  - `enum AmcIcon { None = -1, FileNew, … }` — original order, `None` added first
  - `UIAction : Component` — `Key`, `Text`, `ToolTipText`, `Icon`, `ShortcutKeys`, `Enabled`, `Checked`, `Visible`; events `Execute`, `Update`, `Changed`; `PerformExecute()`, `PerformUpdate()`, `ApplyTranslation(ITranslator)`
  - `ActionList : Component, IExtenderProvider` — extender `Action` on `ToolStripItem` (`GetAction`/`SetAction`), `AutoUpdate`, `UpdateActions()`, `Actions`, static `FindAction(ToolStripItem)`
  - `Localizer : Component` — static `Translator`, `ContainerControl`, `FormName`, `Apply()`, `CollectKeys()`
  - `IconThemeImageList : Component, IExtenderProvider` — extender `UseThemeIcons` on `ToolStrip`, `Theme`, `ImageList`, `ResourceDirectory`, `ThemeChanged`, `Refresh()`; static `Themes` (`IconTheme(Id, FileName, NameKey)`), `DefaultThemeId = "WindowsXP"`, `NormalizeThemeId(string?)`, `LoadStrip(string themeId, string resourceDirectory)`

- [ ] **Step 1: Write failing tests**

`tests/GameCatalog.UI.Tests/AssemblyInfo.cs`:
```csharp
// Localizer.Translator is static; run UI tests sequentially.
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]
```

`tests/GameCatalog.UI.Tests/FakeTranslator.cs`:
```csharp
using GameCatalog.Localization;

namespace GameCatalog.UI.Tests;

internal sealed class FakeTranslator : ITranslator
{
    public Dictionary<string, string> Strings { get; } = new();
    public Dictionary<(string Form, string Key), string> Forms { get; } = new();
    public string CurrentLanguage { get; private set; } = "pl";
    public event EventHandler? LanguageChanged;

    public bool TryGet(string key, out string value)
    {
        if (Strings.TryGetValue(key, out var v)) { value = v; return true; }
        value = key;
        return false;
    }

    public string Get(string key, params object?[] args) => TryGet(key, out var v) ? string.Format(v, args) : key;
    public string? GetFormValue(string form, string key) => Forms.TryGetValue((form, key), out var v) ? v : null;

    public void Switch(string code)
    {
        CurrentLanguage = code;
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }
}
```

`tests/GameCatalog.UI.Tests/ActionListTests.cs`:
```csharp
using System.ComponentModel;
using GameCatalog.UI.Components;
using GameCatalog.UI.Icons;

namespace GameCatalog.UI.Tests;

public class ActionListTests
{
    [Fact]
    public void SetAction_CopiesPropertiesToMenuItemAndButton()
    {
        using var container = new Container();
        var list = new ActionList(container);
        var action = new UIAction(container) { Text = "&Save", Icon = AmcIcon.FileSave, ShortcutKeys = Keys.Control | Keys.S };
        var menu = new ToolStripMenuItem();
        var button = new ToolStripButton();

        list.SetAction(menu, action);
        list.SetAction(button, action);

        Assert.Equal("&Save", menu.Text);
        Assert.Equal(Keys.Control | Keys.S, menu.ShortcutKeys);
        Assert.Equal((int)AmcIcon.FileSave, menu.ImageIndex);
        Assert.Equal("Save", button.ToolTipText);
        Assert.Same(action, ActionList.FindAction(button));
    }

    [Fact]
    public void ActionChanges_PropagateToBoundItems()
    {
        using var container = new Container();
        var list = new ActionList(container);
        var action = new UIAction(container) { Text = "A" };
        var menu = new ToolStripMenuItem();
        list.SetAction(menu, action);

        action.Enabled = false;
        action.Checked = true;
        action.Text = "B";

        Assert.False(menu.Enabled);
        Assert.True(menu.Checked);
        Assert.Equal("B", menu.Text);
    }

    [Fact]
    public void ItemClick_ExecutesAction_OnlyWhenEnabled()
    {
        using var container = new Container();
        var list = new ActionList(container);
        var action = new UIAction(container);
        int executed = 0;
        action.Execute += (_, _) => executed++;
        var menu = new ToolStripMenuItem();
        list.SetAction(menu, action);

        menu.PerformClick();
        action.Enabled = false;
        action.PerformExecute();

        Assert.Equal(1, executed);
    }

    [Fact]
    public void UpdateActions_RaisesUpdateOncePerAction()
    {
        using var container = new Container();
        var list = new ActionList(container) { AutoUpdate = false };
        var action = new UIAction(container);
        int updates = 0;
        action.Update += (_, _) => updates++;
        list.SetAction(new ToolStripMenuItem(), action);
        list.SetAction(new ToolStripButton(), action);

        list.UpdateActions();

        Assert.Equal(1, updates);
    }

    [Fact]
    public void SetAction_Null_Unbinds()
    {
        using var container = new Container();
        var list = new ActionList(container);
        var action = new UIAction(container) { Text = "A" };
        var menu = new ToolStripMenuItem();
        list.SetAction(menu, action);
        list.SetAction(menu, null);
        action.Text = "B";
        Assert.Equal("A", menu.Text);
        Assert.Null(ActionList.FindAction(menu));
    }
}
```

`tests/GameCatalog.UI.Tests/LocalizerTests.cs`:
```csharp
using System.ComponentModel;
using GameCatalog.UI.Components;

namespace GameCatalog.UI.Tests;

public class LocalizerTests
{
    private sealed class SampleForm : Form
    {
        public readonly IContainer Parts = new Container();
        public readonly Label lblHello = new() { Name = "lblHello", Text = "Cześć" };
        public readonly Button btnOk = new() { Name = "btnOk", Text = "OK" };
        public readonly TextBox txtData = new() { Name = "txtData", Text = "user data" };
        public readonly MenuStrip menu = new() { Name = "menu" };
        public readonly ToolStripMenuItem mnuFile = new() { Name = "mnuFile", Text = "&Plik" };
        public readonly ToolStripMenuItem mnuFileSave = new() { Name = "mnuFileSave", Text = "zapisz-design" };
        public readonly ListView list = new() { Name = "list" };
        public readonly ColumnHeader colTitle = new() { Name = "colTitle", Text = "Tytuł" };
        public readonly UIAction actSave;
        public readonly ActionList actions;
        public readonly Localizer localizer;

        public SampleForm()
        {
            Text = "Formularz";
            mnuFile.DropDownItems.Add(mnuFileSave);
            menu.Items.Add(mnuFile);
            list.Columns.Add(colTitle);
            Controls.AddRange(new Control[] { lblHello, btnOk, txtData, menu, list });
            actSave = new UIAction(Parts) { Key = "FileSave", Text = "Zapisz" };
            actions = new ActionList(Parts) { AutoUpdate = false };
            actions.SetAction(mnuFileSave, actSave);
            localizer = new Localizer(Parts) { ContainerControl = this };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) Parts.Dispose();
            base.Dispose(disposing);
        }
    }

    private static FakeTranslator English()
    {
        var t = new FakeTranslator();
        t.Forms[("SampleForm", "$this.Text")] = "Form";
        t.Forms[("SampleForm", "lblHello.Text")] = "Hello";
        t.Forms[("SampleForm", "mnuFile.Text")] = "&File";
        t.Forms[("SampleForm", "colTitle.Text")] = "Title";
        t.Forms[("SampleForm", "mnuFileSave.Text")] = "MUST NOT BE USED";
        t.Strings["Action.FileSave"] = "Save";
        t.Strings["Action.FileSave.Hint"] = "Save the catalog";
        return t;
    }

    [Fact]
    public void Apply_TranslatesCaptionsAndActions_KeepsUserData()
    {
        Localizer.Translator = English();
        using var f = new SampleForm();
        f.localizer.Apply();

        Assert.Equal("Form", f.Text);
        Assert.Equal("Hello", f.lblHello.Text);
        Assert.Equal("OK", f.btnOk.Text);                 // no translation → designer text
        Assert.Equal("user data", f.txtData.Text);         // editors are never touched
        Assert.Equal("&File", f.mnuFile.Text);
        Assert.Equal("Title", f.colTitle.Text);
        Assert.Equal("Save", f.mnuFileSave.Text);          // bound item follows its action
        Assert.Equal("Save the catalog", f.actSave.ToolTipText);
    }

    [Fact]
    public void LanguageSwitch_RestoresDesignerTextsWhenKeyMissing()
    {
        var t = English();
        Localizer.Translator = t;
        using var f = new SampleForm();
        f.localizer.Apply();

        t.Forms.Clear();
        t.Strings.Clear();
        t.Switch("pl");

        Assert.Equal("Cześć", f.lblHello.Text);
        Assert.Equal("Formularz", f.Text);
        Assert.Equal("Zapisz", f.mnuFileSave.Text);
    }

    [Fact]
    public void CollectKeys_ListsTranslatableProperties()
    {
        Localizer.Translator = new FakeTranslator();
        using var f = new SampleForm();
        var keys = f.localizer.CollectKeys().Select(k => k.Key).ToList();

        Assert.Contains("$this.Text", keys);
        Assert.Contains("lblHello.Text", keys);
        Assert.Contains("btnOk.Text", keys);
        Assert.Contains("mnuFile.Text", keys);
        Assert.Contains("colTitle.Text", keys);
        Assert.DoesNotContain("txtData.Text", keys);
        Assert.DoesNotContain("mnuFileSave.Text", keys);
        Assert.All(f.localizer.CollectKeys(), k => Assert.Equal("SampleForm", k.Form));
    }
}
```

`tests/GameCatalog.UI.Tests/IconThemeImageListTests.cs`:
```csharp
using GameCatalog.UI.Components;

namespace GameCatalog.UI.Tests;

public class IconThemeImageListTests
{
    [Theory]
    [InlineData("Klasyczny", "Classic")]
    [InlineData("Kryształowy", "Crystal")]
    [InlineData("Świeży", "Fresh")]
    [InlineData("Gnom", "Gnome")]
    [InlineData("Scrows", "Scrows")]
    [InlineData("Windows XP", "WindowsXP")]
    [InlineData("WindowsXP", "WindowsXP")]
    [InlineData("crystal", "Crystal")]
    [InlineData("", "WindowsXP")]
    [InlineData(null, "WindowsXP")]
    [InlineData("Unknown", "WindowsXP")]
    public void NormalizeThemeId_MapsLegacyPolishNames(string? stored, string expected)
        => Assert.Equal(expected, IconThemeImageList.NormalizeThemeId(stored));

    [Fact]
    public void LoadStrip_CutsBitmapInto16x16Icons()
    {
        var dir = Directory.CreateTempSubdirectory("gc-icons-").FullName;
        using (var bmp = new Bitmap(48, 16)) bmp.Save(Path.Combine(dir, "Classic.bmp"));
        using var images = IconThemeImageList.LoadStrip("Classic", dir);
        Assert.Equal(3, images.Images.Count);
    }

    [Fact]
    public void LoadStrip_MissingFile_ReturnsEmptyList()
    {
        using var images = IconThemeImageList.LoadStrip("Classic", Path.Combine(Path.GetTempPath(), "nope-" + Guid.NewGuid()));
        Assert.Empty(images.Images);
    }

    [Fact]
    public void UseThemeIcons_AssignsImageListToStripAndDropDowns()
    {
        using var themes = new IconThemeImageList();
        var strip = new MenuStrip();
        var sub = new ToolStripMenuItem("x");
        sub.DropDownItems.Add("y");
        strip.Items.Add(sub);

        themes.SetUseThemeIcons(strip, true);

        Assert.Same(themes.ImageList, strip.ImageList);
        Assert.Same(themes.ImageList, sub.DropDown.ImageList);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test tests/GameCatalog.UI.Tests`
Expected: build FAIL — components do not exist.

- [ ] **Step 3: Move the icon enum**

Create `src/GameCatalog.UI/Icons/AmcIcon.cs` in namespace `GameCatalog.UI.Icons` with the enum body copied verbatim from `src/GameCatalog.App/AmcIcons.cs`, `None = -1,` inserted as the first member, and:
```csharp
/// <summary>
/// Original Ant Movie Catalog 16x16 toolbar icons. Order is identical to TIconIndex in
/// ConstValues.pas — the numeric value is the index in the icon strip bitmap.
/// <see cref="None"/> means "no icon".
/// </summary>
```
Delete `src/GameCatalog.App/AmcIcons.cs`. In App:
- `AmcIcons.LoadStrip(theme)` → `IconThemeImageList.LoadStrip(theme, Path.Combine(AppContext.BaseDirectory, "Resources"))`.
- `PrefsForm` theme combo: items are `IconThemeImageList.Themes`; display text `AppServices.Languages.Get(theme.NameKey)`; the form's `Theme` property returns the selected `theme.Id`.
- `PrefsStore`: `Theme` defaults to `IconThemeImageList.DefaultThemeId`; in `Load()` after deserialization: `prefs.Theme = IconThemeImageList.NormalizeThemeId(prefs.Theme);`.
- Add `using GameCatalog.UI.Icons; using GameCatalog.UI.Components;` where needed.
- `Program.Main`, after `WireTranslations(...)`: `GameCatalog.UI.Components.Localizer.Translator = AppServices.Languages;`

- [ ] **Step 4: Implement `UIAction.cs`**

```csharp
using System.ComponentModel;
using GameCatalog.Localization;
using GameCatalog.UI.Icons;

namespace GameCatalog.UI.Components;

/// <summary>
/// A user command shared by menu items, toolbar buttons and context menus — counterpart of
/// Delphi TAction. Bind it to items with <see cref="ActionList"/>; bound items mirror its
/// text, icon, shortcut and state.
/// </summary>
[ToolboxItem(true), DefaultEvent(nameof(Execute)), DefaultProperty(nameof(Text))]
public sealed class UIAction : Component
{
    private string _text = "", _toolTipText = "", _key = "";
    private string? _designText, _designToolTip;
    private AmcIcon _icon = AmcIcon.None;
    private Keys _shortcutKeys;
    private bool _enabled = true, _checked, _visible = true;

    public UIAction() { }

    public UIAction(IContainer container) : this() => container.Add(this);

    /// <summary>Translation key: texts come from strings "Action.&lt;Key&gt;" and "Action.&lt;Key&gt;.Hint".</summary>
    [Category("Localization"), DefaultValue("")]
    public string Key { get => _key; set => _key = value ?? ""; }

    [Category("Appearance"), DefaultValue("")]
    public string Text { get => _text; set => Set(ref _text, value ?? ""); }

    /// <summary>Hint shown on toolbar buttons; when empty the text without the mnemonic is used.</summary>
    [Category("Appearance"), DefaultValue("")]
    public string ToolTipText { get => _toolTipText; set => Set(ref _toolTipText, value ?? ""); }

    [Category("Appearance"), DefaultValue(AmcIcon.None)]
    public AmcIcon Icon { get => _icon; set => Set(ref _icon, value); }

    [Category("Behavior"), DefaultValue(Keys.None)]
    public Keys ShortcutKeys { get => _shortcutKeys; set => Set(ref _shortcutKeys, value); }

    [Category("Behavior"), DefaultValue(true)]
    public bool Enabled { get => _enabled; set => Set(ref _enabled, value); }

    [Category("Behavior"), DefaultValue(false)]
    public bool Checked { get => _checked; set => Set(ref _checked, value); }

    [Category("Behavior"), DefaultValue(true)]
    public bool Visible { get => _visible; set => Set(ref _visible, value); }

    /// <summary>Raised when the user invokes the action and it is enabled.</summary>
    [Category("Action")]
    public event EventHandler? Execute;

    /// <summary>Raised on idle so the handler can refresh Enabled/Checked (Delphi OnUpdate).</summary>
    [Category("Action")]
    public event EventHandler? Update;

    /// <summary>Raised whenever a visual property changes.</summary>
    [Browsable(false)]
    public event EventHandler? Changed;

    public void PerformExecute()
    {
        if (_enabled) Execute?.Invoke(this, EventArgs.Empty);
    }

    public void PerformUpdate() => Update?.Invoke(this, EventArgs.Empty);

    /// <summary>Applies texts of the current language; designer texts are the fallback.</summary>
    public void ApplyTranslation(ITranslator translator)
    {
        if (_key.Length == 0) return;
        _designText ??= _text;
        _designToolTip ??= _toolTipText;
        Text = translator.TryGet("Action." + _key, out var text) ? text : _designText;
        ToolTipText = translator.TryGet("Action." + _key + ".Hint", out var hint) ? hint : _designToolTip;
    }

    private void Set<T>(ref T field, T value)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
```

- [ ] **Step 5: Implement `ActionList.cs`**

```csharp
using System.ComponentModel;
using System.Runtime.CompilerServices;
using GameCatalog.UI.Icons;

namespace GameCatalog.UI.Components;

/// <summary>
/// Binds <see cref="UIAction"/>s to tool strip items (adds the "Action" property to menu items
/// and toolbar buttons in the Designer) and refreshes action state on idle — Delphi TActionList.
/// </summary>
[ToolboxItem(true), ProvideProperty("Action", typeof(ToolStripItem))]
public sealed class ActionList : Component, IExtenderProvider
{
    private static readonly ConditionalWeakTable<ToolStripItem, UIAction> Bound = new();
    private readonly Dictionary<ToolStripItem, UIAction> _items = new();
    private bool _autoUpdate = true, _idleHooked;

    public ActionList() { }

    public ActionList(IContainer container) : this() => container.Add(this);

    /// <summary>When true, every action's Update event is raised on Application.Idle.</summary>
    [DefaultValue(true)]
    public bool AutoUpdate
    {
        get => _autoUpdate;
        set { _autoUpdate = value; HookIdle(); }
    }

    /// <summary>Distinct actions bound to at least one item.</summary>
    [Browsable(false)]
    public IEnumerable<UIAction> Actions => _items.Values.Distinct();

    public bool CanExtend(object extendee) =>
        extendee is ToolStripMenuItem or ToolStripButton or ToolStripSplitButton or ToolStripDropDownButton;

    [DefaultValue(null), Category("Behavior"), Description("Action that drives this item.")]
    public UIAction? GetAction(ToolStripItem item) => _items.GetValueOrDefault(item);

    public void SetAction(ToolStripItem item, UIAction? action)
    {
        if (_items.Remove(item, out var old))
        {
            old.Changed -= OnActionChanged;
            item.Click -= OnItemClick;
            Bound.Remove(item);
        }
        if (action is null)
        {
            HookIdle();
            return;
        }
        _items[item] = action;
        Bound.AddOrUpdate(item, action);
        action.Changed += OnActionChanged;
        item.Click += OnItemClick;
        Sync(item, action);
        HookIdle();
    }

    /// <summary>Action bound to an item by any ActionList, or null.</summary>
    public static UIAction? FindAction(ToolStripItem item) => Bound.TryGetValue(item, out var a) ? a : null;

    public void UpdateActions()
    {
        foreach (var action in Actions.ToList()) action.PerformUpdate();
    }

    private void OnItemClick(object? sender, EventArgs e)
    {
        if (sender is ToolStripItem item && _items.TryGetValue(item, out var action)) action.PerformExecute();
    }

    private void OnActionChanged(object? sender, EventArgs e)
    {
        foreach (var (item, action) in _items)
            if (ReferenceEquals(action, sender)) Sync(item, action);
    }

    private static string WithoutMnemonic(string text) =>
        text.Replace("&&", "\0").Replace("&", "").Replace("\0", "&");

    private static void Sync(ToolStripItem item, UIAction a)
    {
        item.Text = a.Text;
        item.ToolTipText = a.ToolTipText.Length > 0 ? a.ToolTipText : WithoutMnemonic(a.Text);
        if (a.Icon != AmcIcon.None) item.ImageIndex = (int)a.Icon;
        item.Enabled = a.Enabled;
        item.Visible = a.Visible;
        switch (item)
        {
            case ToolStripMenuItem m:
                m.ShortcutKeys = a.ShortcutKeys;
                m.Checked = a.Checked;
                break;
            case ToolStripButton b:
                b.Checked = a.Checked;
                break;
        }
    }

    private void HookIdle()
    {
        bool want = _autoUpdate && _items.Count > 0 && !(Site?.DesignMode ?? false);
        if (want == _idleHooked) return;
        if (want) Application.Idle += OnIdle; else Application.Idle -= OnIdle;
        _idleHooked = want;
    }

    private void OnIdle(object? sender, EventArgs e) => UpdateActions();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var item in _items.Keys.ToList()) SetAction(item, null);
            if (_idleHooked) Application.Idle -= OnIdle;
            _idleHooked = false;
        }
        base.Dispose(disposing);
    }
}
```

- [ ] **Step 6: Implement `Localizer.cs`**

```csharp
using System.ComponentModel;
using System.ComponentModel.Design;
using GameCatalog.Localization;

namespace GameCatalog.UI.Components;

/// <summary>
/// Translates a form or user control from the language files — counterpart of AMC
/// TAntTranslator.Translate(Form). Keys are "&lt;ControlName&gt;.&lt;Property&gt;" inside the section
/// named after the container type ("$this.Text" is the form caption). Designer texts are the
/// fallback. Nested user controls translate themselves with their own Localizer; tool strip
/// items bound to a <see cref="UIAction"/> take their texts from the action.
/// </summary>
[ToolboxItem(true)]
public sealed class Localizer : Component
{
    private readonly IContainer? _components;
    private readonly Dictionary<string, string> _designTexts = new(StringComparer.Ordinal);
    private ContainerControl? _container;
    private bool _subscribed;

    /// <summary>Active translator; set once by the application before the first form is created.</summary>
    public static ITranslator? Translator { get; set; }

    public Localizer() { }

    public Localizer(IContainer container) : this()
    {
        container.Add(this);
        _components = container;
    }

    /// <summary>The translated form or user control (filled in automatically by the Designer).</summary>
    [DefaultValue(null)]
    public ContainerControl? ContainerControl
    {
        get => _container;
        set
        {
            if (_container is not null) _container.HandleCreated -= OnHandleCreated;
            _container = value;
            if (_container is null || (Site?.DesignMode ?? false)) return;
            _container.HandleCreated += OnHandleCreated;
            Subscribe();
        }
    }

    /// <summary>Section name in the language file; empty = container type name.</summary>
    [DefaultValue("")]
    public string FormName { get; set; } = "";

    public override ISite? Site
    {
        get => base.Site;
        set
        {
            base.Site = value;
            // Like ErrorProvider: default the container to the root component being designed.
            if (value?.GetService(typeof(IDesignerHost)) is IDesignerHost { RootComponent: ContainerControl root })
                _container = root;
        }
    }

    private string Section => FormName.Length > 0 ? FormName : _container?.GetType().Name ?? "";

    /// <summary>Applies the current language to the container now.</summary>
    public void Apply()
    {
        if (_container is null) return;
        var t = Translator;
        foreach (var (key, get, set) in Targets())
        {
            if (!_designTexts.ContainsKey(key)) _designTexts[key] = get();
            set(t?.GetFormValue(Section, key) ?? _designTexts[key]);
        }
        if (t is not null && _components is not null)
            foreach (var action in _components.Components.OfType<UIAction>())
                action.ApplyTranslation(t);
    }

    /// <summary>Every translatable property with its designer text (for the translation template).</summary>
    public IReadOnlyList<FormKey> CollectKeys() =>
        Targets().Select(t => new FormKey(Section, t.Key, _designTexts.GetValueOrDefault(t.Key) ?? t.Get())).ToList();

    private IEnumerable<(string Key, Func<string> Get, Action<string> Set)> Targets()
    {
        if (_container is null) yield break;
        var root = _container;
        if (root is Form) yield return ("$this.Text", () => root.Text, v => root.Text = v);
        foreach (var target in ControlTargets(root)) yield return target;
        if (_components is null) yield break;
        foreach (var strip in _components.Components.OfType<ToolStrip>())   // context menus live in the tray
            foreach (var t in ItemTargets(strip.Items)) yield return t;
        foreach (var tip in _components.Components.OfType<ToolTip>())
            foreach (var c in Descendants(root))
            {
                var control = c;
                if (control.Name.Length > 0 && tip.GetToolTip(control).Length > 0)
                    yield return (control.Name + ".ToolTip", () => tip.GetToolTip(control), v => tip.SetToolTip(control, v));
            }
    }

    private static IEnumerable<Control> Descendants(Control parent)
    {
        foreach (Control c in parent.Controls)
        {
            yield return c;
            if (c is UserControl) continue;
            foreach (var d in Descendants(c)) yield return d;
        }
    }

    private static IEnumerable<(string, Func<string>, Action<string>)> ControlTargets(Control parent)
    {
        foreach (Control c in parent.Controls)
        {
            var control = c;
            if (control.Name.Length > 0 && control is ButtonBase or Label or GroupBox or TabPage)
                yield return (control.Name + ".Text", () => control.Text, v => control.Text = v);
            switch (control)
            {
                case ToolStrip strip:
                    foreach (var t in ItemTargets(strip.Items)) yield return t;
                    break;
                case ListView lv:
                    foreach (ColumnHeader col in lv.Columns)
                    {
                        var column = col;
                        if (!string.IsNullOrEmpty(column.Name))
                            yield return (column.Name + ".Text", () => column.Text, v => column.Text = v);
                    }
                    break;
                case DataGridView grid:
                    foreach (DataGridViewColumn col in grid.Columns)
                    {
                        var column = col;
                        if (column.Name.Length > 0)
                            yield return (column.Name + ".HeaderText", () => column.HeaderText, v => column.HeaderText = v);
                    }
                    break;
            }
            if (control is UserControl) continue;   // frames localize themselves
            foreach (var t in ControlTargets(control)) yield return t;
        }
    }

    private static IEnumerable<(string, Func<string>, Action<string>)> ItemTargets(ToolStripItemCollection items)
    {
        foreach (ToolStripItem i in items)
        {
            var item = i;
            bool translatable = item.Name.Length > 0 && ActionList.FindAction(item) is null &&
                                item is not (ToolStripTextBox or ToolStripComboBox or ToolStripSeparator);
            if (translatable)
            {
                yield return (item.Name + ".Text", () => item.Text ?? "", v => item.Text = v);
                if (!string.IsNullOrEmpty(item.ToolTipText) && item.ToolTipText != item.Text)
                    yield return (item.Name + ".ToolTipText", () => item.ToolTipText ?? "", v => item.ToolTipText = v);
            }
            if (item is ToolStripDropDownItem dd)
                foreach (var t in ItemTargets(dd.DropDownItems)) yield return t;
        }
    }

    private void OnHandleCreated(object? sender, EventArgs e)
    {
        if (_container is not null) _container.HandleCreated -= OnHandleCreated;
        Apply();
    }

    private void Subscribe()
    {
        if (_subscribed || Translator is null) return;
        Translator.LanguageChanged += OnLanguageChanged;
        _subscribed = true;
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        if (_container is { IsHandleCreated: true, InvokeRequired: true })
            _container.BeginInvoke(Apply);
        else
            Apply();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _subscribed && Translator is not null)
        {
            Translator.LanguageChanged -= OnLanguageChanged;
            _subscribed = false;
        }
        base.Dispose(disposing);
    }
}
```

- [ ] **Step 7: Implement `IconThemeImageList.cs`**

```csharp
using System.ComponentModel;

namespace GameCatalog.UI.Components;

/// <summary>
/// Provides the AMC toolbar icon themes (Resources\*.bmp strips of 16x16 icons) and assigns
/// the active theme to tool strips: set "UseThemeIcons" on a ToolStrip in the Designer.
/// </summary>
[ToolboxItem(true), ProvideProperty("UseThemeIcons", typeof(ToolStrip))]
public sealed class IconThemeImageList : Component, IExtenderProvider
{
    public sealed record IconTheme(string Id, string FileName, string NameKey);

    public const string DefaultThemeId = "WindowsXP";

    public static readonly IReadOnlyList<IconTheme> Themes = new[]
    {
        new IconTheme("Classic", "Classic.bmp", "Theme.Classic"),
        new IconTheme("Crystal", "Crystal.bmp", "Theme.Crystal"),
        new IconTheme("Fresh", "Fresh.bmp", "Theme.Fresh"),
        new IconTheme("Gnome", "Gnome.bmp", "Theme.Gnome"),
        new IconTheme("Scrows", "Scrows.bmp", "Theme.Scrows"),
        new IconTheme("WindowsXP", "Windows XP.bmp", "Theme.WindowsXP"),
    };

    /// <summary>Theme names stored in prefs.json by the Polish-only version.</summary>
    private static readonly Dictionary<string, string> LegacyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Klasyczny"] = "Classic",
        ["Kryształowy"] = "Crystal",
        ["Świeży"] = "Fresh",
        ["Gnom"] = "Gnome",
        ["Windows XP"] = "WindowsXP",
    };

    private readonly HashSet<ToolStrip> _strips = new();
    private string _theme = DefaultThemeId;

    public IconThemeImageList() => ImageList = NewImageList();

    public IconThemeImageList(IContainer container) : this() => container.Add(this);

    [Browsable(false)]
    public ImageList ImageList { get; private set; }

    /// <summary>Folder with the theme bitmaps, relative to the executable.</summary>
    [DefaultValue("Resources")]
    public string ResourceDirectory { get; set; } = "Resources";

    [DefaultValue(DefaultThemeId)]
    public string Theme
    {
        get => _theme;
        set
        {
            _theme = NormalizeThemeId(value);
            if (!(Site?.DesignMode ?? false)) Reload();
        }
    }

    public event EventHandler? ThemeChanged;

    public static string NormalizeThemeId(string? stored)
    {
        if (string.IsNullOrWhiteSpace(stored)) return DefaultThemeId;
        var byId = Themes.FirstOrDefault(t => t.Id.Equals(stored, StringComparison.OrdinalIgnoreCase));
        if (byId is not null) return byId.Id;
        return LegacyNames.GetValueOrDefault(stored, DefaultThemeId);
    }

    /// <summary>Cuts a theme strip into 16x16 icons; a missing or unreadable file gives an empty list.</summary>
    public static ImageList LoadStrip(string themeId, string resourceDirectory)
    {
        var images = NewImageList();
        var id = NormalizeThemeId(themeId);
        var theme = Themes.First(t => t.Id == id);
        var path = Path.Combine(resourceDirectory, theme.FileName);
        if (!File.Exists(path)) return images;
        try
        {
            using var strip = new Bitmap(path);
            for (int i = 0; i < strip.Width / 16; i++)
            {
                var icon = new Bitmap(16, 16);
                using (var g = Graphics.FromImage(icon))
                    g.DrawImage(strip, new Rectangle(0, 0, 16, 16), new Rectangle(i * 16, 0, 16, 16), GraphicsUnit.Pixel);
                images.Images.Add(icon);
            }
        }
        catch (ArgumentException) { /* not a valid bitmap: keep the list empty */ }
        return images;
    }

    public bool CanExtend(object extendee) => extendee is ToolStrip;

    [DefaultValue(false), Category("Appearance"), Description("Use the AMC icon theme for this strip.")]
    public bool GetUseThemeIcons(ToolStrip strip) => _strips.Contains(strip);

    public void SetUseThemeIcons(ToolStrip strip, bool value)
    {
        if (value)
        {
            _strips.Add(strip);
            Assign(strip);
        }
        else
        {
            _strips.Remove(strip);
        }
    }

    /// <summary>Re-assigns the image list (call after adding drop-down items at runtime).</summary>
    public void Refresh()
    {
        foreach (var strip in _strips) Assign(strip);
    }

    private void Reload()
    {
        var dir = Path.IsPathRooted(ResourceDirectory)
            ? ResourceDirectory
            : Path.Combine(AppContext.BaseDirectory, ResourceDirectory);
        var old = ImageList;
        ImageList = LoadStrip(_theme, dir);
        Refresh();
        old.Dispose();
        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Assign(ToolStrip strip)
    {
        strip.ImageList = ImageList;
        AssignDropDowns(strip.Items);
    }

    private void AssignDropDowns(ToolStripItemCollection items)
    {
        foreach (ToolStripItem item in items)
        {
            if (item is not ToolStripDropDownItem dd) continue;
            dd.DropDown.ImageList = ImageList;
            AssignDropDowns(dd.DropDownItems);
        }
    }

    private static ImageList NewImageList() => new()
    {
        ImageSize = new Size(16, 16),
        ColorDepth = ColorDepth.Depth32Bit,
        TransparentColor = Color.Magenta,
    };

    protected override void Dispose(bool disposing)
    {
        if (disposing) ImageList.Dispose();
        base.Dispose(disposing);
    }
}
```

- [ ] **Step 8: Run tests, build, run the app**

Run: `dotnet test tests/GameCatalog.UI.Tests` → Expected: all PASS. If a WinForms control throws a threading error under the xUnit runner, add package `Xunit.StaFact` to the test project and change the affected `[Fact]` to `[StaFact]`.
Run: `dotnet test GameCatalog.slnx` → all PASS.
Run the app: icons unchanged; Preferences shows translated theme names; switching theme works; `prefs.json` now stores `"Theme": "WindowsXP"` (or the chosen id).

- [ ] **Step 9: Commit**

```bash
git add -A
git commit -m "feat(ui): ActionList/UIAction, Localizer and IconThemeImageList components

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 8: App services — session, undo, numbering, list presenter, clipboard, covers

These services are created and tested here but wired into the UI only in Task 11 (the new `MainForm`). The current `MainForm` keeps working unchanged until then.

**Files:**
- Create: `src/GameCatalog.App/Services/CatalogSession.cs`, `UndoService.cs`, `NumberConflictResolution.cs`, `NumberingService.cs`, `GameListPresenter.cs`, `GameClipboard.cs`, `CoverImages.cs`
- Modify: `src/GameCatalog.App/Services/PrefsStore.cs` (`FilePath`, `Load(string? path)`), `src/GameCatalog.App/Forms/SmallDialogs.cs` (remove the enum; `using GameCatalog.App.Services;`)
- Test: `tests/GameCatalog.App.Tests/CatalogSessionTests.cs`, `UndoServiceTests.cs`, `NumberingServiceTests.cs`, `GameListPresenterTests.cs`, `GameClipboardTests.cs`, `CoverImagesTests.cs`

**Interfaces:**
- Consumes: `GameList`, `GameFieldValues`, `CatalogLabels` (Task 2); `SqliteGameCatalogStore`, `CatalogData` (Task 3); `PrefsStore` (Task 6); `SampleDataFactory.CreateSamples()` (existing).
- Produces (`namespace GameCatalog.App.Services`):
  - `PrefsStore.Load(string? path = null)`, `[JsonIgnore] string? FilePath`, `Save()` (no-op when `FilePath` is null)
  - `UndoService` — `const int Capacity = 50`, `Count`, `Push(Action)`, `TryUndo() : bool`, `Clear()`
  - `CatalogSession(PrefsStore prefs)` — `Games : GameList`, `Loans : List<LoanRecord>`, `Undo : UndoService`, `FilePath`, `IsDirty`, `static DefaultPath()`, `Open(string path, bool addSamplesIfNew = false)`, `New(string path)`, `Save()`, `SaveAs(string path)`, `MarkDirty()`, events `Opened`, `Saving`, `Saved`, `DirtyChanged`, `Changed`
  - `enum NumberConflictResolution { AcceptDuplicate, RenumberExisting, ShiftNew }`
  - `NumberingService(CatalogSession)` — `ShiftUntilFree(int)`, `ResolveNewNumber(int desired, NumberConflictResolution) : int`, `ChangeNumber(Game, int newNumber, NumberConflictResolution) : int?` (returns the number actually assigned or null when unchanged), `RenumberSequential(IReadOnlyList<Game> view)`
  - `ListColumn { string Tag; int Width }`, `ListViewState`, `FindCriteria(string FieldTag, string Text, bool WholeField, bool Reverse)`, `GameRow(Game Game, IReadOnlyList<string> Cells, string GroupKey)`, `GameGroup(string Key, string Caption)`, `ListViewModel(IReadOnlyList<GameRow> Rows, IReadOnlyList<GameGroup> Groups, int Total)`
  - `GameListPresenter(GameList games, PrefsStore prefs)` — `State`, `CurrentView()`, `Build()`, `FormatTitle(Game)`, `CellText(Game, string tag)`, `static Matches(Game, FindCriteria)`, `FindAll(FindCriteria)`, `FindFirst(FindCriteria)`, `FindNext(FindCriteria, int? currentNumber)`, `static AvailableColumns`, `ToggleColumn(string tag, bool on)`
  - `GameClipboard.Serialize(Game) : string`, `GameClipboard.TryDeserialize(string?) : Game?`
  - `CoverImages.Downscale(byte[], int maxWidth)`, `ToPng(Image)`, `TryLoad(byte[]?) : Image?`, `ToDataUri(byte[]?) : string?`

- [ ] **Step 1: Make `PrefsStore` testable**

In `Services/PrefsStore.cs`:
```csharp
/// <summary>File the preferences were loaded from; null for in-memory instances (tests).</summary>
[System.Text.Json.Serialization.JsonIgnore]
public string? FilePath { get; private set; }

public static PrefsStore Load(string? path = null)
{
    path ??= PrefsPath();
    PrefsStore prefs;
    try
    {
        prefs = File.Exists(path)
            ? JsonSerializer.Deserialize<PrefsStore>(File.ReadAllText(path)) ?? new PrefsStore()
            : new PrefsStore();
    }
    catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
    {
        prefs = new PrefsStore();
    }
    prefs.FilePath = path;
    prefs.Theme = IconThemeImageList.NormalizeThemeId(prefs.Theme);
    return prefs;
}

public void Save()
{
    if (FilePath is null) return;
    try { File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true })); }
    catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { /* preferences are best effort */ }
}
```

- [ ] **Step 2: Write failing tests**

`tests/GameCatalog.App.Tests/UndoServiceTests.cs`:
```csharp
using GameCatalog.App.Services;

namespace GameCatalog.App.Tests;

public class UndoServiceTests
{
    [Fact]
    public void TryUndo_RunsLastActionFirst()
    {
        var u = new UndoService();
        var log = new List<int>();
        u.Push(() => log.Add(1));
        u.Push(() => log.Add(2));
        Assert.True(u.TryUndo());
        Assert.True(u.TryUndo());
        Assert.False(u.TryUndo());
        Assert.Equal(new[] { 2, 1 }, log);
    }

    [Fact]
    public void Push_KeepsOnlyTheNewestActions()
    {
        var u = new UndoService();
        for (int i = 0; i < UndoService.Capacity + 10; i++) u.Push(() => { });
        Assert.Equal(UndoService.Capacity, u.Count);
    }
}
```

`tests/GameCatalog.App.Tests/CatalogSessionTests.cs`:
```csharp
using GameCatalog.App.Services;
using GameCatalog.Core.Models;
using Microsoft.Data.Sqlite;

namespace GameCatalog.App.Tests;

public sealed class CatalogSessionTests : IDisposable
{
    private readonly string _dir = Directory.CreateTempSubdirectory("gc-session-").FullName;
    private string PathOf(string name) => Path.Combine(_dir, name);

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        Directory.Delete(_dir, recursive: true);
    }

    [Fact]
    public void Open_NewDefaultFile_AddsSamples()
    {
        var s = new CatalogSession(new PrefsStore());
        s.Open(PathOf("new.gc"), addSamplesIfNew: true);
        Assert.NotEmpty(s.Games.Games);
        Assert.False(s.IsDirty);
    }

    [Fact]
    public void Open_ExistingEmptyFile_DoesNotAddSamples()
    {
        var s = new CatalogSession(new PrefsStore());
        s.New(PathOf("empty.gc"));
        var reopened = new CatalogSession(new PrefsStore());
        reopened.Open(PathOf("empty.gc"), addSamplesIfNew: true);
        Assert.Empty(reopened.Games.Games);
    }

    [Fact]
    public void Save_RaisesSavingThenSaved_ClearsDirty_WritesBackup()
    {
        var prefs = new PrefsStore { BackupOnSave = true };
        var s = new CatalogSession(prefs);
        s.New(PathOf("a.gc"));
        var events = new List<string>();
        s.Saving += (_, _) => events.Add("saving");
        s.Saved += (_, _) => events.Add("saved");
        s.Games.Add(new Game { Title = "X" });
        s.MarkDirty();
        Assert.True(s.IsDirty);

        s.Save();

        Assert.Equal(new[] { "saving", "saved" }, events);
        Assert.False(s.IsDirty);
        Assert.True(File.Exists(PathOf("a.gc.bak")));
    }

    [Fact]
    public void SaveAs_WritesToNewFileAndKeepsGames()
    {
        var s = new CatalogSession(new PrefsStore());
        s.New(PathOf("a.gc"));
        s.Games.Add(new Game { Title = "X" });
        s.SaveAs(PathOf("b.gc"));
        var other = new CatalogSession(new PrefsStore());
        other.Open(PathOf("b.gc"));
        Assert.Equal("X", Assert.Single(other.Games.Games).Title);
        Assert.Equal(PathOf("b.gc"), s.FilePath);
    }

    [Fact]
    public void MarkDirty_RaisesDirtyChangedOnce_AndChangedEveryTime()
    {
        var s = new CatalogSession(new PrefsStore());
        s.New(PathOf("a.gc"));
        int dirty = 0, changed = 0;
        s.DirtyChanged += (_, _) => dirty++;
        s.Changed += (_, _) => changed++;
        s.MarkDirty();
        s.MarkDirty();
        Assert.Equal(1, dirty);
        Assert.Equal(2, changed);
    }

    [Fact]
    public void Open_PushesRecentFile()
    {
        var prefs = new PrefsStore();
        var s = new CatalogSession(prefs);
        s.New(PathOf("a.gc"));
        Assert.Equal(PathOf("a.gc"), prefs.RecentFiles[0]);
    }
}
```

`tests/GameCatalog.App.Tests/NumberingServiceTests.cs`:
```csharp
using GameCatalog.App.Services;
using GameCatalog.Core.Models;

namespace GameCatalog.App.Tests;

public class NumberingServiceTests
{
    private static (CatalogSession Session, NumberingService Numbering) Create()
    {
        var s = new CatalogSession(new PrefsStore());
        s.Games.Add(new Game { Number = 1, Title = "A", Extras = { new GameExtra { Title = "A-DLC", GameNumber = 1 } } });
        s.Games.Add(new Game { Number = 2, Title = "B" });
        s.Games.Add(new Game { Number = 5, Title = "C" });
        s.Loans.Add(new LoanRecord { GameNumber = 1, Borrower = "Ola" });
        s.Loans.Add(new LoanRecord { GameNumber = 2, Borrower = "Jan" });
        return (s, new NumberingService(s));
    }

    [Fact]
    public void ShiftUntilFree_SkipsTakenNumbers()
        => Assert.Equal(3, Create().Numbering.ShiftUntilFree(1));

    [Fact]
    public void ResolveNewNumber_Variants()
    {
        var (s, n) = Create();
        Assert.Equal(4, n.ResolveNewNumber(4, NumberConflictResolution.ShiftNew));
        Assert.Equal(2, n.ResolveNewNumber(2, NumberConflictResolution.AcceptDuplicate));
        Assert.Equal(3, n.ResolveNewNumber(2, NumberConflictResolution.ShiftNew));
        Assert.Equal(2, n.ResolveNewNumber(2, NumberConflictResolution.RenumberExisting));
        Assert.Null(s.Games.Find(2));                         // "B" moved away
        Assert.Equal(6, s.Games.Games.Single(g => g.Title == "B").Number);
        Assert.Equal(6, s.Loans.Single(l => l.Borrower == "Jan").GameNumber);
    }

    [Fact]
    public void ChangeNumber_RenumberExisting_SwapsGamesLoansAndExtras()
    {
        var (s, n) = Create();
        var a = s.Games.Find(1)!;

        Assert.Equal(2, n.ChangeNumber(a, 2, NumberConflictResolution.RenumberExisting));

        Assert.Equal("A", s.Games.Find(2)!.Title);
        Assert.Equal("B", s.Games.Find(1)!.Title);
        Assert.Equal(2, a.Extras[0].GameNumber);
        Assert.Equal(2, s.Loans.Single(l => l.Borrower == "Ola").GameNumber);
        Assert.Equal(1, s.Loans.Single(l => l.Borrower == "Jan").GameNumber);
        Assert.True(s.IsDirty);
    }

    [Fact]
    public void ChangeNumber_ShiftNew_UsesNextFreeNumber()
    {
        var (s, n) = Create();
        Assert.Equal(3, n.ChangeNumber(s.Games.Find(1)!, 2, NumberConflictResolution.ShiftNew));
    }

    [Fact]
    public void ChangeNumber_AcceptDuplicate_LeavesOtherGameAlone()
    {
        var (s, n) = Create();
        n.ChangeNumber(s.Games.Find(1)!, 2, NumberConflictResolution.AcceptDuplicate);
        Assert.Equal(2, s.Games.Games.Count(g => g.Number == 2));
        Assert.Equal(2, s.Loans.Single(l => l.Borrower == "Jan").GameNumber);
    }

    [Fact]
    public void ChangeNumber_SameNumber_ReturnsNull()
    {
        var (s, n) = Create();
        Assert.Null(n.ChangeNumber(s.Games.Find(1)!, 1, NumberConflictResolution.RenumberExisting));
        Assert.Equal(0, s.Undo.Count);
    }

    [Fact]
    public void Undo_RestoresNumbersLoansAndExtras()
    {
        var (s, n) = Create();
        n.ChangeNumber(s.Games.Find(1)!, 2, NumberConflictResolution.RenumberExisting);
        Assert.True(s.Undo.TryUndo());
        Assert.Equal("A", s.Games.Find(1)!.Title);
        Assert.Equal(1, s.Games.Find(1)!.Extras[0].GameNumber);
        Assert.Equal(1, s.Loans.Single(l => l.Borrower == "Ola").GameNumber);
    }

    [Fact]
    public void RenumberSequential_FollowsViewOrder()
    {
        var (s, n) = Create();
        var view = s.Games.Games.OrderByDescending(g => g.Title).ToList();   // C, B, A
        n.RenumberSequential(view);
        Assert.Equal(new[] { "C", "B", "A" }, s.Games.Games.OrderBy(g => g.Number).Select(g => g.Title));
        Assert.Equal(3, s.Loans.Single(l => l.Borrower == "Ola").GameNumber);
        Assert.True(s.Undo.TryUndo());
        Assert.Equal("A", s.Games.Find(1)!.Title);
    }
}
```

`tests/GameCatalog.App.Tests/GameListPresenterTests.cs`:
```csharp
using GameCatalog.App.Services;
using GameCatalog.Core.Catalog;
using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;

namespace GameCatalog.App.Tests;

public class GameListPresenterTests
{
    private static GameListPresenter Create(PrefsStore? prefs = null)
    {
        CatalogLabels.Current = new InvariantCatalogLabels();
        var list = new GameList();
        list.Add(new Game { Title = "Doom 10", LocalizedTitle = "", Genre = "FPS" });
        list.Add(new Game { Title = "Doom 2", LocalizedTitle = "Zagłada 2", Genre = "FPS" });
        list.Add(new Game { Title = "Abe", Genre = "" });
        return new GameListPresenter(list, prefs ?? new PrefsStore { NaturalSort = true, TitleFormat = "Both", ShowGroupCount = true });
    }

    [Fact]
    public void CurrentView_SortsNaturallyAndRespectsFindOnly()
    {
        var p = Create();
        Assert.Equal(new[] { "Abe", "Doom 2", "Doom 10" }, p.CurrentView().Select(g => g.Title));
        p.State.FindOnly = new HashSet<int> { 1 };
        Assert.Equal("Doom 10", Assert.Single(p.CurrentView()).Title);
    }

    [Fact]
    public void Build_GroupsWithCaptionsAndCounts()
    {
        var p = Create();
        p.State.GroupTag = "Genre";
        var vm = p.Build();
        Assert.Equal(3, vm.Total);
        Assert.Contains(vm.Groups, g => g.Key == "" && g.Caption == "(none) (1)");
        Assert.Contains(vm.Groups, g => g.Key == "FPS" && g.Caption == "FPS (2)");
        Assert.All(vm.Rows, r => Assert.Equal(p.State.Columns.Count, r.Cells.Count));
    }

    [Theory]
    [InlineData("Both", "Doom 2 / Zagłada 2")]
    [InlineData("Original", "Doom 2")]
    [InlineData("Localized", "Zagłada 2")]
    public void FormatTitle_FollowsPreference(string format, string expected)
    {
        var p = Create(new PrefsStore { TitleFormat = format });
        var g = p.CurrentView().Single(x => x.Title == "Doom 2");
        Assert.Equal(expected, p.FormatTitle(g));
    }

    [Fact]
    public void Matches_WholeFieldAndReverse()
    {
        var g = new Game { Title = "Doom" };
        Assert.True(GameListPresenter.Matches(g, new FindCriteria("Title", "oo", false, false)));
        Assert.False(GameListPresenter.Matches(g, new FindCriteria("Title", "oo", true, false)));
        Assert.True(GameListPresenter.Matches(g, new FindCriteria("Title", "doom", true, false)));
        Assert.True(GameListPresenter.Matches(g, new FindCriteria("Title", "xyz", false, true)));
    }

    [Fact]
    public void FindNext_WrapsAround()
    {
        var p = Create();
        var c = new FindCriteria("Title", "Doom", false, false);
        var first = p.FindNext(c, currentNumber: null)!;
        var second = p.FindNext(c, first.Number)!;
        var third = p.FindNext(c, second.Number)!;
        Assert.Equal("Doom 2", first.Title);
        Assert.Equal("Doom 10", second.Title);
        Assert.Equal("Doom 2", third.Title);
    }

    [Fact]
    public void ToggleColumn_NeverRemovesTheLastColumn()
    {
        var p = Create();
        p.ToggleColumn("Genre", true);
        Assert.Contains(p.State.Columns, c => c.Tag == "Genre");
        p.ToggleColumn("Genre", false);
        p.ToggleColumn("Title", false);
        p.ToggleColumn("Number", false);
        Assert.Single(p.State.Columns);
    }
}
```

`tests/GameCatalog.App.Tests/GameClipboardTests.cs`:
```csharp
using GameCatalog.App.Services;
using GameCatalog.Core.Models;

namespace GameCatalog.App.Tests;

public class GameClipboardTests
{
    [Fact]
    public void RoundTrip_ResetsNumberAndKeepsData()
    {
        var g = new Game { Number = 7, Title = "Quake", CoverImage = new byte[] { 9 } };
        g.Extras.Add(new GameExtra { Title = "DLC" });
        var copy = GameClipboard.TryDeserialize(GameClipboard.Serialize(g))!;
        Assert.Equal(0, copy.Number);
        Assert.Equal("Quake", copy.Title);
        Assert.Equal(new byte[] { 9 }, copy.CoverImage);
        Assert.Equal("DLC", Assert.Single(copy.Extras).Title);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("hello")]
    [InlineData("GAMECATALOG1:{not json")]
    [InlineData("GAMECATALOG1:{\"Title\":\"\"}")]
    public void TryDeserialize_RejectsForeignOrEmptyContent(string? text)
        => Assert.Null(GameClipboard.TryDeserialize(text));
}
```

`tests/GameCatalog.App.Tests/CoverImagesTests.cs`:
```csharp
using System.Drawing.Imaging;
using GameCatalog.App.Services;

namespace GameCatalog.App.Tests;

public class CoverImagesTests
{
    private static byte[] Png(int w, int h)
    {
        using var bmp = new Bitmap(w, h);
        using var ms = new MemoryStream();
        bmp.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }

    [Fact]
    public void Downscale_ReducesWidthKeepingAspect()
    {
        using var img = CoverImages.TryLoad(CoverImages.Downscale(Png(400, 200), 100))!;
        Assert.Equal(100, img.Width);
        Assert.Equal(50, img.Height);
    }

    [Fact]
    public void Downscale_ZeroOrNarrowImage_ReturnsSameBytes()
    {
        var bytes = Png(50, 50);
        Assert.Same(bytes, CoverImages.Downscale(bytes, 0));
        Assert.Same(bytes, CoverImages.Downscale(bytes, 100));
    }

    [Fact]
    public void ToDataUri_DetectsPng_NullForEmpty()
    {
        Assert.StartsWith("data:image/png;base64,", CoverImages.ToDataUri(Png(2, 2)));
        Assert.Null(CoverImages.ToDataUri(null));
        Assert.Null(CoverImages.ToDataUri(Array.Empty<byte>()));
    }

    [Fact]
    public void TryLoad_InvalidBytes_ReturnsNull() => Assert.Null(CoverImages.TryLoad(new byte[] { 1, 2, 3 }));
}
```

- [ ] **Step 3: Run tests to verify they fail**

Run: `dotnet test tests/GameCatalog.App.Tests`
Expected: build FAIL — services missing.

- [ ] **Step 4: Implement `UndoService.cs` and `NumberConflictResolution.cs`**

```csharp
namespace GameCatalog.App.Services;

/// <summary>Bounded stack of undo actions (newest first).</summary>
public sealed class UndoService
{
    public const int Capacity = 50;
    private readonly LinkedList<Action> _actions = new();

    public int Count => _actions.Count;

    public void Push(Action undo)
    {
        _actions.AddLast(undo);
        if (_actions.Count > Capacity) _actions.RemoveFirst();
    }

    public bool TryUndo()
    {
        if (_actions.Last is not { } last) return false;
        _actions.RemoveLast();
        last.Value();
        return true;
    }

    public void Clear() => _actions.Clear();
}
```

```csharp
namespace GameCatalog.App.Services;

/// <summary>How to resolve a game number that is already taken — options of the AMC "Movie number" dialog.</summary>
public enum NumberConflictResolution
{
    /// <summary>Accept a duplicate (two records with the same number).</summary>
    AcceptDuplicate,
    /// <summary>Give the existing record another number (the next free one, or swap on change).</summary>
    RenumberExisting,
    /// <summary>Increase the new number until it is free.</summary>
    ShiftNew,
}
```
Remove the enum from `Forms/SmallDialogs.cs`.

- [ ] **Step 5: Implement `CatalogSession.cs`**

```csharp
using GameCatalog.Core.Catalog;
using GameCatalog.Core.Models;
using GameCatalog.Core.Services;
using GameCatalog.Storage;

namespace GameCatalog.App.Services;

/// <summary>
/// The open catalog: games, loans, file, dirty flag and undo stack (AMC TMovieList + file
/// handling from main.pas). UI code listens to its events instead of tracking state itself.
/// </summary>
public sealed class CatalogSession(PrefsStore prefs)
{
    private SqliteGameCatalogStore? _store;

    public GameList Games { get; } = new();
    public List<LoanRecord> Loans { get; } = new();
    public UndoService Undo { get; } = new();
    public string FilePath { get; private set; } = "";
    public bool IsDirty { get; private set; }

    /// <summary>A catalog was opened or created.</summary>
    public event EventHandler? Opened;
    /// <summary>Raised before writing — editors commit pending changes here.</summary>
    public event EventHandler? Saving;
    public event EventHandler? Saved;
    public event EventHandler? DirtyChanged;
    /// <summary>Catalog content changed; views refresh.</summary>
    public event EventHandler? Changed;

    /// <summary>Documents\GameCatalog\catalog.gc (copied once from the legacy catalog.db).</summary>
    public static string DefaultPath()
    {
        var gc = Path.Combine(AppPaths.DataDirectory, "catalog.gc");
        var legacy = Path.Combine(AppPaths.DataDirectory, "catalog.db");
        if (!File.Exists(gc) && File.Exists(legacy))
        {
            try { File.Copy(legacy, gc); }
            catch (IOException) { /* the legacy file stays usable */ }
        }
        return gc;
    }

    /// <param name="addSamplesIfNew">Fill a catalog file that did not exist yet with sample games (first run).</param>
    public void Open(string path, bool addSamplesIfNew = false)
    {
        bool existed = File.Exists(path);
        _store = new SqliteGameCatalogStore(path);
        var data = _store.LoadAll();
        Games.ReplaceAll(data.Games);
        Games.CustomFieldDefs.Clear();
        Games.CustomFieldDefs.AddRange(data.CustomFieldDefs);
        Loans.Clear();
        Loans.AddRange(data.Loans);
        Undo.Clear();
        FilePath = path;
        SetDirty(false);
        if (!existed && addSamplesIfNew)
        {
            foreach (var g in SampleDataFactory.CreateSamples()) Games.Add(g);
            _store.SaveAll(Games, Loans);
        }
        RememberRecent(path);
        Opened?.Invoke(this, EventArgs.Empty);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void New(string path)
    {
        Games.Clear();
        Games.CustomFieldDefs.Clear();
        Loans.Clear();
        Undo.Clear();
        _store = new SqliteGameCatalogStore(path);
        _store.SaveAll(Games, Loans);
        FilePath = path;
        SetDirty(false);
        RememberRecent(path);
        Opened?.Invoke(this, EventArgs.Empty);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Save()
    {
        if (_store is null) throw new InvalidOperationException("No catalog is open.");
        Saving?.Invoke(this, EventArgs.Empty);
        if (prefs.BackupOnSave && File.Exists(FilePath))
        {
            try { File.Copy(FilePath, FilePath + ".bak", overwrite: true); }
            catch (IOException) { /* a failed backup must not block saving */ }
        }
        _store.SaveAll(Games, Loans);
        SetDirty(false);
        Saved?.Invoke(this, EventArgs.Empty);
    }

    public void SaveAs(string path)
    {
        _store = new SqliteGameCatalogStore(path);
        FilePath = path;
        Save();
        RememberRecent(path);
    }

    /// <summary>Marks unsaved changes and notifies views.</summary>
    public void MarkDirty()
    {
        SetDirty(true);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void SetDirty(bool value)
    {
        if (IsDirty == value) return;
        IsDirty = value;
        DirtyChanged?.Invoke(this, EventArgs.Empty);
    }

    private void RememberRecent(string path)
    {
        prefs.PushRecent(path);
        prefs.Save();
    }
}
```

- [ ] **Step 6: Implement `NumberingService.cs`**

```csharp
using GameCatalog.Core.Models;

namespace GameCatalog.App.Services;

/// <summary>
/// Game number changes (AMC number.pas / renumber.pas): keeps loans and extras pointing at the
/// right game and records one undo step per operation.
/// </summary>
public sealed class NumberingService(CatalogSession session)
{
    public int ShiftUntilFree(int start)
    {
        int n = start;
        while (session.Games.Find(n) is not null) n++;
        return n;
    }

    /// <summary>Number to give a new game whose desired number may already be taken.</summary>
    public int ResolveNewNumber(int desired, NumberConflictResolution choice)
    {
        var existing = session.Games.Find(desired);
        if (existing is null) return desired;
        switch (choice)
        {
            case NumberConflictResolution.ShiftNew:
                return ShiftUntilFree(desired);
            case NumberConflictResolution.RenumberExisting:
                var snapshot = Snapshot();
                Reassign(existing, ShiftUntilFree(session.Games.NextNumber()));
                Commit(snapshot);
                return desired;
            default:
                return desired;
        }
    }

    /// <returns>The number actually assigned, or null when nothing changed.</returns>
    public int? ChangeNumber(Game game, int newNumber, NumberConflictResolution conflict)
    {
        if (newNumber == game.Number) return null;
        var other = session.Games.Find(newNumber);
        var snapshot = Snapshot();
        int oldNumber = game.Number;
        if (other is null || conflict == NumberConflictResolution.AcceptDuplicate)
        {
            Reassign(game, newNumber);
        }
        else if (conflict == NumberConflictResolution.ShiftNew)
        {
            newNumber = ShiftUntilFree(newNumber);
            Reassign(game, newNumber);
        }
        else
        {
            // Swap: loans are remapped in one pass so the two sets do not collide.
            foreach (var loan in session.Loans)
            {
                if (loan.GameNumber == oldNumber) loan.GameNumber = newNumber;
                else if (loan.GameNumber == newNumber) loan.GameNumber = oldNumber;
            }
            SetNumber(other, oldNumber);
            SetNumber(game, newNumber);
        }
        Commit(snapshot);
        return newNumber;
    }

    /// <summary>Numbers the given games 1..n in the given order (Tools → Renumber).</summary>
    public void RenumberSequential(IReadOnlyList<Game> view)
    {
        if (view.Count == 0) return;
        var snapshot = Snapshot();
        var newByOld = new Dictionary<int, int>();
        int n = 1;
        foreach (var g in view)
        {
            newByOld.TryAdd(g.Number, n);
            SetNumber(g, n++);
        }
        foreach (var loan in session.Loans)
            if (newByOld.TryGetValue(loan.GameNumber, out var nn)) loan.GameNumber = nn;
        Commit(snapshot);
    }

    private void Reassign(Game game, int number)
    {
        foreach (var loan in session.Loans)
            if (loan.GameNumber == game.Number) loan.GameNumber = number;
        SetNumber(game, number);
    }

    private static void SetNumber(Game game, int number)
    {
        game.Number = number;
        foreach (var e in game.Extras) e.GameNumber = number;
    }

    private sealed record NumberSnapshot(Dictionary<Game, int> Games, Dictionary<LoanRecord, int> Loans);

    private NumberSnapshot Snapshot()
    {
        // Reference equality: duplicates by number must stay distinct entries.
        var games = new Dictionary<Game, int>(ReferenceEqualityComparer.Instance);
        foreach (var g in session.Games.Games) games[g] = g.Number;
        var loans = new Dictionary<LoanRecord, int>(ReferenceEqualityComparer.Instance);
        foreach (var l in session.Loans) loans[l] = l.GameNumber;
        return new NumberSnapshot(games, loans);
    }

    private void Commit(NumberSnapshot snapshot)
    {
        session.Games.ReplaceAll(session.Games.Games.OrderBy(g => g.Number));
        session.Undo.Push(() =>
        {
            foreach (var (g, number) in snapshot.Games) SetNumber(g, number);
            foreach (var (l, number) in snapshot.Loans) l.GameNumber = number;
            session.Games.ReplaceAll(session.Games.Games.OrderBy(g => g.Number));
            session.MarkDirty();
        });
        session.MarkDirty();
    }
}
```
- [ ] **Step 7: Implement `GameListPresenter.cs`**

```csharp
using GameCatalog.Core.Catalog;
using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;

namespace GameCatalog.App.Services;

/// <summary>A visible list column.</summary>
public sealed class ListColumn(string tag, int width)
{
    public string Tag { get; } = tag;
    public int Width { get; set; } = width;
}

/// <summary>View options of the game list (AMC sort / group / grid fields state).</summary>
public sealed class ListViewState
{
    public string SortTag { get; set; } = "Title";
    public bool Ascending { get; set; } = true;
    /// <summary>Empty = no grouping.</summary>
    public string GroupTag { get; set; } = "";
    public bool Thumbnails { get; set; }
    public List<ListColumn> Columns { get; } = new() { new("Number", 40), new("Title", 210) };
    /// <summary>When set, only these game numbers are shown (find bar "show results").</summary>
    public HashSet<int>? FindOnly { get; set; }
}

public sealed record FindCriteria(string FieldTag, string Text, bool WholeField, bool Reverse);
public sealed record GameRow(Game Game, IReadOnlyList<string> Cells, string GroupKey);
public sealed record GameGroup(string Key, string Caption);
public sealed record ListViewModel(IReadOnlyList<GameRow> Rows, IReadOnlyList<GameGroup> Groups, int Total);

/// <summary>Computes what the game list shows; free of WinForms so it can be tested.</summary>
public sealed class GameListPresenter(GameList games, PrefsStore prefs)
{
    /// <summary>Columns offered in View → Grid fields, with their default widths.</summary>
    public static readonly IReadOnlyList<(string Tag, int Width)> AvailableColumns = new[]
    {
        ("Number", 40), ("Title", 210), ("Developer", 110), ("Publisher", 110), ("Genre", 100),
        ("Platform", 80), ("ReleaseYear", 50), ("CompletionStatus", 105), ("UserRating", 55),
        ("PlaytimeHours", 60), ("Store", 110),
    };

    public ListViewState State { get; } = new();

    public List<Game> CurrentView()
    {
        IEnumerable<Game> q = games.Games;
        if (State.FindOnly is { } only) q = q.Where(g => only.Contains(g.Number));
        return GameList.Sort(q, State.SortTag, State.Ascending, prefs.NaturalSort).ToList();
    }

    public ListViewModel Build()
    {
        var view = CurrentView();
        var groups = new List<GameGroup>();
        if (State.GroupTag.Length > 0)
        {
            foreach (var grp in GameList.GroupBy(view, State.GroupTag))
            {
                var caption = GameList.GroupCaption(State.GroupTag, grp.Key, CatalogLabels.Current);
                groups.Add(new GameGroup(grp.Key, prefs.ShowGroupCount ? $"{caption} ({grp.Count()})" : caption));
            }
        }
        var rows = view.Select(g => new GameRow(
            g,
            State.Columns.Select(c => CellText(g, c.Tag)).ToList(),
            State.GroupTag.Length > 0 ? GameList.GroupKeyOf(g, State.GroupTag) : "")).ToList();
        return new ListViewModel(rows, groups, games.Games.Count);
    }

    /// <summary>Title as configured in preferences (AMC title formatting).</summary>
    public string FormatTitle(Game g) => prefs.TitleFormat switch
    {
        "Original" => g.Title,
        "Localized" => string.IsNullOrWhiteSpace(g.LocalizedTitle) ? g.Title : g.LocalizedTitle,
        _ => g.DisplayTitle,
    };

    public string CellText(Game g, string tag) => tag == "Title" ? FormatTitle(g) : GameFieldValues.Get(g, tag);

    public static bool Matches(Game g, FindCriteria c)
    {
        var value = GameFieldValues.Get(g, c.FieldTag);
        bool hit = c.WholeField
            ? value.Equals(c.Text, StringComparison.OrdinalIgnoreCase)
            : value.Contains(c.Text, StringComparison.OrdinalIgnoreCase);
        return c.Reverse ? !hit : hit;
    }

    public HashSet<int> FindAll(FindCriteria c) =>
        games.Games.Where(g => Matches(g, c)).Select(g => g.Number).ToHashSet();

    public Game? FindFirst(FindCriteria c) => CurrentView().FirstOrDefault(g => Matches(g, c));

    /// <summary>Next match after the current game in view order, wrapping around.</summary>
    public Game? FindNext(FindCriteria c, int? currentNumber)
    {
        var view = CurrentView();
        if (view.Count == 0) return null;
        int start = 0;
        if (currentNumber is int n)
        {
            int i = view.FindIndex(g => g.Number == n);
            if (i >= 0) start = (i + 1) % view.Count;
        }
        for (int k = 0; k < view.Count; k++)
        {
            var g = view[(start + k) % view.Count];
            if (Matches(g, c)) return g;
        }
        return null;
    }

    public void ToggleColumn(string tag, bool on)
    {
        if (on)
        {
            if (State.Columns.Any(c => c.Tag == tag)) return;
            var width = AvailableColumns.FirstOrDefault(c => c.Tag == tag).Width;
            State.Columns.Add(new ListColumn(tag, width > 0 ? width : 110));
        }
        else if (State.Columns.Count > 1)
        {
            State.Columns.RemoveAll(c => c.Tag == tag);
        }
    }
}
```

- [ ] **Step 8: Implement `GameClipboard.cs` and `CoverImages.cs`**

```csharp
using System.Text.Json;
using GameCatalog.Core.Models;

namespace GameCatalog.App.Services;

/// <summary>Text format used to copy/paste whole games through the clipboard.</summary>
public static class GameClipboard
{
    public const string Header = "GAMECATALOG1:";

    public static string Serialize(Game game) => Header + JsonSerializer.Serialize(game);

    /// <summary>The pasted game with Number = 0 (a new number is assigned on add), or null.</summary>
    public static Game? TryDeserialize(string? text)
    {
        if (text is null || !text.StartsWith(Header, StringComparison.Ordinal)) return null;
        try
        {
            var game = JsonSerializer.Deserialize<Game>(text[Header.Length..]);
            if (game is null || string.IsNullOrWhiteSpace(game.Title)) return null;
            game.Number = 0;
            return game;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
```

```csharp
using System.Drawing.Imaging;

namespace GameCatalog.App.Services;

/// <summary>Cover image helpers (AMC picture import options).</summary>
public static class CoverImages
{
    /// <summary>Scales the image down to <paramref name="maxWidth"/> (PNG); returns the input when no scaling is needed or possible.</summary>
    public static byte[] Downscale(byte[] bytes, int maxWidth)
    {
        if (maxWidth <= 0) return bytes;
        try
        {
            using var ms = new MemoryStream(bytes);
            using var img = Image.FromStream(ms);
            if (img.Width <= maxWidth) return bytes;
            int h = (int)Math.Round(img.Height * (maxWidth / (double)img.Width));
            using var bmp = new Bitmap(img, new Size(maxWidth, h));
            return ToPng(bmp);
        }
        catch (ArgumentException)
        {
            return bytes;
        }
    }

    public static byte[] ToPng(Image image)
    {
        using var ms = new MemoryStream();
        image.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }

    /// <summary>Decodes an image detached from its stream, or null when the bytes are not an image.</summary>
    public static Image? TryLoad(byte[]? bytes)
    {
        if (bytes is not { Length: > 0 }) return null;
        try
        {
            using var ms = new MemoryStream(bytes);
            using var img = Image.FromStream(ms);
            return new Bitmap(img);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    /// <summary>data: URI for embedding the cover in HTML previews (no temporary files).</summary>
    public static string? ToDataUri(byte[]? bytes)
    {
        if (bytes is not { Length: > 0 }) return null;
        string mime = bytes switch
        {
            [0x89, 0x50, 0x4E, 0x47, ..] => "image/png",
            [0xFF, 0xD8, ..] => "image/jpeg",
            [0x47, 0x49, 0x46, ..] => "image/gif",
            [0x42, 0x4D, ..] => "image/bmp",
            _ => "application/octet-stream",
        };
        return $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
    }
}
```

- [ ] **Step 9: Run tests**

Run: `dotnet test tests/GameCatalog.App.Tests` → Expected: all PASS.
Run: `dotnet build GameCatalog.slnx` → `Build succeeded` (the old `MainForm` still compiles: it only lost the enum, now imported from `GameCatalog.App.Services`).

- [ ] **Step 10: Commit**

```bash
git add -A
git commit -m "feat(app): catalog session, undo, numbering, list presenter, clipboard and cover services

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Designer conventions (apply to Tasks 9–12)

- Create every Form/UserControl with VS "Add → Form/User Control (Windows Forms)" or by hand in the same shape: `X.cs` (`public partial class X : Form|UserControl`, constructor calls `InitializeComponent()`), `X.Designer.cs` (fields + `InitializeComponent` + `Dispose`), `X.resx` only when the Designer generates one.
- Every Form/UserControl gets a `components` container (`private System.ComponentModel.IContainer components = null;`) and a `Localizer` created as `this.localizer = new GameCatalog.UI.Components.Localizer(this.components);` with `this.localizer.ContainerControl = this;`.
- Control names: `lbl` Label, `txt` TextBox, `cbx` ComboBox, `dtp` DateTimePicker, `num` NumericUpDown, `chk` CheckBox, `btn` Button/ToolStripButton, `lv` ListView, `col` ColumnHeader, `tlp` TableLayoutPanel, `pnl` Panel, `ts` ToolStrip, `mnu` ToolStripMenuItem, `tab` TabPage, `act` UIAction. Names are translation keys — never rename one without renaming the JSON keys.
- Designer texts are Polish (the default language). For each form, the JSON snippet in the task is added to `forms` in `pl.json` (Polish) and `en.json` (English) — same keys.
- Layout uses docking/`TableLayoutPanel`; no absolute pixel positioning except inside fixed-size dialogs.
- Logic stays in `X.cs`; `X.Designer.cs` contains only what the Designer would generate.

---

### Task 9: Reusable controls — game list, thumbnails, HTML preview, picture window, find bar

**Files:**
- Create: `src/GameCatalog.UI/Controls/ColorTags.cs`, `GameListView.cs`, `ThumbnailView.cs`, `HtmlPreview.cs` + `.Designer.cs`, `PictureToolWindow.cs` + `.Designer.cs`, `FindBar.cs` + `.Designer.cs`
- Move: `src/GameCatalog.App/StatsCanvas.cs` → `src/GameCatalog.UI/Controls/StatsCanvas.cs` (namespace `GameCatalog.UI.Controls`, English comments)
- Move: `src/GameCatalog.App/HtmlTemplates.cs` → `src/GameCatalog.App/Services/HtmlTemplates.cs` (namespace `GameCatalog.App.Services`; `{Status}` via `CatalogLabels.Current.Status`, ratings via `GameFieldValues.Rating`, template folder `AppPaths.TemplatesDirectory`)
- Modify: `pl.json`, `en.json` (forms `HtmlPreview`, `FindBar`)
- Test: `tests/GameCatalog.UI.Tests/GameListViewTests.cs`, `FindBarTests.cs`, `ColorTagsTests.cs`

**Interfaces:**
- Consumes: `Localizer` (Task 7).
- Produces (`namespace GameCatalog.UI.Controls`):
  - `ColorTags` — `Count`, `ToColor(int tag) : Color` (`Color.Empty` for 0/out of range), `Swatch(int tag) : Bitmap`
  - `GameListItem(int Number, bool Checked, int ColorTag, IReadOnlyList<string> Cells, string GroupKey)`, `GameListGroup(string Key, string Caption)`
  - `GameListView : ListView` — `SetColumns(IEnumerable<(string Tag, int Width)>, Func<string,string> caption)`, `Display(IReadOnlyList<GameListItem>, IReadOnlyList<GameListGroup>)`, `UpdateItem(GameListItem)`, `SelectNumber(int) : bool`, `SelectedNumber : int?`, `StretchFirstColumn()`, `ColumnWidths() : IReadOnlyList<(string Tag, int Width)>`; events `SelectedGameChanged`, `GameCheckedChanged` (`GameCheckedEventArgs(int Number, bool Checked)`), `SortRequested` (`ColumnSortEventArgs(string Tag)`)
  - `ThumbnailView : ListView` — `Placeholder`, `Display(IReadOnlyList<(int Number, string Caption, byte[]? Cover)>)`, `SelectNumber(int)`, `SelectedNumber`, event `SelectedGameChanged`
  - `HtmlPreview : UserControl` — static `UserDataFolder`, `IsAvailable`, `ShowHtmlAsync(string html) : Task`, `Print()`
  - `PictureToolWindow : UserControl` — `Toolbar`, `LoadButton`, `SaveAsButton`, `CopyButton`, `PasteButton`, `DeleteButton`, `ShowCover(byte[]?)`, `CurrentImage : Image?`
  - `FindBar : UserControl` — `SetFields(IEnumerable<(string Tag, string Caption)>, string selectedTag)`, `FieldTag`, `SearchText`, `WholeField`, `ShowResults`, `Reverse`, `FocusText()`; events `CriteriaChanged`, `FindNextRequested`, `CloseRequested`
  - `StatsCanvas` (moved, API unchanged)

- [ ] **Step 1: Write failing tests**

`tests/GameCatalog.UI.Tests/ColorTagsTests.cs`:
```csharp
using GameCatalog.UI.Controls;

namespace GameCatalog.UI.Tests;

public class ColorTagsTests
{
    [Fact]
    public void ToColor_ConvertsAmcBgrValues()
    {
        Assert.Equal(Color.Empty, ColorTags.ToColor(0));
        Assert.Equal(Color.Empty, ColorTags.ToColor(99));
        Assert.Equal(Color.FromArgb(0xED, 0x1C, 0x24), ColorTags.ToColor(1));   // 0x241CED BGR → red
        Assert.Equal(13, ColorTags.Count);
    }
}
```

`tests/GameCatalog.UI.Tests/GameListViewTests.cs`:
```csharp
using GameCatalog.UI.Controls;

namespace GameCatalog.UI.Tests;

public class GameListViewTests
{
    private static GameListView Create()
    {
        var lv = new GameListView();
        lv.SetColumns(new[] { ("Number", 40), ("Title", 200) }, tag => tag == "Title" ? "Tytuł" : "#");
        lv.Display(
            new[]
            {
                new GameListItem(1, true, 0, new[] { "1", "Doom" }, "FPS"),
                new GameListItem(2, false, 1, new[] { "2", "Abe" }, ""),
            },
            new[] { new GameListGroup("FPS", "FPS (1)"), new GameListGroup("", "(brak) (1)") });
        return lv;
    }

    [Fact]
    public void Display_FillsItemsGroupsAndColumns()
    {
        using var lv = Create();
        Assert.Equal(2, lv.Items.Count);
        Assert.Equal(2, lv.Groups.Count);
        Assert.Equal("Tytuł", lv.Columns[1].Text);
        Assert.Equal("FPS (1)", lv.Items[0].Group!.Header);
        Assert.True(lv.Items[0].Checked);
        Assert.Equal(ColorTags.ToColor(1), lv.Items[1].BackColor);
    }

    [Fact]
    public void SelectNumber_SelectsAndRaisesEvent()
    {
        using var lv = Create();
        lv.CreateControl();   // selection events need a window handle
        int raised = 0;
        lv.SelectedGameChanged += (_, _) => raised++;
        Assert.True(lv.SelectNumber(2));
        Assert.Equal(2, lv.SelectedNumber);
        Assert.False(lv.SelectNumber(99));
        Assert.True(raised >= 1);
    }

    [Fact]
    public void Display_DoesNotRaiseCheckedEvents()
    {
        var lv = new GameListView();
        lv.SetColumns(new[] { ("Number", 40) }, t => t);
        int raised = 0;
        lv.GameCheckedChanged += (_, _) => raised++;
        lv.Display(new[] { new GameListItem(1, true, 0, new[] { "1" }, "") }, Array.Empty<GameListGroup>());
        Assert.Equal(0, raised);
    }

    [Fact]
    public void UpdateItem_ReplacesCellsOfMatchingNumber()
    {
        using var lv = Create();
        lv.UpdateItem(new GameListItem(1, false, 0, new[] { "1", "Doom II" }, "FPS"));
        Assert.Equal("Doom II", lv.Items[0].SubItems[1].Text);
        Assert.False(lv.Items[0].Checked);
    }
}
```

`tests/GameCatalog.UI.Tests/FindBarTests.cs`:
```csharp
using GameCatalog.UI.Controls;

namespace GameCatalog.UI.Tests;

public class FindBarTests
{
    [Fact]
    public void Criteria_ReflectControls_AndChangesRaiseEvent()
    {
        using var bar = new FindBar();
        bar.SetFields(new[] { ("Title", "Tytuł"), ("Genre", "Gatunek") }, "Genre");
        int changed = 0;
        bar.CriteriaChanged += (_, _) => changed++;

        bar.SearchText = "rpg";
        bar.WholeField = true;

        Assert.Equal("Genre", bar.FieldTag);
        Assert.Equal("rpg", bar.SearchText);
        Assert.True(bar.WholeField);
        Assert.False(bar.Reverse);
        Assert.True(changed >= 2);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test tests/GameCatalog.UI.Tests` → Expected: build FAIL.

- [ ] **Step 3: Implement `ColorTags.cs`**

```csharp
namespace GameCatalog.UI.Controls;

/// <summary>Color tag palette from AMC movieclass.pas DefaultColorsTag (values are Delphi TColor = $BBGGRR).</summary>
public static class ColorTags
{
    private static readonly int[] Values =
    {
        0xFFFFFF, 0x241CED, 0x277FFF, 0x00F2FF, 0x1DE6B5, 0x4CB122, 0xEEEE1C,
        0xFFB040, 0xFF80A0, 0xE060FF, 0xE00ED0, 0x5786B9, 0x999999,
    };

    public static int Count => Values.Length;

    /// <summary>Row color for a tag; <see cref="Color.Empty"/> for "no tag" (0) or unknown tags.</summary>
    public static Color ToColor(int tag)
    {
        if (tag <= 0 || tag >= Values.Length) return Color.Empty;
        int c = Values[tag];
        return Color.FromArgb(c & 0xFF, (c >> 8) & 0xFF, (c >> 16) & 0xFF);
    }

    /// <summary>16x16 menu swatch of a tag color.</summary>
    public static Bitmap Swatch(int tag)
    {
        var bitmap = new Bitmap(16, 16);
        using var g = Graphics.FromImage(bitmap);
        g.Clear(ToColor(tag) is { IsEmpty: false } c ? c : Color.White);
        g.DrawRectangle(Pens.Black, 0, 0, 15, 15);
        return bitmap;
    }
}
```

- [ ] **Step 4: Implement `GameListView.cs`**

```csharp
namespace GameCatalog.UI.Controls;

public sealed record GameListItem(int Number, bool Checked, int ColorTag, IReadOnlyList<string> Cells, string GroupKey);
public sealed record GameListGroup(string Key, string Caption);
public sealed class GameCheckedEventArgs(int number, bool @checked) : EventArgs
{
    public int Number { get; } = number;
    public bool Checked { get; } = @checked;
}
public sealed class ColumnSortEventArgs(string tag) : EventArgs
{
    public string Tag { get; } = tag;
}

/// <summary>
/// The catalog list (AMC ListView1, a TElTree): details view with check boxes, color tags and
/// groups. It only displays rows computed elsewhere and reports user actions through events.
/// </summary>
public sealed class GameListView : ListView
{
    private bool _filling;

    public GameListView()
    {
        View = View.Details;
        FullRowSelect = true;
        MultiSelect = false;
        CheckBoxes = true;
        HideSelection = false;
        ShowGroups = true;
        DoubleBuffered = true;
    }

    public event EventHandler? SelectedGameChanged;
    public event EventHandler<GameCheckedEventArgs>? GameCheckedChanged;
    public event EventHandler<ColumnSortEventArgs>? SortRequested;

    public int? SelectedNumber => SelectedItems.Count > 0 && SelectedItems[0].Tag is int n ? n : null;

    public void SetColumns(IEnumerable<(string Tag, int Width)> columns, Func<string, string> caption)
    {
        BeginUpdate();
        Columns.Clear();
        foreach (var (tag, width) in columns)
            Columns.Add(new ColumnHeader { Text = caption(tag), Width = width, Tag = tag });
        EndUpdate();
    }

    public IReadOnlyList<(string Tag, int Width)> ColumnWidths() =>
        Columns.Cast<ColumnHeader>().Select(c => ((string)c.Tag!, c.Width)).ToList();

    /// <summary>Replaces all rows; keeps the selected game when it is still present.</summary>
    public void Display(IReadOnlyList<GameListItem> items, IReadOnlyList<GameListGroup> groups)
    {
        int? selected = SelectedNumber;
        _filling = true;
        try
        {
            BeginUpdate();
            Groups.Clear();
            Items.Clear();
            var byKey = new Dictionary<string, ListViewGroup>();
            foreach (var g in groups)
            {
                var lg = new ListViewGroup(g.Caption, HorizontalAlignment.Left) { Tag = g.Key };
                Groups.Add(lg);
                byKey[g.Key] = lg;
            }
            foreach (var item in items)
            {
                var lvi = CreateItem(item);
                if (byKey.TryGetValue(item.GroupKey, out var lg)) lvi.Group = lg;
                Items.Add(lvi);
            }
            EndUpdate();
        }
        finally { _filling = false; }
        if (selected is int n && !SelectNumber(n)) SelectedGameChanged?.Invoke(this, EventArgs.Empty);
    }

    public void UpdateItem(GameListItem item)
    {
        _filling = true;
        try
        {
            foreach (ListViewItem lvi in Items)
            {
                if (lvi.Tag is not int n || n != item.Number) continue;
                for (int i = 0; i < item.Cells.Count && i < lvi.SubItems.Count; i++)
                    lvi.SubItems[i].Text = item.Cells[i];
                lvi.Checked = item.Checked;
                var color = ColorTags.ToColor(item.ColorTag);
                lvi.BackColor = color.IsEmpty ? BackColor : color;
                break;
            }
        }
        finally { _filling = false; }
    }

    public bool SelectNumber(int number)
    {
        foreach (ListViewItem lvi in Items)
        {
            if (lvi.Tag is int n && n == number)
            {
                lvi.Selected = true;
                lvi.Focused = true;
                lvi.EnsureVisible();
                return true;
            }
        }
        return false;
    }

    /// <summary>Widens the first column to fill the remaining width (AMC "Stretch list").</summary>
    public void StretchFirstColumn()
    {
        if (Columns.Count == 0) return;
        int others = Columns.Cast<ColumnHeader>().Skip(1).Sum(c => c.Width);
        Columns[0].Width = Math.Max(40, ClientSize.Width - others - 4);
    }

    private static ListViewItem CreateItem(GameListItem item)
    {
        var lvi = new ListViewItem(item.Cells.Count > 0 ? item.Cells[0] : "") { Tag = item.Number, Checked = item.Checked };
        for (int i = 1; i < item.Cells.Count; i++) lvi.SubItems.Add(item.Cells[i]);
        var color = ColorTags.ToColor(item.ColorTag);
        if (!color.IsEmpty) lvi.BackColor = color;
        return lvi;
    }

    protected override void OnItemChecked(ItemCheckedEventArgs e)
    {
        base.OnItemChecked(e);
        if (!_filling && e.Item.Tag is int n) GameCheckedChanged?.Invoke(this, new GameCheckedEventArgs(n, e.Item.Checked));
    }

    protected override void OnSelectedIndexChanged(EventArgs e)
    {
        base.OnSelectedIndexChanged(e);
        if (!_filling) SelectedGameChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnColumnClick(ColumnClickEventArgs e)
    {
        base.OnColumnClick(e);
        if (Columns[e.Column].Tag is string tag) SortRequested?.Invoke(this, new ColumnSortEventArgs(tag));
    }
}
```
- [ ] **Step 5: Implement `ThumbnailView.cs`**

```csharp
namespace GameCatalog.UI.Controls;

/// <summary>Cover thumbnails of the visible games (AMC ThumbsViewer / rkSmartView).</summary>
public sealed class ThumbnailView : ListView
{
    private readonly ImageList _images = new() { ImageSize = new Size(64, 64), ColorDepth = ColorDepth.Depth32Bit };

    public ThumbnailView()
    {
        View = View.LargeIcon;
        MultiSelect = false;
        HideSelection = false;
        LargeImageList = _images;
        DoubleBuffered = true;
    }

    /// <summary>Image shown for games without a cover.</summary>
    public Image? Placeholder { get; set; }

    public event EventHandler? SelectedGameChanged;

    public int? SelectedNumber => SelectedItems.Count > 0 && SelectedItems[0].Tag is int n ? n : null;

    public void Display(IReadOnlyList<(int Number, string Caption, byte[]? Cover)> items)
    {
        int? selected = SelectedNumber;
        BeginUpdate();
        Items.Clear();
        _images.Images.Clear();
        _images.Images.Add(Placeholder ?? new Bitmap(64, 64));
        foreach (var (number, caption, cover) in items)
        {
            int index = 0;
            if (TryDecode(cover) is { } img)
            {
                _images.Images.Add(img);
                index = _images.Images.Count - 1;
            }
            Items.Add(new ListViewItem(caption, index) { Tag = number });
        }
        EndUpdate();
        if (selected is int n) SelectNumber(n);
    }

    public bool SelectNumber(int number)
    {
        foreach (ListViewItem item in Items)
            if (item.Tag is int n && n == number)
            {
                item.Selected = true;
                item.EnsureVisible();
                return true;
            }
        return false;
    }

    private static Image? TryDecode(byte[]? bytes)
    {
        if (bytes is not { Length: > 0 }) return null;
        try
        {
            using var ms = new MemoryStream(bytes);
            using var img = Image.FromStream(ms);
            return new Bitmap(img);
        }
        catch (ArgumentException) { return null; }
    }

    protected override void OnSelectedIndexChanged(EventArgs e)
    {
        base.OnSelectedIndexChanged(e);
        SelectedGameChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _images.Dispose();
        base.Dispose(disposing);
    }
}
```

- [ ] **Step 6: Implement `HtmlPreview` (UserControl, Designer)**

Designer content (`HtmlPreview.Designer.cs`): `components` container; `webView` (`Microsoft.Web.WebView2.WinForms.WebView2`, `Dock = Fill`); `pnlMissing` (`Panel`, `Dock = Fill`, `Visible = false`, `Padding = 12`) containing `lblMissing` (`Label`, `Dock = Top`, `AutoSize = false`, `Height = 40`, Text `Podgląd HTML wymaga składnika Microsoft Edge WebView2 Runtime, którego nie znaleziono.`) and `lnkInstall` (`LinkLabel`, `Dock = Top`, Text `Pobierz WebView2 Runtime`); `localizer` as in the conventions.

`HtmlPreview.cs`:
```csharp
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Web.WebView2.Core;

namespace GameCatalog.UI.Controls;

/// <summary>
/// HTML preview of the current game (AMC HTMLViewer) on Microsoft Edge WebView2.
/// When the WebView2 runtime is missing a message with a download link is shown instead.
/// </summary>
public partial class HtmlPreview : UserControl
{
    private const string RuntimeDownloadUrl = "https://developer.microsoft.com/microsoft-edge/webview2/";
    private const int NavigateToStringLimit = 1_500_000;
    private Task? _initialization;

    public HtmlPreview()
    {
        InitializeComponent();
        lnkInstall.LinkClicked += (_, _) => OpenExternal(RuntimeDownloadUrl);
    }

    /// <summary>WebView2 profile folder; must be writable (not next to the exe in Program Files).</summary>
    public static string UserDataFolder { get; set; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GameCatalog", "WebView2");

    public bool IsAvailable { get; private set; } = true;

    public async Task ShowHtmlAsync(string html)
    {
        await (_initialization ??= InitializeAsync());
        if (!IsAvailable || webView.CoreWebView2 is null) return;
        if (html.Length < NavigateToStringLimit)
        {
            webView.NavigateToString(html);
        }
        else
        {
            var file = Path.Combine(Path.GetTempPath(), "gamecatalog_preview.html");
            await File.WriteAllTextAsync(file, html);
            webView.CoreWebView2.Navigate(new Uri(file).AbsoluteUri);
        }
    }

    public void Print()
    {
        if (IsAvailable && webView.CoreWebView2 is not null)
            webView.CoreWebView2.ShowPrintUI(CoreWebView2PrintDialogKind.Browser);
    }

    private async Task InitializeAsync()
    {
        try
        {
            var environment = await CoreWebView2Environment.CreateAsync(null, UserDataFolder);
            await webView.EnsureCoreWebView2Async(environment);
            webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
            webView.CoreWebView2.NewWindowRequested += (_, e) =>
            {
                e.Handled = true;
                OpenExternal(e.Uri);
            };
            webView.CoreWebView2.NavigationStarting += (_, e) =>
            {
                if (e.IsUserInitiated && e.Uri.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    e.Cancel = true;
                    OpenExternal(e.Uri);
                }
            };
        }
        catch (Exception ex) when (ex is WebView2RuntimeNotFoundException or COMException or InvalidOperationException)
        {
            IsAvailable = false;
            webView.Visible = false;
            pnlMissing.Visible = true;
        }
    }

    private static void OpenExternal(string url)
    {
        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
        catch (System.ComponentModel.Win32Exception) { /* no browser registered */ }
    }
}
```

Translations — add to `forms` in `pl.json`:
```json
"HtmlPreview": {
  "lblMissing.Text": "Podgląd HTML wymaga składnika Microsoft Edge WebView2 Runtime, którego nie znaleziono.",
  "lnkInstall.Text": "Pobierz WebView2 Runtime"
}
```
and `en.json`:
```json
"HtmlPreview": {
  "lblMissing.Text": "The HTML preview needs the Microsoft Edge WebView2 Runtime, which was not found.",
  "lnkInstall.Text": "Download the WebView2 Runtime"
}
```

- [ ] **Step 7: Implement `PictureToolWindow` (UserControl, Designer)**

Designer content: `tsPicture` (`ToolStrip`, `Dock = Top`, `GripStyle = Hidden`) with `btnPicLoad`, `btnPicSaveAs`, `btnPicCopy`, `btnPicPaste`, `btnPicDelete` (`ToolStripButton`, `DisplayStyle = Image`; no text — the main form binds actions to them); `picCover` (`PictureBox`, `Dock = Fill`, `SizeMode = Zoom`, `BorderStyle = FixedSingle`); `localizer`.

`PictureToolWindow.cs`:
```csharp
namespace GameCatalog.UI.Controls;

/// <summary>Cover panel under the list with its mini toolbar (AMC ToolbarPictureWindow).</summary>
public partial class PictureToolWindow : UserControl
{
    public PictureToolWindow() => InitializeComponent();

    public ToolStrip Toolbar => tsPicture;
    public ToolStripButton LoadButton => btnPicLoad;
    public ToolStripButton SaveAsButton => btnPicSaveAs;
    public ToolStripButton CopyButton => btnPicCopy;
    public ToolStripButton PasteButton => btnPicPaste;
    public ToolStripButton DeleteButton => btnPicDelete;

    public Image? CurrentImage => picCover.Image;

    public void ShowCover(byte[]? bytes)
    {
        var old = picCover.Image;
        picCover.Image = null;
        old?.Dispose();
        if (bytes is not { Length: > 0 }) return;
        try
        {
            using var ms = new MemoryStream(bytes);
            using var img = Image.FromStream(ms);
            picCover.Image = new Bitmap(img);
        }
        catch (ArgumentException) { /* unreadable cover: show nothing */ }
    }
}
```
No translation keys (buttons take texts from actions).

- [ ] **Step 8: Implement `FindBar` (UserControl, Designer)**

Designer content (`Dock = Bottom`, `Height = 58`, `BorderStyle = FixedSingle`): a `tlpFind` (`TableLayoutPanel`, `Dock = Fill`, 6 columns: AutoSize, 100%, AutoSize, AutoSize, AutoSize, AutoSize; 2 rows) holding:

| Row | Col | Control | Text |
|---|---|---|---|
| 0 | 0 | `lblField` Label | `Szukaj w &polu:` |
| 0 | 1 | `cbxField` ComboBox (`DropDownList`, Width 150, Anchor Left) | |
| 0 | 2 | `chkWhole` CheckBox | `Całe pole` |
| 0 | 3 | `chkShow` CheckBox | `Pokaż wyniki` |
| 0 | 4 | `chkReverse` CheckBox | `Odwróć wyniki` |
| 1 | 0 | `lblValue` Label | `Szukana &wartość:` |
| 1 | 1 | `txtValue` TextBox (Dock Fill) | |
| 1 | 2–3 | `btnNext` Button (ColumnSpan 2) | `Znajdź następny` |
| 1 | 4 | `btnHide` Button | `Ukryj` |

`FindBar.cs`:
```csharp
namespace GameCatalog.UI.Controls;

/// <summary>Find toolbar under the main window (AMC ToolbarFind).</summary>
public partial class FindBar : UserControl
{
    private List<string> _tags = new();

    public FindBar()
    {
        InitializeComponent();
        cbxField.SelectedIndexChanged += (_, _) => OnCriteriaChanged();
        txtValue.TextChanged += (_, _) => OnCriteriaChanged();
        chkWhole.CheckedChanged += (_, _) => OnCriteriaChanged();
        chkShow.CheckedChanged += (_, _) => OnCriteriaChanged();
        chkReverse.CheckedChanged += (_, _) => OnCriteriaChanged();
        btnNext.Click += (_, _) => FindNextRequested?.Invoke(this, EventArgs.Empty);
        btnHide.Click += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);
        txtValue.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Escape) CloseRequested?.Invoke(this, EventArgs.Empty);
            else if (e.KeyCode == Keys.Enter) FindNextRequested?.Invoke(this, EventArgs.Empty);
        };
    }

    public event EventHandler? CriteriaChanged;
    public event EventHandler? FindNextRequested;
    public event EventHandler? CloseRequested;

    public string FieldTag => cbxField.SelectedIndex >= 0 ? _tags[cbxField.SelectedIndex] : "Title";
    public string SearchText { get => txtValue.Text.Trim(); set => txtValue.Text = value; }
    public bool WholeField { get => chkWhole.Checked; set => chkWhole.Checked = value; }
    public bool ShowResults { get => chkShow.Checked; set => chkShow.Checked = value; }
    public bool Reverse { get => chkReverse.Checked; set => chkReverse.Checked = value; }

    /// <summary>Fills the field list (captions in the current language) and keeps/sets the selection.</summary>
    public void SetFields(IEnumerable<(string Tag, string Caption)> fields, string selectedTag)
    {
        var list = fields.ToList();
        _tags = list.Select(f => f.Tag).ToList();
        cbxField.BeginUpdate();
        cbxField.Items.Clear();
        foreach (var f in list) cbxField.Items.Add(f.Caption);
        cbxField.EndUpdate();
        cbxField.SelectedIndex = Math.Max(0, _tags.IndexOf(selectedTag));
    }

    public void FocusText()
    {
        txtValue.Focus();
        txtValue.SelectAll();
    }

    private void OnCriteriaChanged() => CriteriaChanged?.Invoke(this, EventArgs.Empty);
}
```

Translations — `pl.json` forms:
```json
"FindBar": {
  "lblField.Text": "Szukaj w &polu:",
  "lblValue.Text": "Szukana &wartość:",
  "chkWhole.Text": "Całe pole",
  "chkShow.Text": "Pokaż wyniki",
  "chkReverse.Text": "Odwróć wyniki",
  "btnNext.Text": "Znajdź następny",
  "btnHide.Text": "Ukryj"
}
```
`en.json` forms:
```json
"FindBar": {
  "lblField.Text": "Search in &field:",
  "lblValue.Text": "Search &value:",
  "chkWhole.Text": "Whole field",
  "chkShow.Text": "Show results",
  "chkReverse.Text": "Reverse results",
  "btnNext.Text": "Find next",
  "btnHide.Text": "Hide"
}
```

- [ ] **Step 9: Move `StatsCanvas` and `HtmlTemplates`**

- `StatsCanvas.cs` → `src/GameCatalog.UI/Controls/StatsCanvas.cs`, namespace `GameCatalog.UI.Controls`, comments in English; `StatsForm` adds `using GameCatalog.UI.Controls;`.
- `HtmlTemplates.cs` → `src/GameCatalog.App/Services/HtmlTemplates.cs`: namespace `GameCatalog.App.Services`; `CustomPath` uses `AppPaths.TemplatesDirectory`; in `Expand` replace `{Status}` with `H(CatalogLabels.Current.Status(g.CompletionStatus))` and ratings with `GameFieldValues.Rating(...)`; parameter `coverFileUri` renamed to `coverUri` (now a data: URI). Template names stay `"Szczegółowy"` / `"Kompaktowy"` (they are file names stored in prefs; translating them is stage 5 export work). The old `MainForm` keeps calling it (update its `using`).

- [ ] **Step 10: Run tests, build, run**

Run: `dotnet test tests/GameCatalog.UI.Tests` → all PASS.
Run: `dotnet test GameCatalog.slnx` → all PASS (incl. `LanguageFilesTests`).
Run app → unchanged behaviour (new controls are not used yet).

- [ ] **Step 11: Commit**

```bash
git add -A
git commit -m "feat(ui): game list, thumbnails, WebView2 HTML preview, picture window and find bar controls

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 10: Game frames — `GameFrame`, `GameCustomFrame`, `GameExtrasFrame`, `GameExtraFrame`, `GameAllFieldsFrame`

**Files:**
- Create: `src/GameCatalog.UI/Frames/IGameFrame.cs`
- Create (each `.cs` + `.Designer.cs`): `src/GameCatalog.UI/Frames/GameFrame`, `GameCustomFrame`, `GameExtrasFrame`, `GameExtraFrame`, `GameAllFieldsFrame`
- Modify: `pl.json`, `en.json` (forms `GameFrame`, `GameExtrasFrame`, `GameExtraFrame`, `GameAllFieldsFrame`; strings `Extras.TitleCount`)
- Test: `tests/GameCatalog.UI.Tests/GameFrameTests.cs`, `GameCustomFrameTests.cs`, `GameExtraFrameTests.cs`

**Interfaces:**
- Consumes: `Game`, `GameExtra`, `CustomFieldDefinition`, `Stores/MediaTypes/Platforms/GameModes/ExtraCategories`, `GameFields`, `GameFieldValues`, `CatalogLabels`, `YesNo` (Task 2); `Localizer` (Task 7).
- Produces (`namespace GameCatalog.UI.Frames`):
  - `FrameModifiedEventArgs(string Tag) : EventArgs`
  - `interface IGameFrame { void Bind(Game? game); void Commit(); event EventHandler<FrameModifiedEventArgs>? Modified; }`
  - `GameFrame : UserControl, IGameFrame` — plus `Commit(string tag)`, `EditorFor(string tag) : Control?`, `SetSuggestions(string tag, IEnumerable<string> values)`, `FocusTitle()`, `RefreshCaptions()`, event `OpenUrlRequested`
  - `GameCustomFrame : UserControl, IGameFrame` — plus `SetDefinitions(IReadOnlyList<CustomFieldDefinition>)`, `EditorFor(string customTag) : Control?`
  - `GameExtrasFrame : UserControl` — `Bind(Game?)`, `SelectedExtra : GameExtra?`, `ShowCategory`, `AddButton`, `EditButton`, `DeleteButton`, `Toolbar`, event `ExtraActivated`, `RefreshCaptions()`
  - `GameExtraFrame : UserControl` — `Bind(GameExtra)`, `Commit()`, `TitleText`, `FocusTitle()`
  - `GameAllFieldsFrame : UserControl` — `Bind(Game?, IReadOnlyList<CustomFieldDefinition>)`

- [ ] **Step 1: Write failing tests**

`tests/GameCatalog.UI.Tests/GameFrameTests.cs`:
```csharp
using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;
using GameCatalog.UI.Frames;

namespace GameCatalog.UI.Tests;

public class GameFrameTests
{
    private static Game Sample() => new()
    {
        Title = "Doom", Developer = "id", UserRating = 85, CriticRating = -1, ReleaseYear = 1993,
        PlaytimeHours = 12.5, LastPlayed = null, CompletionStatus = CompletionStatus.Playing, Checked = true,
    };

    [Fact]
    public void Bind_ThenCommitWithoutEdits_ChangesNothing()
    {
        CatalogLabels.Current = new InvariantCatalogLabels();
        using var f = new GameFrame();
        var g = Sample();
        var modified = new List<string>();
        f.Modified += (_, e) => modified.Add(e.Tag);

        f.Bind(g);
        f.Commit();

        Assert.Empty(modified);
        Assert.Equal(85, g.UserRating);
        Assert.Equal(-1, g.CriticRating);
        Assert.Null(g.LastPlayed);
        Assert.Equal(CompletionStatus.Playing, g.CompletionStatus);
    }

    [Fact]
    public void EditingEditors_WritesBackAndRaisesModified()
    {
        CatalogLabels.Current = new InvariantCatalogLabels();
        using var f = new GameFrame();
        var g = Sample();
        var modified = new List<string>();
        f.Modified += (_, e) => modified.Add(e.Tag);
        f.Bind(g);

        ((TextBox)f.EditorFor("Title")!).Text = "Doom II";
        ((NumericUpDown)f.EditorFor("UserRating")!).Value = 9.0m;
        ((NumericUpDown)f.EditorFor("ReleaseYear")!).Value = 1994;
        ((ComboBox)f.EditorFor("CompletionStatus")!).SelectedIndex = (int)CompletionStatus.Completed;
        f.Commit();

        Assert.Equal("Doom II", g.Title);
        Assert.Equal(90, g.UserRating);
        Assert.Equal(1994, g.ReleaseYear);
        Assert.Equal(CompletionStatus.Completed, g.CompletionStatus);
        Assert.Contains("Title", modified);
        Assert.Contains("UserRating", modified);
    }

    [Fact]
    public void RatingNoneCheckbox_StoresMinusOne()
    {
        using var f = new GameFrame();
        var g = Sample();
        f.Bind(g);
        ((CheckBox)f.EditorFor("UserRatingNone")!).Checked = true;
        f.Commit();
        Assert.Equal(-1, g.UserRating);
    }

    [Fact]
    public void Bind_Null_DisablesFrame()
    {
        using var f = new GameFrame();
        f.Bind(null);
        Assert.False(f.Enabled);
    }

    [Fact]
    public void SetSuggestions_KeepsPresetsAndCurrentText()
    {
        using var f = new GameFrame();
        var g = Sample();
        g.Store = "Steam";
        f.Bind(g);
        f.SetSuggestions("Store", new[] { "itch.io", "Steam" });
        var cbx = (ComboBox)f.EditorFor("Store")!;
        Assert.Contains("itch.io", cbx.Items.Cast<string>());
        Assert.Contains("GOG", cbx.Items.Cast<string>());
        Assert.Equal("Steam", cbx.Text);
    }
}
```

`tests/GameCatalog.UI.Tests/GameCustomFrameTests.cs`:
```csharp
using GameCatalog.Core.Models;
using GameCatalog.UI.Frames;

namespace GameCatalog.UI.Tests;

public class GameCustomFrameTests
{
    [Fact]
    public void YesNoAndTextFields_RoundTrip_LegacyTakReadsAsChecked()
    {
        using var f = new GameCustomFrame();
        f.SetDefinitions(new[]
        {
            new CustomFieldDefinition { Tag = "Owned", Name = "Owned", FieldType = "YesNo" },
            new CustomFieldDefinition { Tag = "Disc", Name = "Disc", FieldType = "Text", DefaultValue = "none" },
        });
        var g = new Game();
        g.CustomValues["Owned"] = "Tak";
        var modified = new List<string>();
        f.Modified += (_, e) => modified.Add(e.Tag);

        f.Bind(g);
        Assert.True(((CheckBox)f.EditorFor("Owned")!).Checked);
        Assert.Equal("none", ((TextBox)f.EditorFor("Disc")!).Text);

        ((CheckBox)f.EditorFor("Owned")!).Checked = false;
        ((TextBox)f.EditorFor("Disc")!).Text = "Blue";
        f.Commit();

        Assert.False(g.CustomValues.ContainsKey("Owned"));
        Assert.Equal("Blue", g.CustomValues["Disc"]);
        Assert.Contains("custom:Disc", modified);
    }
}
```

`tests/GameCatalog.UI.Tests/GameExtraFrameTests.cs`:
```csharp
using GameCatalog.Core.Models;
using GameCatalog.UI.Frames;

namespace GameCatalog.UI.Tests;

public class GameExtraFrameTests
{
    [Fact]
    public void Commit_TrimsAndWritesAllFields()
    {
        using var f = new GameExtraFrame();
        var e = new GameExtra { Title = "A" };
        f.Bind(e);
        f.TitleText = "  Hearts of Stone ";
        f.Commit();
        Assert.Equal("Hearts of Stone", e.Title);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test tests/GameCatalog.UI.Tests` → Expected: build FAIL.

- [ ] **Step 3: Implement `IGameFrame.cs`**

```csharp
using GameCatalog.Core.Models;

namespace GameCatalog.UI.Frames;

public sealed class FrameModifiedEventArgs(string tag) : EventArgs
{
    /// <summary>Field tag that changed ("custom:&lt;tag&gt;" for custom fields).</summary>
    public string Tag { get; } = tag;
}

/// <summary>
/// An editor frame of the game info panel (AMC TMovieFrame family): shows a game and writes
/// edits back to it. Frames never touch the catalog; the host reacts to <see cref="Modified"/>.
/// </summary>
public interface IGameFrame
{
    void Bind(Game? game);
    /// <summary>Writes all pending editor values into the bound game.</summary>
    void Commit();
    event EventHandler<FrameModifiedEventArgs>? Modified;
}
```

- [ ] **Step 4: Build `GameFrame` in the Designer**

Layout source of truth: `GameFieldsControl.Build()` in the current code — reproduce the same visual arrangement (AMC framemovie.dfm: PanelMain / PanelMedia / PanelVideo) with docked `TableLayoutPanel`s, top to bottom, inside a root `pnlScroll` (`Panel`, `Dock = Fill`, `AutoScroll = true`, `BackColor = Window`, `Padding = 8`):

1. `tlpTop` (4 columns: 130 px, 100%, 108 px, 212 px). Rows: `MediaLabel` | `MediaType`; `Store` | `DateAdded`; `Borrower` | `DateFinished`; `Title` | `UserRating`; `LocalizedTitle` | `CriticRating`.
2. `tlpCrew` (3 columns: 130 px, 210 px, 100%). Rows 0–3 col 0–1: `Developer`, `Publisher`, `Designer`, `Composer`; col 2: `lblVoiceActors` above `txtVoiceActors` (multiline, `RowSpan = 4` via a nested `pnlVoiceActors`).
3. `tlpMain` (4 columns: 130 px, 100%, 70 px, 205 px). Rows: `Country` | `ReleaseYear` + `PlaytimeHours` (both in a `flpYearTime` FlowLayoutPanel: `lblReleaseYear`, `numReleaseYear`, `lblPlaytimeHours`, `numPlaytimeHours`, `lblPlaytimeUnit`); `Genre` | `Pegi`; `URL` (editor `ColumnSpan = 3`, `txtURL` + `btnUrl` "…" in `pnlUrl`); `Description` (multiline, height 110, span 3); `Comments` (multiline, height 120, span 3); `lblChecked` | `chkChecked`.
4. `tlpTech` (4 columns: 130 px, 100%, 80 px, 240 px). Rows: `InstallPath` (span 3); `Engine` | `Version`; `GameMode` | `Platform`; `Resolution` | `Framerate`; `AudioLanguages` (span 3); `Subtitles` (span 3); `SizeGb` | `DiskCount`; `LastPlayed` | `CompletionStatus`.

Editors (name, type, settings):

| Tag | Editor | Settings |
|---|---|---|
| MediaLabel, Borrower, Title, LocalizedTitle, Designer, Composer, InstallPath, Version, Resolution, Framerate, AudioLanguages, Subtitles, SizeGb | `txt<Tag>` TextBox | Dock Fill |
| VoiceActors, Description, Comments | `txt<Tag>` TextBox | Multiline, ScrollBars Vertical, Dock Fill |
| URL | `txtURL` TextBox + `btnUrl` Button ("…", Width 30, Dock Right) | |
| Store, MediaType, Developer, Publisher, Country, Genre, Pegi, Engine, GameMode, Platform | `cbx<Tag>` ComboBox | `DropDownStyle = DropDown`; `cbxPegi.Items` = `""`, `PEGI 3`, `PEGI 7`, `PEGI 12`, `PEGI 16`, `PEGI 18` |
| CompletionStatus | `cbxCompletionStatus` ComboBox | `DropDownList` (items filled in code) |
| DateAdded, DateFinished, LastPlayed | `dtp<Tag>` DateTimePicker | `ShowCheckBox = true`, `Format = Short` |
| UserRating, CriticRating | `num<Tag>` NumericUpDown + `lbl<Tag>Scale` ("/ 10") + `chk<Tag>None` ("brak") in `flp<Tag>` | Maximum 10, DecimalPlaces 1, Increment 0.1, Width 70 |
| ReleaseYear | `numReleaseYear` | Minimum 0, Maximum 2100, Width 65 |
| PlaytimeHours | `numPlaytimeHours` | Maximum 100000, DecimalPlaces 1, Width 65 |
| DiskCount | `numDiskCount` | Maximum 100 |
| Checked | `chkChecked` CheckBox ("Zaznaczona") | |

Labels are `lbl<Tag>` with the Polish captions below. Add `localizer`.

Translations (`forms.GameFrame`), Polish / English:

| Key | pl | en |
|---|---|---|
| lblMediaLabel.Text | Etykieta gry: | Game label: |
| lblStore.Text | Sklep / źródło: | Store / source: |
| lblBorrower.Text | Pożyczona komu: | Borrower: |
| lblTitle.Text | Tytuł oryginalny: | Original title: |
| lblLocalizedTitle.Text | Tytuł polski: | Localized title: |
| lblMediaType.Text | Typ nośnika: | Media type: |
| lblDateAdded.Text | Data dodania: | Date added: |
| lblDateFinished.Text | Data ukończenia: | Date finished: |
| lblUserRating.Text | Ocena własna: | User rating: |
| lblCriticRating.Text | Ocena recenzji: | Critic rating: |
| lblUserRatingScale.Text | / 10 | / 10 |
| lblCriticRatingScale.Text | / 10 | / 10 |
| chkUserRatingNone.Text | brak | none |
| chkCriticRatingNone.Text | brak | none |
| lblDeveloper.Text | Developer: | Developer: |
| lblPublisher.Text | Wydawca: | Publisher: |
| lblDesigner.Text | Projektant: | Designer: |
| lblComposer.Text | Kompozytor: | Composer: |
| lblVoiceActors.Text | Aktorzy głosowi: | Voice actors: |
| lblCountry.Text | Kraj: | Country: |
| lblReleaseYear.Text | Rok wyd.: | Year: |
| lblPlaytimeHours.Text | Czas gry: | Playtime: |
| lblPlaytimeUnit.Text | h | h |
| lblGenre.Text | Gatunek: | Genre: |
| lblPegi.Text | PEGI: | PEGI: |
| lblURL.Text | Strona WWW: | Web page: |
| btnUrl.Text | … | … |
| lblDescription.Text | Opis: | Description: |
| lblComments.Text | Komentarze: | Comments: |
| lblChecked.Text | Znacznik: | Mark: |
| chkChecked.Text | Zaznaczona | Checked |
| lblInstallPath.Text | Ścieżka instalacji: | Install path: |
| lblEngine.Text | Silnik: | Engine: |
| lblVersion.Text | Wersja: | Version: |
| lblGameMode.Text | Tryb gry: | Game mode: |
| lblPlatform.Text | Platforma: | Platform: |
| lblResolution.Text | Rozdzielczość: | Resolution: |
| lblFramerate.Text | Klatkaż: | Framerate: |
| lblAudioLanguages.Text | Dubbing: | Audio languages: |
| lblSubtitles.Text | Napisy: | Subtitles: |
| lblSizeGb.Text | Rozmiar (GB): | Size (GB): |
| lblDiskCount.Text | Liczba płyt: | Disc count: |
| lblLastPlayed.Text | Ostatnio grano: | Last played: |
| lblCompletionStatus.Text | Status: | Status: |

- [ ] **Step 5: Implement `GameFrame.cs`**

```csharp
using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;

namespace GameCatalog.UI.Frames;

/// <summary>Fixed game fields editor (AMC framemovie.pas, "Game fields" tab).</summary>
public partial class GameFrame : UserControl, IGameFrame
{
    private readonly Dictionary<string, Control> _editors;
    private readonly Dictionary<string, string[]> _presets;
    private Game? _game;
    private bool _loading;

    public GameFrame()
    {
        InitializeComponent();
        _editors = new Dictionary<string, Control>(StringComparer.Ordinal)
        {
            ["MediaLabel"] = txtMediaLabel, ["MediaType"] = cbxMediaType, ["Store"] = cbxStore,
            ["DateAdded"] = dtpDateAdded, ["Borrower"] = txtBorrower, ["DateFinished"] = dtpDateFinished,
            ["UserRating"] = numUserRating, ["UserRatingNone"] = chkUserRatingNone,
            ["CriticRating"] = numCriticRating, ["CriticRatingNone"] = chkCriticRatingNone,
            ["Title"] = txtTitle, ["LocalizedTitle"] = txtLocalizedTitle, ["Developer"] = cbxDeveloper,
            ["Publisher"] = cbxPublisher, ["Designer"] = txtDesigner, ["Composer"] = txtComposer,
            ["VoiceActors"] = txtVoiceActors, ["Country"] = cbxCountry, ["ReleaseYear"] = numReleaseYear,
            ["PlaytimeHours"] = numPlaytimeHours, ["Genre"] = cbxGenre, ["Pegi"] = cbxPegi, ["URL"] = txtURL,
            ["Description"] = txtDescription, ["Comments"] = txtComments, ["Checked"] = chkChecked,
            ["InstallPath"] = txtInstallPath, ["Engine"] = cbxEngine, ["Version"] = txtVersion,
            ["GameMode"] = cbxGameMode, ["Platform"] = cbxPlatform, ["Resolution"] = txtResolution,
            ["Framerate"] = txtFramerate, ["AudioLanguages"] = txtAudioLanguages, ["Subtitles"] = txtSubtitles,
            ["SizeGb"] = txtSizeGb, ["DiskCount"] = numDiskCount, ["LastPlayed"] = dtpLastPlayed,
            ["CompletionStatus"] = cbxCompletionStatus,
        };
        _presets = new Dictionary<string, string[]>
        {
            ["Store"] = Stores.All, ["MediaType"] = MediaTypes.All, ["Platform"] = Platforms.All,
            ["GameMode"] = GameModes.All, ["Pegi"] = new[] { "", "PEGI 3", "PEGI 7", "PEGI 12", "PEGI 16", "PEGI 18" },
        };
        foreach (var (tag, values) in _presets) SetSuggestions(tag, Array.Empty<string>());

        foreach (var (tag, editor) in _editors)
        {
            string fieldTag = tag.EndsWith("None", StringComparison.Ordinal) ? tag[..^4] : tag;
            switch (editor)
            {
                case CheckBox box:
                    box.CheckedChanged += (_, _) => Commit(fieldTag);
                    break;
                case ComboBox { DropDownStyle: ComboBoxStyle.DropDownList } list:
                    list.SelectionChangeCommitted += (_, _) => Commit(fieldTag);
                    break;
                default:
                    editor.Leave += (_, _) => Commit(fieldTag);
                    break;
            }
        }
        chkUserRatingNone.CheckedChanged += (_, _) => numUserRating.Enabled = !chkUserRatingNone.Checked;
        chkCriticRatingNone.CheckedChanged += (_, _) => numCriticRating.Enabled = !chkCriticRatingNone.Checked;
        btnUrl.Click += (_, _) => OpenUrlRequested?.Invoke(this, EventArgs.Empty);
        RefreshCaptions();
        Enabled = false;
    }

    public event EventHandler<FrameModifiedEventArgs>? Modified;
    public event EventHandler? OpenUrlRequested;

    /// <summary>Editor control of a field tag (also "UserRatingNone"/"CriticRatingNone").</summary>
    public Control? EditorFor(string tag) => _editors.GetValueOrDefault(tag);

    public void FocusTitle()
    {
        txtTitle.Focus();
        txtTitle.SelectAll();
    }

    /// <summary>Re-fills captions that come from the core labels (status list) after a language change.</summary>
    public void RefreshCaptions()
    {
        int index = cbxCompletionStatus.SelectedIndex;
        cbxCompletionStatus.Items.Clear();
        foreach (var s in Enum.GetValues<CompletionStatus>())
            cbxCompletionStatus.Items.Add(CatalogLabels.Current.Status(s));
        cbxCompletionStatus.SelectedIndex = index;
    }

    /// <summary>Drop-down values: presets for the field, then catalog values (AMC lists).</summary>
    public void SetSuggestions(string tag, IEnumerable<string> values)
    {
        if (_editors.GetValueOrDefault(tag) is not ComboBox { DropDownStyle: ComboBoxStyle.DropDown } box) return;
        var text = box.Text;
        var all = _presets.GetValueOrDefault(tag, Array.Empty<string>())
            .Concat(values.Where(v => !string.IsNullOrWhiteSpace(v)).OrderBy(v => v, StringComparer.CurrentCulture))
            .Distinct()
            .ToArray();
        box.BeginUpdate();
        box.Items.Clear();
        box.Items.AddRange(all);
        box.EndUpdate();
        box.Text = text;
    }

    public void Bind(Game? game)
    {
        _loading = true;
        try
        {
            _game = game;
            Enabled = game is not null;
            if (game is null) return;
            foreach (var tag in _editors.Keys)
            {
                if (tag.EndsWith("None", StringComparison.Ordinal)) continue;
                Load(game, tag);
            }
        }
        finally { _loading = false; }
    }

    public void Commit()
    {
        foreach (var tag in _editors.Keys.Where(t => !t.EndsWith("None", StringComparison.Ordinal)))
            Commit(tag);
    }

    public void Commit(string tag)
    {
        if (_loading || _game is null) return;
        if (Store(_game, tag)) Modified?.Invoke(this, new FrameModifiedEventArgs(tag));
    }

    private void Load(Game g, string tag)
    {
        switch (tag)
        {
            case "UserRating": SetRating(numUserRating, chkUserRatingNone, g.UserRating); break;
            case "CriticRating": SetRating(numCriticRating, chkCriticRatingNone, g.CriticRating); break;
            case "ReleaseYear": SetNumber(numReleaseYear, g.ReleaseYear); break;
            case "PlaytimeHours": SetNumber(numPlaytimeHours, (decimal)g.PlaytimeHours); break;
            case "DiskCount": SetNumber(numDiskCount, g.DiskCount); break;
            case "DateAdded": SetDate(dtpDateAdded, g.DateAdded); break;
            case "DateFinished": SetDate(dtpDateFinished, g.DateFinished); break;
            case "LastPlayed": SetDate(dtpLastPlayed, g.LastPlayed); break;
            case "Checked": chkChecked.Checked = g.Checked; break;
            case "CompletionStatus": cbxCompletionStatus.SelectedIndex = (int)g.CompletionStatus; break;
            default: _editors[tag].Text = GameFieldValues.Get(g, tag); break;
        }
    }

    /// <returns>True when the game changed.</returns>
    private bool Store(Game g, string tag)
    {
        switch (tag)
        {
            case "UserRating": return Assign(g.UserRating, GetRating(numUserRating, chkUserRatingNone), v => g.UserRating = v);
            case "CriticRating": return Assign(g.CriticRating, GetRating(numCriticRating, chkCriticRatingNone), v => g.CriticRating = v);
            case "ReleaseYear": return Assign(g.ReleaseYear, (int)numReleaseYear.Value, v => g.ReleaseYear = v);
            case "PlaytimeHours": return Assign(g.PlaytimeHours, (double)numPlaytimeHours.Value, v => g.PlaytimeHours = v);
            case "DiskCount": return Assign(g.DiskCount, (int)numDiskCount.Value, v => g.DiskCount = v);
            case "DateAdded": return Assign(g.DateAdded, GetDate(dtpDateAdded), v => g.DateAdded = v);
            case "DateFinished": return Assign(g.DateFinished, GetDate(dtpDateFinished), v => g.DateFinished = v);
            case "LastPlayed": return Assign(g.LastPlayed, GetDate(dtpLastPlayed), v => g.LastPlayed = v);
            case "Checked": return Assign(g.Checked, chkChecked.Checked, v => g.Checked = v);
            case "CompletionStatus":
                return cbxCompletionStatus.SelectedIndex >= 0 &&
                       Assign(g.CompletionStatus, (CompletionStatus)cbxCompletionStatus.SelectedIndex, v => g.CompletionStatus = v);
            default:
                var text = _editors[tag].Text;
                if (GameFieldValues.Get(g, tag) == text) return false;
                return GameFieldWriter.Set(g, tag, text);
        }
    }

    private static bool Assign<T>(T old, T value, Action<T> apply)
    {
        if (EqualityComparer<T>.Default.Equals(old, value)) return false;
        apply(value);
        return true;
    }

    private static void SetNumber(NumericUpDown n, decimal v) => n.Value = Math.Clamp(v, n.Minimum, n.Maximum);

    private static void SetDate(DateTimePicker p, DateTime? d)
    {
        p.Value = d ?? DateTime.Today;
        p.Checked = d.HasValue;
    }

    private static DateTime? GetDate(DateTimePicker p) => p.Checked ? p.Value.Date : null;

    private static void SetRating(NumericUpDown n, CheckBox none, int rating)
    {
        none.Checked = rating < 0;
        n.Value = rating < 0 ? 0 : Math.Clamp(rating / 10m, 0, 10);
    }

    private static int GetRating(NumericUpDown n, CheckBox none) =>
        none.Checked ? -1 : (int)Math.Round(n.Value * 10, MidpointRounding.AwayFromZero);
}
```
- [ ] **Step 6: Build `GameCustomFrame`**

Designer: root `pnlScroll` (`Dock = Fill`, `AutoScroll = true`, `BackColor = Window`, `Padding = 8`) containing `tlpFields` (`Dock = Top`, `AutoSize = true`, 2 columns: 130 px, 100%); `localizer`. No static texts.

```csharp
using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;

namespace GameCatalog.UI.Frames;

/// <summary>Custom fields editor generated from the catalog's definitions (AMC framemoviecustom.pas).</summary>
public partial class GameCustomFrame : UserControl, IGameFrame
{
    private readonly Dictionary<string, Control> _editors = new(StringComparer.OrdinalIgnoreCase);
    private IReadOnlyList<CustomFieldDefinition> _definitions = Array.Empty<CustomFieldDefinition>();
    private Game? _game;
    private bool _loading;

    public GameCustomFrame()
    {
        InitializeComponent();
        Enabled = false;
    }

    public event EventHandler<FrameModifiedEventArgs>? Modified;

    public Control? EditorFor(string customTag) => _editors.GetValueOrDefault(customTag);

    public void SetDefinitions(IReadOnlyList<CustomFieldDefinition> definitions)
    {
        _definitions = definitions;
        tlpFields.SuspendLayout();
        foreach (Control c in tlpFields.Controls.Cast<Control>().ToList()) c.Dispose();
        tlpFields.Controls.Clear();
        tlpFields.RowStyles.Clear();
        tlpFields.RowCount = 0;
        _editors.Clear();
        foreach (var def in definitions)
        {
            var editor = CreateEditor(def);
            int row = tlpFields.RowCount++;
            tlpFields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpFields.Controls.Add(new Label { Text = def.Name + ":", AutoSize = true, Anchor = AnchorStyles.Left, Padding = new Padding(0, 5, 0, 0) }, 0, row);
            editor.Dock = DockStyle.Fill;
            editor.Margin = new Padding(0, 2, 0, 2);
            tlpFields.Controls.Add(editor, 1, row);
            _editors[def.Tag] = editor;
            string tag = def.Tag;
            if (editor is CheckBox box) box.CheckedChanged += (_, _) => Commit(tag);
            else editor.Leave += (_, _) => Commit(tag);
        }
        tlpFields.ResumeLayout();
        Bind(_game);
    }

    public void Bind(Game? game)
    {
        _loading = true;
        try
        {
            _game = game;
            Enabled = game is not null;
            if (game is null) return;
            foreach (var def in _definitions)
            {
                var value = game.CustomValues.TryGetValue(def.Tag, out var v) ? v : def.DefaultValue;
                switch (_editors[def.Tag])
                {
                    case CheckBox c: c.Checked = YesNo.Parse(value); break;
                    case NumericUpDown n:
                        // Stored invariant; the Polish-only version stored the current culture ("12,5").
                        bool parsed = decimal.TryParse(value, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var d)
                                      || decimal.TryParse(value, out d);
                        n.Value = Math.Clamp(parsed ? d : 0, n.Minimum, n.Maximum);
                        break;
                    case DateTimePicker p:
                        bool has = DateTime.TryParse(value, out var date);
                        p.Value = has ? date : DateTime.Today;
                        p.Checked = has;
                        break;
                    case var other: other.Text = value; break;
                }
            }
        }
        finally { _loading = false; }
    }

    public void Commit()
    {
        foreach (var def in _definitions) Commit(def.Tag);
    }

    private void Commit(string tag)
    {
        if (_loading || _game is null || !_editors.TryGetValue(tag, out var editor)) return;
        string value = editor switch
        {
            CheckBox c => c.Checked ? YesNo.StoredTrue : "",
            NumericUpDown n => n.Value.ToString(System.Globalization.CultureInfo.InvariantCulture),
            DateTimePicker p => p.Checked ? p.Value.ToString("yyyy-MM-dd") : "",
            _ => editor.Text,
        };
        string old = _game.CustomValues.TryGetValue(tag, out var o) ? o : "";
        bool same = editor is CheckBox ? YesNo.Parse(old) == (value.Length > 0) : old == value;
        if (same) return;
        GameFieldWriter.Set(_game, GameFieldWriter.CustomPrefix + tag, value);
        Modified?.Invoke(this, new FrameModifiedEventArgs(GameFieldWriter.CustomPrefix + tag));
    }

    private static Control CreateEditor(CustomFieldDefinition def) => def.FieldType switch
    {
        "Number" => new NumericUpDown { Minimum = -1_000_000, Maximum = 1_000_000, DecimalPlaces = 2 },
        "Date" => new DateTimePicker { ShowCheckBox = true, Format = DateTimePickerFormat.Short },
        "List" => ListEditor(def.ListValues),
        "YesNo" => new CheckBox { Text = def.Name, AutoSize = true },
        _ => new TextBox(),
    };

    private static ComboBox ListEditor(string listValues)
    {
        var box = new ComboBox { DropDownStyle = ComboBoxStyle.DropDown };
        box.Items.AddRange(listValues.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return box;
    }
}
```
In the test, the "Disc" field: bound value is the default `"none"`; changing it to `"Blue"` commits. The "Owned" YES/NO: legacy `"Tak"` reads as checked; unchecking stores `""`, which removes the key.

- [ ] **Step 7: Build `GameExtrasFrame`**

Designer: `lblExtrasTitle` (Label, `Dock = Top`, Height 22, Text `Dodatki`), `tsExtras` (ToolStrip, `Dock = Top`, `GripStyle = Hidden`) with `btnExtraAdd`, `btnExtraEdit`, `btnExtraDelete` (`DisplayStyle = Image`, bound to actions by the main form), `lvExtras` (ListView, `Dock = Fill`, `View = Details`, `FullRowSelect = true`, `HideSelection = false`) with `colExtraTitle` ("Tytuł", 220) and `colExtraCategory` ("Kategoria", 120); `localizer`.

```csharp
using GameCatalog.Core.Models;
using GameCatalog.UI.Components;

namespace GameCatalog.UI.Frames;

/// <summary>Extras/DLC list of the current game (AMC framemovieextras.pas).</summary>
public partial class GameExtrasFrame : UserControl
{
    private Game? _game;
    private bool _showCategory = true;

    public GameExtrasFrame()
    {
        InitializeComponent();
        lvExtras.DoubleClick += (_, _) => ExtraActivated?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? ExtraActivated;

    public ToolStrip Toolbar => tsExtras;
    public ToolStripButton AddButton => btnExtraAdd;
    public ToolStripButton EditButton => btnExtraEdit;
    public ToolStripButton DeleteButton => btnExtraDelete;

    public GameExtra? SelectedExtra => lvExtras.SelectedItems.Count > 0 ? lvExtras.SelectedItems[0].Tag as GameExtra : null;

    public bool ShowCategory
    {
        get => _showCategory;
        set { _showCategory = value; colExtraCategory.Width = value ? 120 : 0; }
    }

    public void Bind(Game? game)
    {
        _game = game;
        lvExtras.BeginUpdate();
        lvExtras.Items.Clear();
        foreach (var extra in game?.Extras ?? Enumerable.Empty<GameExtra>())
        {
            var item = new ListViewItem(extra.Title) { Tag = extra };
            item.SubItems.Add(extra.Category);
            lvExtras.Items.Add(item);
        }
        lvExtras.EndUpdate();
        RefreshCaptions();
    }

    /// <summary>Title with the extras count; call after a language change.</summary>
    public void RefreshCaptions()
    {
        var t = Localizer.Translator;
        lblExtrasTitle.Text = _game is null
            ? t?.Get("Extras.Title") ?? lblExtrasTitle.Text
            : t?.Get("Extras.TitleCount", _game.Extras.Count) ?? $"{lblExtrasTitle.Text} ({_game.Extras.Count})";
    }
}
```
Strings to add — `pl.json`: `"Extras.Title": "Dodatki"`, `"Extras.TitleCount": "Dodatki ({0})"`; `en.json`: `"Extras.Title": "Extras"`, `"Extras.TitleCount": "Extras ({0})"`.
Forms `GameExtrasFrame`: `lblExtrasTitle.Text` = `Dodatki` / `Extras`, `colExtraTitle.Text` = `Tytuł` / `Title`, `colExtraCategory.Text` = `Kategoria` / `Category`. `RefreshCaptions()` overrides the label with the count after the host binds a game.

- [ ] **Step 8: Build `GameExtraFrame`**

Designer: `tlpExtra` (`Dock = Fill`, 2 columns AutoSize/100%, `Padding = 10`), rows: `lblExtraTitle`/`txtExtraTitle`, `lblExtraCategory`/`cbxExtraCategory` (DropDown), `lblExtraTag`/`txtExtraTag`, `lblExtraCreatedBy`/`txtExtraCreatedBy`, `lblExtraUrl`/`txtExtraUrl`, `lblExtraDescription`/`txtExtraDescription` (multiline, Height 80), `lblExtraComments`/`txtExtraComments` (multiline, Height 60); `localizer`.

```csharp
using GameCatalog.Core.Models;

namespace GameCatalog.UI.Frames;

/// <summary>Editor of one extra/DLC (AMC frameextra.pas).</summary>
public partial class GameExtraFrame : UserControl
{
    private GameExtra? _extra;

    public GameExtraFrame()
    {
        InitializeComponent();
        cbxExtraCategory.Items.AddRange(ExtraCategories.All);
    }

    public string TitleText { get => txtExtraTitle.Text; set => txtExtraTitle.Text = value; }

    public void FocusTitle() => txtExtraTitle.Focus();

    public void Bind(GameExtra extra)
    {
        _extra = extra;
        txtExtraTitle.Text = extra.Title;
        cbxExtraCategory.Text = extra.Category;
        txtExtraTag.Text = extra.Tag;
        txtExtraCreatedBy.Text = extra.CreatedBy;
        txtExtraUrl.Text = extra.Url;
        txtExtraDescription.Text = extra.Description;
        txtExtraComments.Text = extra.Comments;
    }

    public void Commit()
    {
        if (_extra is null) return;
        _extra.Title = txtExtraTitle.Text.Trim();
        _extra.Category = cbxExtraCategory.Text.Trim();
        _extra.Tag = txtExtraTag.Text.Trim();
        _extra.CreatedBy = txtExtraCreatedBy.Text.Trim();
        _extra.Url = txtExtraUrl.Text.Trim();
        _extra.Description = txtExtraDescription.Text;
        _extra.Comments = txtExtraComments.Text;
    }
}
```
Forms `GameExtraFrame`: `lblExtraTitle.Text` Tytuł:/Title:, `lblExtraCategory.Text` Kategoria:/Category:, `lblExtraTag.Text` Etykieta:/Label:, `lblExtraCreatedBy.Text` Twórca:/Created by:, `lblExtraUrl.Text` URL:/URL:, `lblExtraDescription.Text` Opis:/Description:, `lblExtraComments.Text` Komentarze:/Comments:.

- [ ] **Step 9: Build `GameAllFieldsFrame`**

Designer: `gridFields` (`DataGridView`, `Dock = Fill`, `ReadOnly = true`, `AllowUserToAddRows = false`, `RowHeadersVisible = false`, `AutoSizeColumnsMode = Fill`) with columns `colField` (HeaderText `Pole`) and `colValue` (`Wartość`); `localizer`.

```csharp
using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;

namespace GameCatalog.UI.Frames;

/// <summary>Read-only list of every field of the current game ("All fields" tab).</summary>
public partial class GameAllFieldsFrame : UserControl
{
    public GameAllFieldsFrame() => InitializeComponent();

    public void Bind(Game? game, IReadOnlyList<CustomFieldDefinition> customFields)
    {
        gridFields.Rows.Clear();
        if (game is null) return;
        foreach (var f in GameFields.All)
            gridFields.Rows.Add(CatalogLabels.Current.Field(f.Tag), GameFieldValues.Get(game, f.Tag));
        foreach (var d in customFields)
            gridFields.Rows.Add(d.Name, game.CustomValues.TryGetValue(d.Tag, out var v) ? v : "");
    }
}
```
Forms `GameAllFieldsFrame`: `colField.HeaderText` Pole/Field, `colValue.HeaderText` Wartość/Value.

- [ ] **Step 10: Run tests, build, check in Designer**

Run: `dotnet test GameCatalog.slnx` → all PASS.
Open each frame in the VS Designer (or `dotnet build` if VS is unavailable) and confirm it renders without designer errors.

- [ ] **Step 11: Commit**

```bash
git add -A
git commit -m "feat(ui): designer-based game, custom fields, extras and all-fields frames

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 11: New `MainForm` in the Designer

The new main window replaces the code-built one. To keep the app runnable during the task, the old form is renamed first and deleted at the end.

**Files:**
- Rename: `src/GameCatalog.App/Forms/MainForm.cs` → `src/GameCatalog.App/Forms/LegacyMainForm.cs` (class `LegacyMainForm`) — Step 1; deleted in Step 12
- Create: `src/GameCatalog.App/MainForm.cs`, `MainForm.Designer.cs`, `MainForm.resx`, `MainForm.File.cs`, `MainForm.Game.cs`, `MainForm.View.cs`, `MainForm.Tools.cs`
- Create: `src/GameCatalog.App/Forms/ChoiceDialog.cs` + `.Designer.cs`
- Modify: `src/GameCatalog.Core/Models/Game.cs` (`CopyFrom`), `src/GameCatalog.Core/Catalog/GameList.cs` (`Remove(Game)`), `src/GameCatalog.App/Services/CatalogSession.cs` (`MarkModified`), `src/GameCatalog.UI/Controls/HtmlPreview.cs` (await navigation), `Program.cs`, `pl.json`, `en.json`
- Delete (Step 12): `Forms/LegacyMainForm.cs`, `Controls/GameFieldsControl.cs`, `Controls/ControlLayout.cs`, `FieldValues.cs`
- Test: `tests/GameCatalog.Core.Tests/GameListTests.cs` (+2 tests), `tests/GameCatalog.App.Tests/CatalogSessionTests.cs` (+1 test)

**Interfaces:**
- Consumes: everything from Tasks 2–10.
- Produces:
  - `Game.CopyFrom(Game source)` — overwrites all fields with deep copies (used by undo)
  - `GameList.Remove(Game game) : bool` — removes this instance only (duplicates by number survive)
  - `CatalogSession.MarkModified()` — sets dirty without raising `Changed` (field edits refresh one row, not the list)
  - `ChoiceDialog(string title, string prompt, IReadOnlyList<string> items)` with `SelectedIndex`
  - `MainForm` members used by Task 15: `_session`, `_presenter`, `_current`, `RefreshList()`, `RebindCurrent()`, `SelectGame(int)`, `ApplyInfoResult(Game, GameInfoResult, string sourceName)`, `ImportEntries()`, `ExportEntries()`, `record ExchangeEntry(string Caption, AmcIcon Icon, Action Run)`, `AddImported(Func<List<Game>>)`, `PickFromList(string, IList<string>)`, `tabsGame`, `menuMain`, `mnuFile`, `mnuGame`, `mnuView`, `mnuTools`, `mnuHelp`, `mnuGameGetInfo`, `toolStripContainer`, `actionList`, `iconThemes`

- [ ] **Step 1: Rename the old window**

Rename class and file `MainForm` → `LegacyMainForm` (`Forms/LegacyMainForm.cs`); `Program.cs` runs `new LegacyMainForm()` until Step 11. Build: `dotnet build GameCatalog.slnx` → succeeds.

- [ ] **Step 2: Write failing tests for the model additions**

Add to `tests/GameCatalog.Core.Tests/GameListTests.cs`:
```csharp
    [Fact]
    public void RemoveByInstance_KeepsDuplicateNumber()
    {
        var list = new GameList();
        var a = new Game { Number = 1, Title = "A" };
        var b = new Game { Number = 1, Title = "B" };
        list.Add(a);
        list.Add(b);
        Assert.True(list.Remove(a));
        Assert.Same(b, Assert.Single(list.Games));
    }

    [Fact]
    public void CopyFrom_OverwritesFieldsWithDeepCopies()
    {
        var source = new Game { Number = 3, Title = "S", UserRating = 50, CoverImage = new byte[] { 1 } };
        source.Extras.Add(new GameExtra { Title = "E" });
        source.CustomValues["K"] = "V";
        var target = new Game { Title = "T" };

        target.CopyFrom(source);
        source.Extras[0].Title = "changed";
        source.CoverImage[0] = 9;
        source.CustomValues["K"] = "changed";

        Assert.Equal(3, target.Number);
        Assert.Equal("S", target.Title);
        Assert.Equal(50, target.UserRating);
        Assert.Equal("E", target.Extras[0].Title);
        Assert.Equal(1, target.CoverImage![0]);
        Assert.Equal("V", target.CustomValues["K"]);
    }
```
Add to `tests/GameCatalog.App.Tests/CatalogSessionTests.cs`:
```csharp
    [Fact]
    public void MarkModified_SetsDirtyWithoutChanged()
    {
        var s = new CatalogSession(new PrefsStore());
        s.New(PathOf("a.gc"));
        int changed = 0;
        s.Changed += (_, _) => changed++;
        s.MarkModified();
        Assert.True(s.IsDirty);
        Assert.Equal(0, changed);
    }
```
Run: `dotnet test GameCatalog.slnx` → Expected: build FAIL (`Remove(Game)`, `CopyFrom`, `MarkModified` missing).

- [ ] **Step 3: Implement the model additions**

`GameList`:
```csharp
    /// <summary>Removes this instance only — other games with the same number stay.</summary>
    public bool Remove(Game game) => _games.Remove(game);
```
`Game`:
```csharp
    /// <summary>Overwrites every field with deep copies of <paramref name="source"/>'s values (used by undo).</summary>
    public void CopyFrom(Game source)
    {
        foreach (var p in typeof(Game).GetProperties())
            if (p.CanRead && p.CanWrite) p.SetValue(this, p.GetValue(source));
        Extras = source.Extras.Select(e => e.Clone()).ToList();
        CustomValues = new Dictionary<string, string>(source.CustomValues, StringComparer.OrdinalIgnoreCase);
        CoverImage = source.CoverImage is null ? null : (byte[])source.CoverImage.Clone();
    }
```
`CatalogSession`:
```csharp
    /// <summary>Marks unsaved changes without asking views to rebuild (single-field edits).</summary>
    public void MarkModified() => SetDirty(true);
```
Run: `dotnet test GameCatalog.slnx` → PASS.

- [ ] **Step 4: Make `HtmlPreview.ShowHtmlAsync` wait for the navigation**

Printing right after showing must print the new page. Replace the navigation part of `ShowHtmlAsync` in `src/GameCatalog.UI/Controls/HtmlPreview.cs`:
```csharp
    public async Task ShowHtmlAsync(string html)
    {
        await (_initialization ??= InitializeAsync());
        if (!IsAvailable || webView.CoreWebView2 is null) return;
        var completed = new TaskCompletionSource();
        void OnCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            webView.NavigationCompleted -= OnCompleted;
            completed.TrySetResult();
        }
        webView.NavigationCompleted += OnCompleted;
        if (html.Length < NavigateToStringLimit)
        {
            webView.NavigateToString(html);
        }
        else
        {
            var file = Path.Combine(Path.GetTempPath(), "gamecatalog_preview.html");
            await File.WriteAllTextAsync(file, html);
            webView.CoreWebView2.Navigate(new Uri(file).AbsoluteUri);
        }
        await completed.Task;
    }
```

- [ ] **Step 5: `ChoiceDialog` (Designer)**

Fixed dialog (`FormBorderStyle = FixedDialog`, `StartPosition = CenterParent`, `MaximizeBox = MinimizeBox = false`, `ShowInTaskbar = false`, ClientSize 380×300, `AcceptButton = btnOk`, `CancelButton = btnCancel`): `lblPrompt` (Dock Top, AutoSize false, Height 24), `lstChoices` (ListBox, Dock Fill, `IntegralHeight = false`), `pnlButtons` (FlowLayoutPanel, Dock Bottom, Height 44, `FlowDirection = RightToLeft`, Padding 8) with `btnCancel` (`Anuluj`, `DialogResult.Cancel`) and `btnOk` (`OK`, `DialogResult.OK`); `localizer`.

```csharp
namespace GameCatalog.App.Forms;

/// <summary>Pick one entry from a list (import/export format, script to run).</summary>
public partial class ChoiceDialog : Form
{
    public ChoiceDialog(string title, string prompt, IReadOnlyList<string> items)
    {
        InitializeComponent();
        Text = title;
        lblPrompt.Text = prompt;
        lstChoices.Items.AddRange(items.Cast<object>().ToArray());
        if (lstChoices.Items.Count > 0) lstChoices.SelectedIndex = 0;
        lstChoices.DoubleClick += (_, _) =>
        {
            DialogResult = DialogResult.OK;
            Close();
        };
    }

    public int SelectedIndex => lstChoices.SelectedIndex;
}
```
The caller sets the caption and the prompt; leave `lblPrompt.Text` empty in the Designer. Translation keys (forms `ChoiceDialog`) are only `btnOk.Text` (OK/OK) and `btnCancel.Text` (Anuluj/Cancel): with no JSON key for `$this.Text` or `lblPrompt.Text`, `Localizer` keeps the values the constructor set (it captures them as the fallback on its first `Apply`).

- [ ] **Step 6: Design `MainForm` — components and actions**

Create `MainForm` (namespace `GameCatalog.App`, file at project root) with `Text = "GameCatalog"`, `ClientSize = 1180×740`, `StartPosition = CenterScreen`, `KeyPreview = true`.

Component tray: `actionList` (`ActionList`), `iconThemes` (`IconThemeImageList`), `localizer` (`Localizer`), `ctxList` (`ContextMenuStrip`), and one `UIAction` per row below (name `act<Key>`, `Key = <Key>`, Polish `Text` as designer fallback, `Icon`, `ShortcutKeys`, `Execute` → `act<Key>_Execute`, `Update` → the handler in the last column when given).

| Key | Shortcut | Icon | Update handler |
|---|---|---|---|
| FileNew | Ctrl+N | FileNew | |
| FileOpen | Ctrl+O | FileOpen | |
| FileSave | Ctrl+S | FileSave | |
| FileSaveAs | Ctrl+Shift+S | FileSaveAs | |
| FilePrint | Ctrl+P | FilePrint | actRequiresGame_Update |
| FileImport | Ctrl+I | FileImport | |
| FileExport | Ctrl+E | FileExport | |
| FileProperties | Alt+Enter | FileProperties | |
| FileExit | Alt+F4 | Exit | |
| GameAdd | Alt+Insert | MovieAdd | |
| GameDelete | Alt+Delete | MovieDelete | actRequiresGame_Update |
| GameFind | Ctrl+F | MovieFind | |
| GameRandom | F8 | Random | |
| GameChangeNumber | F2 | MovieNumber | actRequiresGame_Update |
| GameGoToNumber | — | MovieNumber | |
| GameCopy | Ctrl+Shift+C | MovieCopy | actRequiresGame_Update |
| GamePaste | Ctrl+Shift+V | MoviePaste | |
| GameUndo | Ctrl+Shift+Z | MovieUndo | actGameUndo_Update |
| GameCheckAll | — | None | |
| GameUncheckAll | — | None | |
| PicShow | — | MoviePicture | actPicShow_Update |
| PicLoad | — | PictureOpen | actRequiresGame_Update |
| PicSaveAs | — | PictureSave | actRequiresCover_Update |
| PicCopy | — | PictureCopy | actRequiresCover_Update |
| PicPaste | — | MoviePaste | actRequiresGame_Update |
| PicDelete | — | PictureDelete | actRequiresCover_Update |
| ExtrasShow | Ctrl+Shift+F12 | MovieExtras | |
| ExtraAdd | — | ExtrasAdd | actRequiresGame_Update |
| ExtraEdit | — | ExtrasEdit | actRequiresExtra_Update |
| ExtraDelete | — | ExtrasDel | actRequiresExtra_Update |
| UrlOpen | — | MovieUrl | actRequiresGame_Update |
| UrlCopy | — | MovieCopy | actRequiresGame_Update |
| PathOpen | — | Browse | actRequiresGame_Update |
| PathCopy | — | MovieCopy | actRequiresGame_Update |
| GetInfoFiles | — | MovieImportFiles | |
| GetInfoScript | F6 | MovieImportScript | actRequiresGame_Update |
| SortAscending | — | SortAscend | actSortAscending_Update |
| SortDescending | — | SortDescend | actSortDescending_Update |
| GroupCount | — | GroupsFormat | actGroupCount_Update |
| StretchList | F11 | StretchList | |
| FlatTable | Shift+F4 | GridMode | actFlatTable_Update |
| Thumbnails | F4 | ViewThumbnails | actThumbnails_Update |
| ShowHtml | F12 | Html | actShowHtml_Update |
| HtmlRefresh | — | Refresh | |
| HtmlCopy | — | MovieCopy | actRequiresGame_Update |
| ViewMainToolbar | — | None | actViewMainToolbar_Update |
| ViewStatusBar | — | None | actViewStatusBar_Update |
| ManageFields | — | FieldEdit | |
| ManagePictures | — | MoviePicture | |
| Scripts | Shift+F6 | Scripting | actRequiresGame_Update |
| Renumber | — | Renumber | |
| Loans | F9 | Loans | |
| Stats | — | Statistics | |
| HtmlEditor | Ctrl+F12 | Html | |
| Preferences | F10 | Options | |
| HelpIndex | F1 | Help | |
| HelpAbout | — | About | |
| HelpVersions | — | Help | |

Action texts (add to `strings`; `Action.<Key>` = menu text, `Action.<Key>.Hint` = toolbar hint; Polish designer `Text` = the pl menu text):

| Key | pl text | pl hint | en text | en hint |
|---|---|---|---|---|
| FileNew | &Nowy | Nowy katalog | &New | New catalog |
| FileOpen | &Otwórz… | Otwórz katalog | &Open… | Open a catalog |
| FileSave | &Zapisz | Zapisz katalog | &Save | Save the catalog |
| FileSaveAs | Zapisz &jako… | Zapisz katalog pod inną nazwą | Save &as… | Save the catalog under another name |
| FilePrint | &Drukuj… | Drukuj kartę gry | &Print… | Print the game card |
| FileImport | &Importuj… | Importuj gry | &Import… | Import games |
| FileExport | &Eksportuj… | Eksportuj katalog | &Export… | Export the catalog |
| FileProperties | &Właściwości… | Właściwości katalogu | P&roperties… | Catalog properties |
| FileExit | Za&mknij | Zamknij program | E&xit | Close the program |
| GameAdd | &Dodaj… | Dodaj grę | &Add… | Add a game |
| GameDelete | &Usuń | Usuń grę | &Delete | Delete the game |
| GameFind | &Znajdź… | Znajdź | &Find… | Find |
| GameRandom | &Losowa | Losowa gra | &Random | Random game |
| GameChangeNumber | Zmień &numer… | Zmień numer gry | Change &number… | Change the game number |
| GameGoToNumber | &Idź do numeru… | Idź do numeru | &Go to number… | Go to number |
| GameCopy | &Kopiuj | Kopiuj grę | &Copy | Copy the game |
| GamePaste | W&klej | Wklej grę | &Paste | Paste a game |
| GameUndo | C&ofnij zmiany | Cofnij | &Undo changes | Undo |
| GameCheckAll | Zaznacz wszystkie | Zaznacz wszystkie | Check all | Check all |
| GameUncheckAll | Odznacz wszystkie | Odznacz wszystkie | Uncheck all | Uncheck all |
| PicShow | Po&każ panel obrazków | Panel obrazków | &Show picture panel | Picture panel |
| PicLoad | &Wczytaj… | Wczytaj okładkę | &Load… | Load a cover |
| PicSaveAs | Zapisz &jako… | Zapisz okładkę jako… | Save &as… | Save the cover as… |
| PicCopy | &Kopiuj | Kopiuj okładkę | &Copy | Copy the cover |
| PicPaste | W&klej | Wklej okładkę | &Paste | Paste a cover |
| PicDelete | &Usuń | Usuń okładkę | &Delete | Delete the cover |
| ExtrasShow | &Dodatki | Dodatki | &Extras | Extras |
| ExtraAdd | Dodaj dodatek | Dodaj dodatek | Add an extra | Add an extra |
| ExtraEdit | Edytuj dodatek | Edytuj dodatek | Edit the extra | Edit the extra |
| ExtraDelete | Usuń dodatek | Usuń dodatek | Delete the extra | Delete the extra |
| UrlOpen | &Otwórz | Otwórz stronę WWW | &Open | Open the web page |
| UrlCopy | &Kopiuj | Kopiuj adres | &Copy | Copy the address |
| PathOpen | Otwórz w &eksploratorze | Otwórz w eksploratorze | Open in &Explorer | Open in Explorer |
| PathCopy | &Kopiuj | Kopiuj ścieżkę | &Copy | Copy the path |
| GetInfoFiles | Z &plików… | Pobierz informacje z plików | From &files… | Get information from files |
| GetInfoScript | Ze &skryptu… | Pobierz informacje ze skryptu | From a &script… | Get information from a script |
| SortAscending | Rosnąc&o | Sortuj rosnąco | &Ascending | Sort ascending |
| SortDescending | Malejąc&o | Sortuj malejąco | &Descending | Sort descending |
| GroupCount | Pokaż &liczbę gier | Pokaż liczbę gier w grupach | Show game &count | Show the number of games in groups |
| StretchList | Rozciągnij &listę | Rozciągnij listę | &Stretch list | Stretch the list |
| FlatTable | Pokaż w &formie tabeli | Tabela bez grup | Show as &table | Table without groups |
| Thumbnails | Pokaż jako &miniatury | Miniatury okładek | Show as &thumbnails | Cover thumbnails |
| ShowHtml | Pokaż &HTML | Podgląd HTML | Show &HTML | HTML preview |
| HtmlRefresh | Odśwież | Odśwież podgląd | Refresh | Refresh the preview |
| HtmlCopy | Kopiuj HTML | Kopiuj HTML karty gry | Copy HTML | Copy the game card HTML |
| ViewMainToolbar | Pasek &narzędzi | Pasek narzędzi | &Toolbar | Toolbar |
| ViewStatusBar | Pasek &stanu | Pasek stanu | &Status bar | Status bar |
| ManageFields | &Zarządzanie polami… | Zarządzanie polami | Manage &fields… | Manage fields |
| ManagePictures | Zarządzanie &obrazkami… | Zarządzanie obrazkami | Manage &pictures… | Manage pictures |
| Scripts | &Skrypty… | Skrypty | &Scripts… | Scripts |
| Renumber | Zmień &numerację gier… | Zmień numerację gier | &Renumber games… | Renumber games |
| Loans | &Pożyczone… | Pożyczone | &Loans… | Loans |
| Stats | &Statystyki… | Statystyki | S&tatistics… | Statistics |
| HtmlEditor | &Edytor HTML… | Edytor HTML | &HTML editor… | HTML editor |
| Preferences | &Preferencje… | Preferencje | &Preferences… | Preferences |
| HelpIndex | &Indeks pomocy | Pomoc | Help &index | Help |
| HelpAbout | &O programie… | O programie | &About… | About |
| HelpVersions | &Wersje… | Historia wersji | &Versions… | Version history |

- [ ] **Step 7: Design `MainForm` — controls**

Control tree (top to bottom of `Controls`, docking order as listed):

```
menuMain (MenuStrip, Dock Top, iconThemes.UseThemeIcons = true)
  mnuFile "&Plik": actFileNew, actFileOpen, actFileSave, actFileSaveAs, ─, actFilePrint, actFileImport,
                   actFileExport, actFileProperties, ─, mnuFileRecent "Ostatnie pliki", ─, actFileExit
  mnuGame "&Gra": actGameAdd, actGameDelete, actGameFind, actGameRandom, ─, actGameChangeNumber,
                  mnuGamePicture "&Obrazki" [actPicShow, ─, actPicLoad, actPicSaveAs, actPicCopy, actPicPaste, actPicDelete],
                  actExtrasShow,
                  mnuGameUrl "&URL" [actUrlOpen, actUrlCopy],
                  mnuGamePath "Ście&żka pliku" [actPathOpen, actPathCopy],
                  mnuGameGetInfo "Pobierz &informacje" [actGetInfoFiles, actGetInfoScript],
                  mnuGameWeb "Szukaj w &Internecie" (filled at runtime),
                  ─, actGameCopy, actGamePaste, actGameUndo
  mnuView "&Wyświetlaj": mnuViewSort "Sortuj &wg" [mnuViewSortSeparator ─, actSortAscending, actSortDescending],
                         mnuViewGroup "&Grupuj wg" (runtime),
                         mnuViewGroupsFormat "&Formatowanie grup" [actGroupCount],
                         actStretchList, actFlatTable,
                         mnuViewGridFields "Pola &tabeli" (runtime), actThumbnails, actShowHtml,
                         mnuViewSelectHtml "&Wybierz HTML" (runtime), ─,
                         mnuViewBars "Paski" [actViewMainToolbar, actViewStatusBar]
  mnuTools "Na&rzędzia": actManageFields, actManagePictures, actScripts, ─, actRenumber, actLoans,
                         actStats, actHtmlEditor, ─, actPreferences
  mnuHelp "P&omoc": actHelpIndex, actHelpAbout, actHelpVersions
findBar (FindBar, Dock Bottom, Visible false)
statusMain (StatusStrip, Dock Bottom): stCount (Width 300), stSelected (Width 350), stHint (Spring true)
toolStripContainer (ToolStripContainer, Dock Fill)
  TopToolStripPanel:
    tsMain (ToolStrip, UseThemeIcons, all buttons DisplayStyle Image):
      actFileNew, actFileOpen, actFileSave, actFileSaveAs, actFilePrint,
      tbImport (ToolStripDropDownButton, ToolTip "Import", Image FileImport, runtime items),
      tbExport (ToolStripDropDownButton, ToolTip "Eksport", Image FileExport, runtime items),
      actFileProperties, ─,
      actGameAdd, actGameGoToNumber, actGameDelete, actGameFind, actGameRandom,
      tbPicture (ToolStripDropDownButton "Okładka", Image MoviePicture) [actPicLoad, actPicSaveAs, actPicCopy, actPicPaste, actPicDelete],
      actExtrasShow,
      tbUrl (ToolStripDropDownButton "URL", Image MovieUrl) [actUrlOpen, actUrlCopy], ─,
      actGameCopy, actGamePaste, actGameUndo, ─,
      actSortAscending, actSortDescending,
      tbGroup (ToolStripDropDownButton "Grupuj wg", Image Group, runtime items),
      actStretchList,
      tbGridFields (ToolStripDropDownButton "Pola tabeli", Image Field, runtime items),
      actThumbnails, actShowHtml, ─,
      actLoans, actStats, actRenumber, actScripts, actHtmlEditor, actPreferences, ─, actHelpAbout
  ContentPanel:
    splitMain (SplitContainer, Dock Fill, Orientation Vertical, FixedPanel Panel1, SplitterDistance 300)
      Panel1: splitLeft (SplitContainer, Dock Fill, Orientation Horizontal, FixedPanel Panel2, SplitterDistance ≈ 480)
        Panel1: gameList (GameListView, Dock Fill, ContextMenuStrip ctxList), thumbView (ThumbnailView, Dock Fill, Visible false, ContextMenuStrip ctxList)
        Panel2: pictureWindow (PictureToolWindow, Dock Fill)
      Panel2: tabsGame (TabControl, Dock Fill)
        tabGameFields "Pola gry": gameFrame (GameFrame, Dock Fill)
        tabCustomFields "Pola niestandardowe": customFrame (GameCustomFrame, Dock Fill)
        tabAllFields "Wszystkie pola": allFieldsFrame (GameAllFieldsFrame, Dock Fill)
        tabExtras "Dodatki": extrasFrame (GameExtrasFrame, Dock Fill)
        tabHtml "HTML": htmlPreview (HtmlPreview, Dock Fill),
                        tsHtml (ToolStrip, Dock Top, UseThemeIcons) [actHtmlRefresh, actFilePrint, actHtmlCopy]
ctxList (ContextMenuStrip, UseThemeIcons): actGameAdd, actGameDelete, actGameGoToNumber, ─, actGameCopy, actGamePaste, ─,
        actGameCheckAll, actGameUncheckAll, mnuCtxColorTag "Etykieta koloru" (runtime)
```
Every action-driven item is a `ToolStripMenuItem`/`ToolStripButton` named `mnu<Key>`/`tb<Key>`/`ctx<Key>` with its `Action` extender property set to the action (the Designer writes `this.actionList.SetAction(this.mnuFileNew, this.actFileNew);`).

Translations — `forms.MainForm` (non-action items only):

| Key | pl | en |
|---|---|---|
| $this.Text | GameCatalog | GameCatalog |
| mnuFile.Text | &Plik | &File |
| mnuFileRecent.Text | Ostatnie pliki | Recent files |
| mnuGame.Text | &Gra | &Game |
| mnuGamePicture.Text | &Obrazki | &Pictures |
| mnuGameUrl.Text | &URL | &URL |
| mnuGamePath.Text | Ście&żka pliku | File &path |
| mnuGameGetInfo.Text | Pobierz &informacje | &Get information |
| mnuGameWeb.Text | Szukaj w &Internecie | Search the &web |
| mnuView.Text | &Wyświetlaj | &View |
| mnuViewSort.Text | Sortuj &wg | Sort &by |
| mnuViewGroup.Text | &Grupuj wg | &Group by |
| mnuViewGroupsFormat.Text | &Formatowanie grup | Group &format |
| mnuViewGridFields.Text | Pola &tabeli | Grid &fields |
| mnuViewSelectHtml.Text | &Wybierz HTML | Select &HTML |
| mnuViewBars.Text | Paski | Bars |
| mnuTools.Text | Na&rzędzia | &Tools |
| mnuHelp.Text | P&omoc | &Help |
| tbImport.Text | Import | Import |
| tbExport.Text | Eksport | Export |
| tbPicture.Text | Okładka | Cover |
| tbUrl.Text | URL | URL |
| tbGroup.Text | Grupuj wg | Group by |
| tbGridFields.Text | Pola tabeli | Grid fields |
| mnuCtxColorTag.Text | Etykieta koloru | Color tag |
| tabGameFields.Text | Pola gry | Game fields |
| tabCustomFields.Text | Pola niestandardowe | Custom fields |
| tabAllFields.Text | Wszystkie pola | All fields |
| tabExtras.Text | Dodatki | Extras |
| tabHtml.Text | HTML | HTML |

(The `ToolTipText` of the four drop-down buttons equals their text, so `Localizer` emits no separate key for them.)

Strings used by the code-behind (add to `strings`):

| Key | pl | en |
|---|---|---|
| Main.Caption | GameCatalog - [{0}] | GameCatalog - [{0}] |
| Status.Count | Gier: {0} / {1} | Games: {0} / {1} |
| Status.CountGroups | Gier: {0} / {1}  (grupy: {2}) | Games: {0} / {1}  (groups: {2}) |
| Status.Saved | Zapisano. Gier: {0} | Saved. Games: {0} |
| Status.CoverLoaded | Wczytano okładkę. | Cover loaded. |
| Status.HtmlCopied | Skopiowano HTML karty gry. | Game card HTML copied. |
| Status.GameCopied | Skopiowano „{0}”. | Copied “{0}”. |
| Status.Undone | Cofnięto operację. | Undone. |
| Status.NothingToUndo | Brak operacji do cofnięcia. | Nothing to undo. |
| Status.RandomPicked | Wylosowano grę. | Random game selected. |
| Status.NotFound | Nie znaleziono. | Not found. |
| Status.Found | Znaleziono: #{0} {1} | Found: #{0} {1} |
| Status.NoGameSelected | Brak wybranej gry. | No game selected. |
| Status.ScriptApplied | Zastosowano dane z „{0}”. | Applied data from “{0}”. |
| Status.Exported | Wyeksportowano: {0} | Exported: {0} |
| Msg.SaveBeforeNew | Zapisać zmiany przed utworzeniem nowego katalogu? | Save changes before creating a new catalog? |
| Msg.SaveBeforeOpen | Zapisać zmiany przed otwarciem innego katalogu? | Save changes before opening another catalog? |
| Msg.SaveBeforeClose | Zapisać zmiany przed zamknięciem? | Save changes before closing? |
| Msg.Confirm | Potwierdź | Confirm |
| Msg.ConfirmDelete | Usunąć „{0}”? | Delete “{0}”? |
| Msg.ConfirmRenumber | Przenumerować {0} gier (1…{0}) wg bieżącego sortowania? | Renumber {0} games (1…{0}) in the current sort order? |
| Msg.NoGameWithNumber | Brak gry o numerze {0}. | There is no game number {0}. |
| Msg.ClipboardNoGame | Schowek nie zawiera gry. | The clipboard does not contain a game. |
| Msg.CannotOpenUrl | Nie można otworzyć adresu: {0} | Cannot open the address: {0} |
| Msg.CannotOpenPath | Nie można otworzyć ścieżki: {0} | Cannot open the path: {0} |
| Msg.OpenFailed | Nie można otworzyć katalogu:\n{0} | Cannot open the catalog:\n{0} |
| Msg.SaveFailed | Nie można zapisać katalogu:\n{0} | Cannot save the catalog:\n{0} |
| Msg.ImportedCount | Zaimportowano {0} gier. | Imported {0} games. |
| Msg.ImportError | Błąd importu: {0} | Import failed: {0} |
| Msg.ExportError | Błąd eksportu: {0} | Export failed: {0} |
| Msg.NoScripts | Brak skryptów w folderze Skrypty. | There are no scripts in the Scripts folder. |
| Msg.ScriptFailed | Pobieranie informacji — błąd | Getting information — error |
| Menu.ColorTag | Kolor {0} | Color {0} |
| Game.NewTitle | Nowa gra | New game |
| Number.ChangeTitle | Zmień numer | Change number |
| Exchange.ImportTitle | Importuj | Import |
| Exchange.ExportTitle | Eksportuj | Export |
| Exchange.ChooseFormat | Wybierz format: | Choose a format: |
| Exchange.ImportFolder | Z folderu… | From a folder… |
| Exchange.ImportCsv | Z pliku CSV… | From a CSV file… |
| Exchange.ImportAmc | Z pliku AMC XML… | From an AMC XML file… |
| Exchange.ExportCsv | Do pliku CSV… | To a CSV file… |
| Exchange.ExportHtml | Do pliku HTML… | To an HTML file… |
| Export.HtmlTitle | Katalog gier | Game catalog |
| Script.SelectTitle | Pobierz informacje ze skryptu | Get information from a script |
| Script.SelectPrompt | Wybierz skrypt: | Choose a script: |
| Folder.SelectGames | Wybierz folder z grami (podfolder = gra) | Choose the games folder (subfolder = game) |
| File.DefaultCatalog | catalog.gc | catalog.gc |
| File.DefaultCsv | katalog.csv | catalog.csv |
| File.DefaultHtml | katalog.html | catalog.html |
| File.CoverName | okladka_{0}.png | cover_{0}.png |
| Filter.Catalog | Katalog GameCatalog (*.gc)\|*.gc | GameCatalog catalog (*.gc)\|*.gc |
| Filter.CatalogAll | Katalog GameCatalog (*.gc)\|*.gc\|Wszystkie pliki (*.*)\|*.* | GameCatalog catalog (*.gc)\|*.gc\|All files (*.*)\|*.* |
| Filter.Images | Obrazy (*.jpg;*.jpeg;*.png;*.gif;*.bmp)\|*.jpg;*.jpeg;*.png;*.gif;*.bmp | Images (*.jpg;*.jpeg;*.png;*.gif;*.bmp)\|*.jpg;*.jpeg;*.png;*.gif;*.bmp |
| Filter.ImageSave | PNG (*.png)\|*.png\|JPEG (*.jpg)\|*.jpg | PNG (*.png)\|*.png\|JPEG (*.jpg)\|*.jpg |
| Filter.Csv | CSV (*.csv)\|*.csv | CSV (*.csv)\|*.csv |
| Filter.Html | HTML (*.html)\|*.html | HTML (*.html)\|*.html |
| Filter.AmcXml | Ant Movie Catalog XML (*.xml)\|*.xml | Ant Movie Catalog XML (*.xml)\|*.xml |

(`\|` in the table is a literal `|` in JSON.)

- [ ] **Step 8: Code-behind — `MainForm.cs`**

```csharp
using GameCatalog.App.Services;
using GameCatalog.Core.Models;
using GameCatalog.Localization;
using GameCatalog.UI.Components;
using GameCatalog.UI.Controls;
using GameCatalog.UI.Icons;

namespace GameCatalog.App;

/// <summary>
/// Main window — port of AMC main.pas/main.dfm: menu and dockable toolbars with the original
/// AMC icons, grouped game list with the cover panel, game info tabs (fields, custom fields,
/// all fields, extras, HTML), find bar and status bar. Every command is a UIAction; catalog
/// logic lives in CatalogSession, NumberingService and GameListPresenter.
/// Split into partial files by menu: File, Game, View, Tools.
/// </summary>
public partial class MainForm : Form
{
    private readonly PrefsStore _prefs;
    private readonly CatalogSession _session;
    private readonly NumberingService _numbering;
    private readonly GameListPresenter _presenter;
    private Game? _current;

    private static ITranslator L => AppServices.Languages;

    public MainForm()
    {
        InitializeComponent();
        _prefs = AppServices.Prefs;
        _session = new CatalogSession(_prefs);
        _numbering = new NumberingService(_session);
        _presenter = new GameListPresenter(_session.Games, _prefs);

        iconThemes.Theme = _prefs.Theme;
        iconThemes.SetUseThemeIcons(pictureWindow.Toolbar, true);
        iconThemes.SetUseThemeIcons(extrasFrame.Toolbar, true);
        BindPanelButtons();
        WireEvents();
        ApplyPreferences();
        SetWindowIcon();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        var defaultPath = CatalogSession.DefaultPath();
        var path = _prefs.ReopenLastFile && _prefs.RecentFiles.FirstOrDefault() is { } last && File.Exists(last)
            ? last
            : defaultPath;
        OpenCatalog(path, addSamplesIfNew: path == defaultPath);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!e.Cancel && !ConfirmDiscard("Msg.SaveBeforeClose")) e.Cancel = true;
        base.OnFormClosing(e);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        L.LanguageChanged -= OnLanguageChanged;
        _prefs.Save();
        base.OnFormClosed(e);
    }

    private void BindPanelButtons()
    {
        actionList.SetAction(pictureWindow.LoadButton, actPicLoad);
        actionList.SetAction(pictureWindow.SaveAsButton, actPicSaveAs);
        actionList.SetAction(pictureWindow.CopyButton, actPicCopy);
        actionList.SetAction(pictureWindow.PasteButton, actPicPaste);
        actionList.SetAction(pictureWindow.DeleteButton, actPicDelete);
        actionList.SetAction(extrasFrame.AddButton, actExtraAdd);
        actionList.SetAction(extrasFrame.EditButton, actExtraEdit);
        actionList.SetAction(extrasFrame.DeleteButton, actExtraDelete);
    }

    private void WireEvents()
    {
        _session.Opened += (_, _) => OnCatalogOpened();
        _session.Changed += (_, _) => RefreshList();
        _session.DirtyChanged += (_, _) => UpdateTitle();
        _session.Saving += (_, _) => CommitEditors();

        gameList.SelectedGameChanged += (_, _) => OnSelectionChanged(gameList.SelectedNumber);
        thumbView.SelectedGameChanged += (_, _) => OnSelectionChanged(thumbView.SelectedNumber);
        gameList.GameCheckedChanged += OnGameChecked;
        gameList.SortRequested += (_, e) => SortByColumn(e.Tag);
        gameList.DoubleClick += (_, _) =>
        {
            tabsGame.SelectedTab = tabGameFields;
            gameFrame.FocusTitle();
        };
        gameFrame.Modified += (_, e) => OnGameModified(e.Tag);
        customFrame.Modified += (_, e) => OnGameModified(e.Tag);
        gameFrame.OpenUrlRequested += (_, _) => actUrlOpen.PerformExecute();
        extrasFrame.ExtraActivated += (_, _) => actExtraEdit.PerformExecute();
        tabsGame.SelectedIndexChanged += (_, _) => OnTabChanged();
        findBar.CriteriaChanged += (_, _) => ApplyFind();
        findBar.FindNextRequested += (_, _) => FindNext();
        findBar.CloseRequested += (_, _) => ShowFindBar(false);
        HookDynamicMenus();
        L.LanguageChanged += OnLanguageChanged;
    }

    /// <summary>Asks to save pending changes; false when the user cancelled.</summary>
    private bool ConfirmDiscard(string questionKey)
    {
        CommitEditors();
        if (!_session.IsDirty) return true;
        var answer = MessageBox.Show(this, L.Get(questionKey), L.Get("Common.AppName"),
            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
        return answer switch
        {
            DialogResult.Cancel => false,
            DialogResult.Yes => SaveCatalog(),
            _ => true,
        };
    }

    private void OnCatalogOpened()
    {
        _presenter.State.FindOnly = null;
        customFrame.SetDefinitions(_session.Games.CustomFieldDefs);
        RefreshSuggestions();
        RebuildRecentMenu();
        UpdateTitle();
        RebindCurrent();
    }

    /// <summary>The user selected another game: commit the editors (still bound to the old game), then rebind.</summary>
    private void OnSelectionChanged(int? number)
    {
        CommitEditors();
        _current = number is int n ? _session.Games.Find(n) : null;
        BindFrames();
    }

    /// <summary>Rebinds the frames to the selected game without committing editor values (after undo, paste, apply…).</summary>
    private void RebindCurrent()
    {
        var number = _presenter.State.Thumbnails ? thumbView.SelectedNumber : gameList.SelectedNumber;
        _current = number is int n ? _session.Games.Find(n) : null;
        BindFrames();
    }

    private void BindFrames()
    {
        gameFrame.Bind(_current);
        customFrame.Bind(_current);
        allFieldsFrame.Bind(_current, _session.Games.CustomFieldDefs);
        extrasFrame.Bind(_current);
        pictureWindow.ShowCover(_current?.CoverImage);
        stSelected.Text = _current is null ? "" : $"#{_current.Number} {_current.Title}";
        _ = RefreshHtmlAsync();
    }

    private void OnGameModified(string tag)
    {
        if (_current is null) return;
        _session.MarkModified();
        if (_prefs.ForceFullRefresh) RefreshList(); else UpdateRow(_current);
        allFieldsFrame.Bind(_current, _session.Games.CustomFieldDefs);
        if (tag is "Title" or "LocalizedTitle")
        {
            stSelected.Text = $"#{_current.Number} {_current.Title}";
            _ = RefreshHtmlAsync();
        }
    }

    private void OnGameChecked(object? sender, GameCheckedEventArgs e)
    {
        var game = _session.Games.Find(e.Number);
        if (game is null || game.Checked == e.Checked) return;
        game.Checked = e.Checked;
        _session.MarkModified();
        if (ReferenceEquals(game, _current)) gameFrame.Bind(game);
    }

    private void CommitEditors()
    {
        gameFrame.Commit();
        customFrame.Commit();
    }

    private void UpdateTitle() =>
        Text = L.Get("Main.Caption", Path.GetFileName(_session.FilePath)) + (_session.IsDirty ? " *" : "");

    private void SetHint(string text) => stHint.Text = text;

    private bool RequireCurrent()
    {
        if (_current is not null) return true;
        SetHint(L.Get("Status.NoGameSelected"));
        return false;
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        gameFrame.RefreshCaptions();
        extrasFrame.RefreshCaptions();
        ApplyColumns();
        RefreshList();
        UpdateTitle();
        BindFrames();
    }

    private void SetWindowIcon()
    {
        if (iconThemes.ImageList.Images.Count <= (int)AmcIcon.GridMode) return;
        using var bitmap = new Bitmap(iconThemes.ImageList.Images[(int)AmcIcon.GridMode]);
        Icon = Icon.FromHandle(bitmap.GetHicon());
    }

    // --- Update handlers shared by several actions ---
    private void actRequiresGame_Update(object? sender, EventArgs e) => ((UIAction)sender!).Enabled = _current is not null;
    private void actRequiresCover_Update(object? sender, EventArgs e) => ((UIAction)sender!).Enabled = _current?.HasCover == true;
    private void actRequiresExtra_Update(object? sender, EventArgs e) => ((UIAction)sender!).Enabled = extrasFrame.SelectedExtra is not null;
}
```

- [ ] **Step 9: Code-behind — `MainForm.File.cs`**

```csharp
using GameCatalog.App.Diagnostics;
using GameCatalog.App.Forms;
using GameCatalog.App.Services;
using GameCatalog.Core.Models;
using GameCatalog.Storage;
using GameCatalog.UI.Icons;
using Microsoft.Data.Sqlite;

namespace GameCatalog.App;

/// <summary>File menu: catalog files, printing, import/export.</summary>
public partial class MainForm
{
    /// <summary>One import or export format offered in the menus and the format dialog.</summary>
    private sealed record ExchangeEntry(string Caption, AmcIcon Icon, Action Run);

    private void actFileNew_Execute(object? sender, EventArgs e)
    {
        if (!ConfirmDiscard("Msg.SaveBeforeNew")) return;
        using var dialog = new SaveFileDialog { Filter = L.Get("Filter.Catalog"), FileName = L.Get("File.DefaultCatalog") };
        if (dialog.ShowDialog(this) == DialogResult.OK)
            RunGuarded("Msg.OpenFailed", () => _session.New(dialog.FileName));
    }

    private void actFileOpen_Execute(object? sender, EventArgs e)
    {
        if (!ConfirmDiscard("Msg.SaveBeforeOpen")) return;
        using var dialog = new OpenFileDialog { Filter = L.Get("Filter.CatalogAll") };
        if (dialog.ShowDialog(this) == DialogResult.OK) OpenCatalog(dialog.FileName);
    }

    private void actFileSave_Execute(object? sender, EventArgs e) => SaveCatalog();

    private void actFileSaveAs_Execute(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog { Filter = L.Get("Filter.Catalog"), FileName = Path.GetFileName(_session.FilePath) };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        if (!RunGuarded("Msg.SaveFailed", () => _session.SaveAs(dialog.FileName))) return;
        RebuildRecentMenu();
        UpdateTitle();
        SetHint(L.Get("Status.Saved", _session.Games.Games.Count));
    }

    private async void actFilePrint_Execute(object? sender, EventArgs e)
    {
        if (!RequireCurrent()) return;
        tabsGame.SelectedTab = tabHtml;
        await RefreshHtmlAsync();
        htmlPreview.Print();
    }

    private void actFileImport_Execute(object? sender, EventArgs e) =>
        ChooseExchange(L.Get("Exchange.ImportTitle"), ImportEntries());

    private void actFileExport_Execute(object? sender, EventArgs e) =>
        ChooseExchange(L.Get("Exchange.ExportTitle"), ExportEntries());

    private void actFileProperties_Execute(object? sender, EventArgs e)
    {
        var games = _session.Games.Games;
        using var dialog = new CatalogPropertiesDialog(_session.FilePath, games.Count,
            games.Sum(g => g.Extras.Count), games.Sum(g => g.PlaytimeHours), _session.Loans.Count(l => !l.IsReturned));
        dialog.ShowDialog(this);
    }

    private void actFileExit_Execute(object? sender, EventArgs e) => Close();

    private void OpenCatalog(string path, bool addSamplesIfNew = false) =>
        RunGuarded("Msg.OpenFailed", () => _session.Open(path, addSamplesIfNew));

    private bool SaveCatalog()
    {
        if (!RunGuarded("Msg.SaveFailed", _session.Save)) return false;
        SetHint(L.Get("Status.Saved", _session.Games.Games.Count));
        return true;
    }

    /// <summary>Runs a file operation; I/O and database errors are logged and shown instead of crashing.</summary>
    private bool RunGuarded(string messageKey, Action action)
    {
        try
        {
            action();
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SqliteException or InvalidOperationException)
        {
            AppLog.Error(ex, messageKey);
            MessageBox.Show(this, L.Get(messageKey, ex.Message), L.Get("Common.Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    private void RebuildRecentMenu()
    {
        mnuFileRecent.DropDownItems.Clear();
        var files = _prefs.RecentFiles.Where(File.Exists).ToList();
        if (files.Count == 0)
        {
            mnuFileRecent.DropDownItems.Add(new ToolStripMenuItem(L.Get("Common.None")) { Enabled = false });
            return;
        }
        for (int i = 0; i < files.Count; i++)
        {
            string path = files[i];
            var item = new ToolStripMenuItem($"&{i + 1}: {path}");
            item.Click += (_, _) =>
            {
                if (ConfirmDiscard("Msg.SaveBeforeOpen")) OpenCatalog(path);
            };
            mnuFileRecent.DropDownItems.Add(item);
        }
    }

    // --- Import / export (built-in formats; plugin formats are merged in Task 15) ---

    private IReadOnlyList<ExchangeEntry> ImportEntries() => new[]
    {
        new ExchangeEntry(L.Get("Exchange.ImportFolder"), AmcIcon.MovieImportFiles, ImportFromFolder),
        new ExchangeEntry(L.Get("Exchange.ImportCsv"), AmcIcon.FileImport, ImportFromCsv),
        new ExchangeEntry(L.Get("Exchange.ImportAmc"), AmcIcon.MovieImport, ImportFromAmc),
    };

    private IReadOnlyList<ExchangeEntry> ExportEntries() => new[]
    {
        new ExchangeEntry(L.Get("Exchange.ExportCsv"), AmcIcon.FileExport, ExportToCsv),
        new ExchangeEntry(L.Get("Exchange.ExportHtml"), AmcIcon.Html, ExportToHtml),
    };

    private void ChooseExchange(string title, IReadOnlyList<ExchangeEntry> entries)
    {
        using var dialog = new ChoiceDialog(title, L.Get("Exchange.ChooseFormat"), entries.Select(e => e.Caption).ToList());
        if (dialog.ShowDialog(this) == DialogResult.OK && dialog.SelectedIndex >= 0) entries[dialog.SelectedIndex].Run();
    }

    private static void FillExchangeMenu(ToolStripDropDownItem menu, IReadOnlyList<ExchangeEntry> entries)
    {
        menu.DropDownItems.Clear();
        foreach (var entry in entries)
        {
            var item = new ToolStripMenuItem(entry.Caption) { ImageIndex = (int)entry.Icon };
            item.Click += (_, _) => entry.Run();
            menu.DropDownItems.Add(item);
        }
    }

    private void ImportFromFolder()
    {
        using var dialog = new FolderBrowserDialog { Description = L.Get("Folder.SelectGames"), UseDescriptionForTitle = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        AddImported(() => CatalogTransfer.ImportFromFolder(dialog.SelectedPath, _session.Games.NextNumber(), _prefs.MediaPatterns));
    }

    private void ImportFromCsv()
    {
        using var dialog = new OpenFileDialog { Filter = L.Get("Filter.Csv") };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        AddImported(() => CatalogTransfer.ImportCsv(dialog.FileName, _session.Games.NextNumber(), _prefs.CsvSeparator()));
    }

    private void ImportFromAmc()
    {
        using var dialog = new OpenFileDialog { Filter = L.Get("Filter.AmcXml") };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        AddImported(() => AmcXmlImporter.Import(dialog.FileName, _session.Games.NextNumber()));
    }

    /// <summary>Adds imported games as one undoable step.</summary>
    private void AddImported(Func<List<Game>> import)
    {
        List<Game> games;
        try
        {
            games = import();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Xml.XmlException or FormatException)
        {
            AppLog.Error(ex, "Import");
            MessageBox.Show(this, L.Get("Msg.ImportError", ex.Message), L.Get("Common.Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        foreach (var g in games) _session.Games.Add(g);
        _session.Undo.Push(() =>
        {
            foreach (var g in games) _session.Games.Remove(g);
            _session.MarkDirty();
        });
        _session.MarkDirty();
        RefreshSuggestions();
        MessageBox.Show(this, L.Get("Msg.ImportedCount", games.Count), L.Get("Exchange.ImportTitle"));
    }

    private void ExportToCsv() => ExportTo(L.Get("Filter.Csv"), L.Get("File.DefaultCsv"),
        path => CatalogTransfer.ExportCsv(_presenter.CurrentView(), path, _prefs.CsvSeparator()));

    private void ExportToHtml() => ExportTo(L.Get("Filter.Html"), L.Get("File.DefaultHtml"),
        path => CatalogTransfer.ExportHtml(_presenter.CurrentView(), path, L.Get("Export.HtmlTitle")));

    private void ExportTo(string filter, string fileName, Action<string> export)
    {
        using var dialog = new SaveFileDialog { Filter = filter, FileName = fileName };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            export(dialog.FileName);
            SetHint(L.Get("Status.Exported", dialog.FileName));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            AppLog.Error(ex, "Export");
            MessageBox.Show(this, L.Get("Msg.ExportError", ex.Message), L.Get("Common.Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
```

- [ ] **Step 10: Code-behind — `MainForm.Game.cs`, `MainForm.View.cs`, `MainForm.Tools.cs`**

`MainForm.Game.cs`:
```csharp
using System.ComponentModel;
using System.Diagnostics;
using GameCatalog.App.Forms;
using GameCatalog.App.Services;
using GameCatalog.Core.Models;
using GameCatalog.UI.Components;
using GameCatalog.UI.Controls;

namespace GameCatalog.App;

/// <summary>Game menu: add/delete/number, clipboard, undo, pictures, extras, URL/path, web search.</summary>
public partial class MainForm
{
    private void actGameAdd_Execute(object? sender, EventArgs e)
    {
        CommitEditors();
        var game = new Game { Title = L.Get("Game.NewTitle"), DateAdded = DateTime.Today };
        if (!_prefs.AutoNumbering)
        {
            // AMC mode: ask for the number ("Movie number" dialog).
            int proposed = _session.Games.NextNumber();
            using var dialog = new NumberConflictDialog(proposed, proposed,
                n => _session.Games.Find(n) is not null, n => _session.Games.Find(n)?.DisplayTitle);
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            if (dialog.DontAskAgain)
            {
                _prefs.AutoNumbering = true;
                _prefs.Save();
            }
            game.Number = _numbering.ResolveNewNumber(dialog.ChosenNumber, dialog.Resolution);
        }
        _session.Games.Add(game);
        _session.Undo.Push(() =>
        {
            _session.Games.Remove(game);
            _session.MarkDirty();
        });
        _session.MarkDirty();
        SelectGame(game.Number);
        tabsGame.SelectedTab = tabGameFields;
        gameFrame.FocusTitle();
    }

    private void actGameDelete_Execute(object? sender, EventArgs e)
    {
        // Like AMC, the delete shortcut is ignored while the user is typing in an editor.
        if (gameFrame.ContainsFocus || customFrame.ContainsFocus || findBar.ContainsFocus) return;
        if (_current is not { } game) return;
        if (MessageBox.Show(this, L.Get("Msg.ConfirmDelete", game.Title), L.Get("Msg.Confirm"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        _session.Games.Remove(game);
        _session.Undo.Push(() =>
        {
            _session.Games.Add(game);
            _session.MarkDirty();
            SelectGame(game.Number);
        });
        _session.MarkDirty();
    }

    private void actGameFind_Execute(object? sender, EventArgs e) => ShowFindBar(!findBar.Visible);

    private void actGameRandom_Execute(object? sender, EventArgs e)
    {
        var view = _presenter.CurrentView();
        if (view.Count == 0) return;
        SelectGame(view[Random.Shared.Next(view.Count)].Number);
        SetHint(L.Get("Status.RandomPicked"));
    }

    private void actGameGoToNumber_Execute(object? sender, EventArgs e)
    {
        int max = Math.Max(1, _session.Games.Games.Count == 0 ? 1 : _session.Games.Games.Max(g => g.Number));
        using var dialog = new NumberDialog(_current?.Number ?? 1, max);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        if (_session.Games.Find(dialog.Value) is null)
        {
            MessageBox.Show(this, L.Get("Msg.NoGameWithNumber", dialog.Value), L.Get("Common.AppName"));
            return;
        }
        _presenter.State.FindOnly = null;
        RefreshList();
        SelectGame(dialog.Value);
    }

    private void actGameChangeNumber_Execute(object? sender, EventArgs e)
    {
        if (_current is not { } game) return;
        using var dialog = new NumberDialog(game.Number, Math.Max(_session.Games.NextNumber(), game.Number)) { Text = L.Get("Number.ChangeTitle") };
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Value == game.Number) return;
        int target = dialog.Value;
        var resolution = NumberConflictResolution.RenumberExisting;
        if (_session.Games.Find(target) is not null)
        {
            // Conflict like in AMC: show the "Game number" dialog.
            using var conflict = new NumberConflictDialog(target, Math.Max(_session.Games.NextNumber(), target),
                n => n != game.Number && _session.Games.Find(n) is not null,
                n => n != game.Number ? _session.Games.Find(n)?.DisplayTitle : null);
            if (conflict.ShowDialog(this) != DialogResult.OK) return;
            if (conflict.DontAskAgain)
            {
                _prefs.AutoNumbering = true;
                _prefs.Save();
            }
            target = conflict.ChosenNumber;
            resolution = conflict.Resolution;
        }
        if (_numbering.ChangeNumber(game, target, resolution) is int assigned) SelectGame(assigned);
    }

    private void actGameCopy_Execute(object? sender, EventArgs e)
    {
        if (_current is not { } game) return;
        CommitEditors();
        Clipboard.SetText(GameClipboard.Serialize(game));
        SetHint(L.Get("Status.GameCopied", game.Title));
    }

    private void actGamePaste_Execute(object? sender, EventArgs e)
    {
        var game = GameClipboard.TryDeserialize(Clipboard.ContainsText() ? Clipboard.GetText() : null);
        if (game is null)
        {
            MessageBox.Show(this, L.Get("Msg.ClipboardNoGame"), L.Get("Common.AppName"));
            return;
        }
        _session.Games.Add(game);
        _session.Undo.Push(() =>
        {
            _session.Games.Remove(game);
            _session.MarkDirty();
        });
        _session.MarkDirty();
        SelectGame(game.Number);
    }

    private void actGameUndo_Execute(object? sender, EventArgs e)
    {
        CommitEditors();
        SetHint(L.Get(_session.Undo.TryUndo() ? "Status.Undone" : "Status.NothingToUndo"));
        RebindCurrent();
    }

    private void actGameUndo_Update(object? sender, EventArgs e) => actGameUndo.Enabled = _session.Undo.Count > 0;

    private void actGameCheckAll_Execute(object? sender, EventArgs e) => SetAllChecked(true);
    private void actGameUncheckAll_Execute(object? sender, EventArgs e) => SetAllChecked(false);

    private void SetAllChecked(bool value)
    {
        foreach (var g in _presenter.CurrentView()) g.Checked = value;
        _session.MarkDirty();
        RebindCurrent();
    }

    private void SetColorTag(int tag)
    {
        if (_current is not { } game) return;
        game.ColorTag = tag;
        _session.MarkModified();
        UpdateRow(game);
    }

    // --- Pictures (AMC PanelPicture + ToolbarPicture) ---

    private void actPicShow_Execute(object? sender, EventArgs e)
    {
        _prefs.CoverVisible = !_prefs.CoverVisible;
        _prefs.Save();
        ApplyCoverVisibility();
    }

    private void actPicShow_Update(object? sender, EventArgs e) => actPicShow.Checked = _prefs.CoverVisible;

    private void ApplyCoverVisibility() => splitLeft.Panel2Collapsed = !_prefs.CoverVisible;

    private void actPicLoad_Execute(object? sender, EventArgs e)
    {
        if (!RequireCurrent()) return;
        using var dialog = new OpenFileDialog { Filter = L.Get("Filter.Images") };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        byte[] bytes;
        try
        {
            bytes = CoverImages.Downscale(File.ReadAllBytes(dialog.FileName), _prefs.MaxCoverWidth);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, ex.Message, L.Get("Common.Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        SetCover(bytes);
        SetHint(L.Get("Status.CoverLoaded"));
    }

    private void actPicSaveAs_Execute(object? sender, EventArgs e)
    {
        if (_current?.CoverImage is not { } cover) return;
        using var dialog = new SaveFileDialog { Filter = L.Get("Filter.ImageSave"), FileName = L.Get("File.CoverName", _current.Number) };
        if (dialog.ShowDialog(this) == DialogResult.OK) File.WriteAllBytes(dialog.FileName, cover);
    }

    private void actPicCopy_Execute(object? sender, EventArgs e)
    {
        if (pictureWindow.CurrentImage is { } image) Clipboard.SetImage(image);
    }

    private void actPicPaste_Execute(object? sender, EventArgs e)
    {
        if (_current is null || !Clipboard.ContainsImage()) return;
        using var image = Clipboard.GetImage();
        if (image is not null) SetCover(CoverImages.ToPng(image));
    }

    private void actPicDelete_Execute(object? sender, EventArgs e)
    {
        if (_current?.HasCover == true) SetCover(null);
    }

    private void SetCover(byte[]? bytes)
    {
        var game = _current!;
        var old = game.CoverImage;
        game.CoverImage = bytes;
        _session.Undo.Push(() =>
        {
            game.CoverImage = old;
            _session.MarkModified();
            RebindCurrent();
        });
        _session.MarkModified();
        pictureWindow.ShowCover(bytes);
        _ = RefreshHtmlAsync();
        if (_presenter.State.Thumbnails) RefreshList();
    }

    // --- Extras (AMC extras tab) ---

    private void actExtrasShow_Execute(object? sender, EventArgs e) => tabsGame.SelectedTab = tabExtras;

    private void actExtraAdd_Execute(object? sender, EventArgs e)
    {
        if (_current is not { } game) return;
        var extra = new GameExtra { GameNumber = game.Number, Category = ExtraCategories.All[0] };
        using var form = new ExtraEditForm(extra);
        if (form.ShowDialog(this) != DialogResult.OK) return;
        game.Extras.Add(extra);
        _session.Undo.Push(() =>
        {
            game.Extras.Remove(extra);
            _session.MarkModified();
            RebindCurrent();
        });
        ExtrasChanged();
    }

    private void actExtraEdit_Execute(object? sender, EventArgs e)
    {
        if (extrasFrame.SelectedExtra is not { } extra) return;
        var backup = extra.Clone();
        using var form = new ExtraEditForm(extra);
        if (form.ShowDialog(this) != DialogResult.OK) return;
        _session.Undo.Push(() =>
        {
            CopyExtra(backup, extra);
            _session.MarkModified();
            RebindCurrent();
        });
        ExtrasChanged();
    }

    private void actExtraDelete_Execute(object? sender, EventArgs e)
    {
        if (_current is not { } game || extrasFrame.SelectedExtra is not { } extra) return;
        int index = game.Extras.IndexOf(extra);
        game.Extras.Remove(extra);
        _session.Undo.Push(() =>
        {
            game.Extras.Insert(Math.Clamp(index, 0, game.Extras.Count), extra);
            _session.MarkModified();
            RebindCurrent();
        });
        ExtrasChanged();
    }

    private void ExtrasChanged()
    {
        _session.MarkModified();
        extrasFrame.Bind(_current);
        allFieldsFrame.Bind(_current, _session.Games.CustomFieldDefs);
        if (_current is not null) UpdateRow(_current);
    }

    private static void CopyExtra(GameExtra from, GameExtra to)
    {
        to.Title = from.Title;
        to.Category = from.Category;
        to.Tag = from.Tag;
        to.CreatedBy = from.CreatedBy;
        to.Url = from.Url;
        to.Description = from.Description;
        to.Comments = from.Comments;
        to.Checked = from.Checked;
    }

    // --- URL / path / web search ---

    private void actUrlOpen_Execute(object? sender, EventArgs e) => OpenExternal(_current?.Url, "Msg.CannotOpenUrl");

    private void actUrlCopy_Execute(object? sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_current?.Url)) Clipboard.SetText(_current.Url);
    }

    private void actPathOpen_Execute(object? sender, EventArgs e)
    {
        var path = _current?.InstallPath;
        if (string.IsNullOrWhiteSpace(path)) return;
        string target = Directory.Exists(path) ? path : File.Exists(path) ? Path.GetDirectoryName(path)! : path;
        try { Process.Start(new ProcessStartInfo("explorer", $"\"{target}\"") { UseShellExecute = true }); }
        catch (Win32Exception ex) { MessageBox.Show(this, L.Get("Msg.CannotOpenPath", ex.Message)); }
    }

    private void actPathCopy_Execute(object? sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_current?.InstallPath)) Clipboard.SetText(_current.InstallPath);
    }

    private void actGetInfoFiles_Execute(object? sender, EventArgs e) => ImportFromFolder();

    private void WebSearch(SearchSite site)
    {
        var title = _current?.Title?.Trim();
        if (string.IsNullOrEmpty(title))
        {
            SetHint(L.Get("Status.NoGameSelected"));
            return;
        }
        string q = Uri.EscapeDataString(title);
        OpenExternal(site.Url.Contains("{q}") ? site.Url.Replace("{q}", q) : site.Url + q, "Msg.CannotOpenUrl");
    }

    private void OpenExternal(string? target, string errorKey)
    {
        if (string.IsNullOrWhiteSpace(target)) return;
        try { Process.Start(new ProcessStartInfo(target) { UseShellExecute = true }); }
        catch (Win32Exception ex) { MessageBox.Show(this, L.Get(errorKey, ex.Message)); }
    }
}
```

`MainForm.View.cs`:
```csharp
using GameCatalog.App.Services;
using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;
using GameCatalog.Scripting;
using GameCatalog.UI.Controls;
using GameCatalog.UI.Icons;

namespace GameCatalog.App;

/// <summary>View menu: list, sorting, grouping, columns, thumbnails, HTML preview, find bar, bars.</summary>
public partial class MainForm
{
    /// <summary>Marks menu items generated at runtime so they can be replaced.</summary>
    private const string DynamicItem = "dynamic";

    private void RefreshList()
    {
        var vm = _presenter.Build();
        if (_presenter.State.Thumbnails)
            thumbView.Display(vm.Rows.Select(r => (r.Game.Number, _presenter.FormatTitle(r.Game), r.Game.CoverImage)).ToList());
        else
            gameList.Display(vm.Rows.Select(ToItem).ToList(), vm.Groups.Select(g => new GameListGroup(g.Key, g.Caption)).ToList());
        stCount.Text = _presenter.State.GroupTag.Length == 0
            ? L.Get("Status.Count", vm.Rows.Count, vm.Total)
            : L.Get("Status.CountGroups", vm.Rows.Count, vm.Total, vm.Groups.Count);
    }

    private static GameListItem ToItem(GameRow row) =>
        new(row.Game.Number, row.Game.Checked, row.Game.ColorTag, row.Cells, row.GroupKey);

    private void UpdateRow(Game game) =>
        gameList.UpdateItem(new GameListItem(game.Number, game.Checked, game.ColorTag,
            _presenter.State.Columns.Select(c => _presenter.CellText(game, c.Tag)).ToList(), ""));

    private void SelectGame(int number)
    {
        if (_presenter.State.Thumbnails) thumbView.SelectNumber(number);
        else gameList.SelectNumber(number);
    }

    private void ApplyColumns() =>
        gameList.SetColumns(_presenter.State.Columns.Select(c => (c.Tag, c.Width)), CatalogLabels.Current.Field);

    /// <summary>Copies widths the user dragged in the list back into the view state.</summary>
    private void SyncColumnWidths()
    {
        foreach (var (tag, width) in gameList.ColumnWidths())
            if (_presenter.State.Columns.FirstOrDefault(c => c.Tag == tag) is { } column) column.Width = width;
    }

    private void ApplyPreferences()
    {
        gameList.FullRowSelect = _prefs.FullRowSelect;
        gameList.GridLines = _prefs.ShowGridLines;
        extrasFrame.ShowCategory = _prefs.ShowExtraCategory;
        if (iconThemes.Theme != _prefs.Theme) iconThemes.Theme = _prefs.Theme;
        iconThemes.SetUseThemeIcons(menuMain, _prefs.ShowMenuIcons);
        if (!_prefs.ShowMenuIcons) ClearImageLists(menuMain.Items);
        ScriptRunner.Timeout = TimeSpan.FromSeconds(Math.Max(5, _prefs.ScriptTimeoutSec));
        ApplyCoverVisibility();
        ApplyColumns();
        RefreshSuggestions();
    }

    private static void ClearImageLists(ToolStripItemCollection items)
    {
        foreach (ToolStripItem item in items)
        {
            if (item.Owner is not null) item.Owner.ImageList = null;
            if (item is ToolStripDropDownItem dd)
            {
                dd.DropDown.ImageList = null;
                ClearImageLists(dd.DropDownItems);
            }
        }
    }

    /// <summary>Editor drop-downs: values used in the catalog plus the user's custom lists (AMC lists).</summary>
    private void RefreshSuggestions()
    {
        foreach (var tag in new[] { "Genre", "Developer", "Publisher", "Store", "Platform", "Country", "Engine", "MediaType", "GameMode" })
        {
            var values = _session.Games.DistinctValues(g => GameFieldValues.Get(g, tag))
                .Concat(_prefs.CustomLists.GetValueOrDefault(tag) ?? new List<string>());
            gameFrame.SetSuggestions(tag, values);
        }
    }

    // --- Sorting and grouping ---

    private void SetSort(string tag)
    {
        _presenter.State.SortTag = tag;
        RefreshList();
    }

    private void SortByColumn(string tag)
    {
        var state = _presenter.State;
        if (tag == state.SortTag) state.Ascending = !state.Ascending;
        else
        {
            state.SortTag = tag;
            state.Ascending = true;
        }
        RefreshList();
    }

    private void actSortAscending_Execute(object? sender, EventArgs e) => SetOrder(true);
    private void actSortDescending_Execute(object? sender, EventArgs e) => SetOrder(false);
    private void actSortAscending_Update(object? sender, EventArgs e) => actSortAscending.Checked = _presenter.State.Ascending;
    private void actSortDescending_Update(object? sender, EventArgs e) => actSortDescending.Checked = !_presenter.State.Ascending;

    private void SetOrder(bool ascending)
    {
        _presenter.State.Ascending = ascending;
        RefreshList();
    }

    private void SetGroup(string tag)
    {
        _presenter.State.GroupTag = tag;
        RefreshList();
    }

    private void actGroupCount_Execute(object? sender, EventArgs e)
    {
        _prefs.ShowGroupCount = !_prefs.ShowGroupCount;
        _prefs.Save();
        RefreshList();
    }

    private void actGroupCount_Update(object? sender, EventArgs e) => actGroupCount.Checked = _prefs.ShowGroupCount;

    private void actStretchList_Execute(object? sender, EventArgs e) => gameList.StretchFirstColumn();

    private void actFlatTable_Execute(object? sender, EventArgs e)
    {
        if (_presenter.State.Thumbnails) SetThumbnails(false);
        SetGroup("");
    }

    private void actFlatTable_Update(object? sender, EventArgs e) =>
        actFlatTable.Checked = _presenter.State.GroupTag.Length == 0 && !_presenter.State.Thumbnails;

    private void actThumbnails_Execute(object? sender, EventArgs e) => SetThumbnails(!_presenter.State.Thumbnails);
    private void actThumbnails_Update(object? sender, EventArgs e) => actThumbnails.Checked = _presenter.State.Thumbnails;

    private void SetThumbnails(bool on)
    {
        int? number = _current?.Number;
        _presenter.State.Thumbnails = on;
        thumbView.Visible = on;
        gameList.Visible = !on;
        RefreshList();
        if (number is int n) SelectGame(n);
    }

    // --- HTML preview ---

    private void actShowHtml_Execute(object? sender, EventArgs e) => tabsGame.SelectedTab = tabHtml;
    private void actShowHtml_Update(object? sender, EventArgs e) => actShowHtml.Checked = tabsGame.SelectedTab == tabHtml;
    private void actHtmlRefresh_Execute(object? sender, EventArgs e) => _ = RefreshHtmlAsync();

    private void actHtmlCopy_Execute(object? sender, EventArgs e)
    {
        if (_current is null) return;
        Clipboard.SetText(BuildHtml(_current));
        SetHint(L.Get("Status.HtmlCopied"));
    }

    private string BuildHtml(Game game) =>
        HtmlTemplates.Expand(HtmlTemplates.Load(_prefs.HtmlTemplate), game, CoverImages.ToDataUri(game.CoverImage));

    /// <summary>Renders the preview only while its tab is visible (WebView2 work is not free).</summary>
    private Task RefreshHtmlAsync()
    {
        if (tabsGame.SelectedTab != tabHtml) return Task.CompletedTask;
        return htmlPreview.ShowHtmlAsync(_current is null ? "<html><body></body></html>" : BuildHtml(_current));
    }

    private void OnTabChanged() => _ = RefreshHtmlAsync();

    // --- Bars ---

    private void actViewMainToolbar_Execute(object? sender, EventArgs e) => tsMain.Visible = !tsMain.Visible;
    private void actViewMainToolbar_Update(object? sender, EventArgs e) => actViewMainToolbar.Checked = tsMain.Visible;
    private void actViewStatusBar_Execute(object? sender, EventArgs e) => statusMain.Visible = !statusMain.Visible;
    private void actViewStatusBar_Update(object? sender, EventArgs e) => actViewStatusBar.Checked = statusMain.Visible;

    // --- Find bar (AMC ToolbarFind) ---

    private void ShowFindBar(bool show)
    {
        findBar.Visible = show;
        actGameFind.Checked = show;
        if (show)
        {
            findBar.SetFields(GameFields.Searchable.Select(f => (f.Tag, CatalogLabels.Current.Field(f.Tag))), findBar.FieldTag);
            findBar.FocusText();
        }
        else
        {
            _presenter.State.FindOnly = null;
            RefreshList();
        }
    }

    private FindCriteria CurrentCriteria() =>
        new(findBar.FieldTag, findBar.SearchText, findBar.WholeField, findBar.Reverse);

    private void ApplyFind()
    {
        var criteria = CurrentCriteria();
        if (criteria.Text.Length == 0 || findBar.ShowResults)
        {
            _presenter.State.FindOnly = criteria.Text.Length == 0 ? null : _presenter.FindAll(criteria);
            RefreshList();
            return;
        }
        _presenter.State.FindOnly = null;
        RefreshList();
        var match = _presenter.FindFirst(criteria);
        if (match is not null) SelectGame(match.Number);
        SetHint(match is null ? L.Get("Status.NotFound") : L.Get("Status.Found", match.Number, match.Title));
    }

    private void FindNext()
    {
        var criteria = CurrentCriteria();
        if (criteria.Text.Length == 0) return;
        var next = _presenter.FindNext(criteria, _current?.Number);
        if (next is null) SetHint(L.Get("Status.NotFound"));
        else SelectGame(next.Number);
    }

    // --- Menus built at runtime (rebuilt on every opening, so they follow language and state) ---

    private void HookDynamicMenus()
    {
        mnuViewSort.DropDownOpening += (_, _) => ReplaceDynamicItems(mnuViewSort, SortItems());
        mnuViewGroup.DropDownOpening += (_, _) => ReplaceDynamicItems(mnuViewGroup, GroupItems());
        tbGroup.DropDownOpening += (_, _) => ReplaceDynamicItems(tbGroup, GroupItems());
        mnuViewGridFields.DropDownOpening += (_, _) => ReplaceDynamicItems(mnuViewGridFields, ColumnItems());
        tbGridFields.DropDownOpening += (_, _) => ReplaceDynamicItems(tbGridFields, ColumnItems());
        mnuViewSelectHtml.DropDownOpening += (_, _) => ReplaceDynamicItems(mnuViewSelectHtml, HtmlTemplateItems());
        mnuGameWeb.DropDownOpening += (_, _) => ReplaceDynamicItems(mnuGameWeb, WebSearchItems());
        mnuCtxColorTag.DropDownOpening += (_, _) => ReplaceDynamicItems(mnuCtxColorTag, ColorTagItems());
        tbImport.DropDownOpening += (_, _) => FillExchangeMenu(tbImport, ImportEntries());
        tbExport.DropDownOpening += (_, _) => FillExchangeMenu(tbExport, ExportEntries());
        // Placeholders so the drop-down arrows open before the first rebuild.
        foreach (var menu in new ToolStripDropDownItem[] { mnuViewGroup, tbGroup, mnuViewGridFields, tbGridFields, mnuViewSelectHtml, mnuGameWeb, mnuCtxColorTag, tbImport, tbExport })
            if (menu.DropDownItems.Count == 0) menu.DropDownItems.Add(new ToolStripMenuItem("…") { Tag = DynamicItem });
    }

    private static void ReplaceDynamicItems(ToolStripDropDownItem menu, IEnumerable<ToolStripItem> items)
    {
        foreach (var old in menu.DropDownItems.Cast<ToolStripItem>().Where(i => Equals(i.Tag, DynamicItem)).ToList())
        {
            menu.DropDownItems.Remove(old);
            old.Dispose();
        }
        int index = 0;
        foreach (var item in items)
        {
            item.Tag = DynamicItem;
            menu.DropDownItems.Insert(index++, item);
        }
    }

    private static ToolStripMenuItem MenuItem(string text, bool isChecked, Action onClick, AmcIcon icon = AmcIcon.None)
    {
        var item = new ToolStripMenuItem(text) { Checked = isChecked };
        if (icon != AmcIcon.None) item.ImageIndex = (int)icon;
        item.Click += (_, _) => onClick();
        return item;
    }

    private IEnumerable<ToolStripItem> SortItems() =>
        GameFields.SortableTags.Select(tag =>
            (ToolStripItem)MenuItem(CatalogLabels.Current.Field(tag), _presenter.State.SortTag == tag, () => SetSort(tag)));

    private IEnumerable<ToolStripItem> GroupItems()
    {
        yield return MenuItem(L.Get("Common.None"), _presenter.State.GroupTag.Length == 0, () => SetGroup(""), AmcIcon.Group);
        yield return new ToolStripSeparator();
        foreach (var tag in GameFields.GroupableTags)
            yield return MenuItem(CatalogLabels.Current.Field(tag), _presenter.State.GroupTag == tag, () => SetGroup(tag), AmcIcon.Group);
    }

    private IEnumerable<ToolStripItem> ColumnItems()
    {
        var visible = _presenter.State.Columns.Select(c => c.Tag).ToHashSet();
        foreach (var (tag, _) in GameListPresenter.AvailableColumns)
        {
            var item = new ToolStripMenuItem(CatalogLabels.Current.Field(tag)) { Checked = visible.Contains(tag), CheckOnClick = true };
            item.CheckedChanged += (_, _) =>
            {
                SyncColumnWidths();
                _presenter.ToggleColumn(tag, item.Checked);
                ApplyColumns();
                RefreshList();
            };
            yield return item;
        }
    }

    private IEnumerable<ToolStripItem> HtmlTemplateItems() =>
        HtmlTemplates.Names.Select(name => (ToolStripItem)MenuItem(name, _prefs.HtmlTemplate == name, () =>
        {
            _prefs.HtmlTemplate = name;
            _prefs.Save();
            _ = RefreshHtmlAsync();
        }));

    private IEnumerable<ToolStripItem> WebSearchItems()
    {
        var sites = _prefs.CustomSearchSites.Count > 0 ? _prefs.CustomSearchSites : PrefsStore.DefaultSearchSites();
        return sites.Select(site => (ToolStripItem)MenuItem(site.Name, false, () => WebSearch(site), AmcIcon.MovieSearch));
    }

    private IEnumerable<ToolStripItem> ColorTagItems()
    {
        for (int i = 0; i < ColorTags.Count; i++)
        {
            int tag = i;
            var item = MenuItem(tag == 0 ? L.Get("Common.None") : L.Get("Menu.ColorTag", tag), _current?.ColorTag == tag, () => SetColorTag(tag));
            if (tag > 0) item.Image = ColorTags.Swatch(tag);
            yield return item;
        }
    }
}
```

`MainForm.Tools.cs`:
```csharp
using GameCatalog.App.Forms;
using GameCatalog.App.Services;
using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;
using GameCatalog.Plugins.Abstractions;
using GameCatalog.Scripting;

namespace GameCatalog.App;

/// <summary>Tools and Help menus; getting information from scripts.</summary>
public partial class MainForm
{
    private void actManageFields_Execute(object? sender, EventArgs e)
    {
        CommitEditors();
        using var form = new CustomFieldsForm(_session.Games.CustomFieldDefs);
        form.ShowDialog(this);
        _session.MarkModified();
        customFrame.SetDefinitions(_session.Games.CustomFieldDefs);
        RebindCurrent();
    }

    private void actManagePictures_Execute(object? sender, EventArgs e)
    {
        using var form = new PicturesForm(_session.Games);
        if (form.ShowDialog(this) != DialogResult.OK) return;
        _session.MarkDirty();
        RebindCurrent();
    }

    private void actScripts_Execute(object? sender, EventArgs e)
    {
        if (_current is not { } game) return;
        using var form = new ScriptsForm(game, script => RunScriptAsync(script, selectAfter: false));
        form.ShowDialog(this);
    }

    private void actGetInfoScript_Execute(object? sender, EventArgs e)
    {
        if (!RequireCurrent()) return;
        var scripts = GameScript.LoadAll();
        if (scripts.Count == 0)
        {
            MessageBox.Show(this, L.Get("Msg.NoScripts"), L.Get("Common.AppName"));
            return;
        }
        using var dialog = new ChoiceDialog(L.Get("Script.SelectTitle"), L.Get("Script.SelectPrompt"), scripts.Select(s => s.ToString()).ToList());
        if (dialog.ShowDialog(this) == DialogResult.OK && dialog.SelectedIndex >= 0)
            _ = RunScriptAsync(scripts[dialog.SelectedIndex], selectAfter: true);
    }

    private async Task RunScriptAsync(GameScript script, bool selectAfter)
    {
        if (_current is not { } game) return;
        CommitEditors();
        UseWaitCursor = true;
        GameInfoResult result;
        try
        {
            result = await ScriptRunner.RunAsync(script, game.Clone(), PickFromList);
        }
        finally { UseWaitCursor = false; }
        ApplyInfoResult(game, result, script.Name);
        if (selectAfter) SelectGame(game.Number);
    }

    /// <summary>Script/provider callback: let the user pick from a list (AMC PickList).</summary>
    private int PickFromList(string title, IList<string> options)
    {
        if (InvokeRequired) return Invoke(() => PickFromList(title, options));
        using var form = new PickForm(title, options);
        return form.ShowDialog(this) == DialogResult.OK ? form.SelectedIndex : -1;
    }

    /// <summary>Shows fetched data, applies what the user ticks as one undoable step (AMC getscript_results).</summary>
    private void ApplyInfoResult(Game game, GameInfoResult result, string sourceName)
    {
        if (!result.Success)
        {
            MessageBox.Show(this, result.Error + "\n\n" + string.Join("\n", result.Log), L.Get("Msg.ScriptFailed"),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        using var form = new ScriptResultForm(game, result);
        if (form.ShowDialog(this) != DialogResult.OK || !result.HasData) return;
        var backup = game.Clone();
        foreach (var (tag, value) in form.CheckedFields) GameFieldWriter.Set(game, tag, value);
        foreach (var extra in result.Extras)
        {
            extra.GameNumber = game.Number;
            game.Extras.Add(extra);
        }
        if (form.IncludeCover) game.CoverImage = result.Cover;
        _session.Undo.Push(() =>
        {
            game.CopyFrom(backup);
            _session.MarkDirty();
            RebindCurrent();
        });
        _session.MarkDirty();
        RefreshSuggestions();
        RebindCurrent();
        SetHint(L.Get("Status.ScriptApplied", sourceName));
    }

    private void actRenumber_Execute(object? sender, EventArgs e)
    {
        var view = _presenter.CurrentView();
        if (view.Count == 0) return;
        if (MessageBox.Show(this, L.Get("Msg.ConfirmRenumber", view.Count), L.Get("Msg.Confirm"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        _numbering.RenumberSequential(view);
        RebindCurrent();
    }

    private void actLoans_Execute(object? sender, EventArgs e)
    {
        CommitEditors();
        using var form = new LoanForm(_session.Games, _session.Loans);
        if (form.ShowDialog(this) != DialogResult.OK) return;
        _session.MarkDirty();
        RebindCurrent();
    }

    private void actStats_Execute(object? sender, EventArgs e)
    {
        using var form = new StatsForm(_session.Games, _presenter.CurrentView().Select(g => g.Number).ToHashSet(),
            _current?.Number, _session.FilePath);
        form.ShowDialog(this);
    }

    private void actHtmlEditor_Execute(object? sender, EventArgs e)
    {
        using var form = new HtmlEditorForm(_prefs.HtmlTemplate);
        if (form.ShowDialog(this) == DialogResult.OK) _ = RefreshHtmlAsync();
    }

    private void actPreferences_Execute(object? sender, EventArgs e)
    {
        using var form = new PrefsForm(_prefs);
        if (form.ShowDialog(this) != DialogResult.OK) return;
        _prefs.AutoNumbering = form.AutoNumbering;
        _prefs.TitleFormat = form.TitleFormat;
        _prefs.NaturalSort = form.NaturalSort;
        _prefs.ForceFullRefresh = form.ForceFullRefresh;
        _prefs.ReopenLastFile = form.ReopenLastFile;
        _prefs.FullRowSelect = form.FullRowSelect;
        _prefs.ShowGridLines = form.ShowGridLines;
        _prefs.CoverVisible = form.CoverVisible;
        _prefs.ShowExtraCategory = form.ShowExtraCategory;
        _prefs.CsvDelimiter = form.CsvDelimiter;
        _prefs.BackupOnSave = form.BackupOnSave;
        _prefs.MediaPatterns = form.MediaPatterns;
        _prefs.MaxCoverWidth = form.MaxCoverWidth;
        _prefs.ScriptTimeoutSec = Math.Max(5, form.ScriptTimeoutSec);
        _prefs.CustomLists = form.CustomLists;
        _prefs.CustomSearchSites = form.CustomSearchSites;
        _prefs.HtmlTemplate = form.HtmlTemplate;
        _prefs.ShowGroupCount = form.ShowGroupCount;
        _prefs.Theme = form.Theme;
        _prefs.ShowMenuIcons = form.ShowMenuIcons;
        _prefs.Save();
        ApplyPreferences();
        RefreshList();
        BindFrames();
    }

    private void actHelpIndex_Execute(object? sender, EventArgs e)
    {
        using var form = new HelpForm();
        form.ShowDialog(this);
    }

    private void actHelpAbout_Execute(object? sender, EventArgs e)
    {
        using var form = new AboutForm();
        form.ShowDialog(this);
    }

    private void actHelpVersions_Execute(object? sender, EventArgs e)
    {
        using var form = new VersionsForm();
        form.ShowDialog(this);
    }
}
```
If an existing dialog's namespace is `GameCatalog.App` (not `GameCatalog.App.Forms`), keep the `using` lines that compile; Task 12 moves all dialogs into `GameCatalog.App.Forms`.

- [ ] **Step 11: Switch `Program` to the new window and verify manually**

`Program.cs`: `Application.Run(new MainForm());`
Run: `dotnet build GameCatalog.slnx` → succeeds. Run the app and check against the old behaviour:
1. Catalog opens; list, groups, sort by column click, cover panel, tabs work.
2. Edit a title → list row updates, title bar shows `*`; Ctrl+S saves; `*` disappears.
3. Add (Alt+Insert, number dialog when auto numbering is off), delete with confirmation, undo restores.
4. Change number (F2) with a conflict → dialog → games swap.
5. Find bar (Ctrl+F): typing selects the first match, "Pokaż wyniki" filters, Escape closes.
6. Thumbnails (F4), HTML tab (F12) renders the card with cover, print (Ctrl+P) opens the print UI.
7. Script (F6) → results window → apply → undo.
8. Toolbars can be dragged inside the top panel; View → Bars toggles them.
9. Set `"Language": "en"` in prefs.json → menus, toolbar hints, tabs, frames and status texts are English.
Take screenshots of the main window in both languages for the review.

- [ ] **Step 12: Delete the legacy code**

Delete `Forms/LegacyMainForm.cs`, `Controls/GameFieldsControl.cs`, `Controls/ControlLayout.cs`, `FieldValues.cs` (and the empty `Controls` folder). Run `dotnet build GameCatalog.slnx` and `dotnet test GameCatalog.slnx` → both succeed.

- [ ] **Step 13: Commit**

```bash
git add -A
git commit -m "feat(app): Designer-based main window built from frames, controls and actions

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Dialog conversion recipe (Tasks 12–13)

Apply to every dialog listed in the two tasks. The behaviour, public constructor signature and public properties of each dialog stay **exactly** as they are (the main window from Task 11 calls them).

1. **One class per file.** Files holding several forms are split (`SmallDialogs.cs` → `NumberDialog`, `NumberConflictDialog`, `CatalogPropertiesDialog`; `HelpForm.cs` → `HelpForm`, `VersionsForm`; `ScriptsForm.cs` → `PickForm`, `ScriptsForm`; `CustomFieldsForm.cs` → `CustomFieldsForm`, `CustomFieldEditForm`). Namespace `GameCatalog.App.Forms`.
2. **Designer file.** Create `<Form>.Designer.cs`. Move every control declared as a field or created in the constructor/`Build…` methods into `InitializeComponent`, keeping type, docking, size, order and default text; name controls by the Designer conventions above. Static items (fixed combo items, list columns, tab pages) go into the Designer too; items computed from data stay in code.
3. **Constructor** = `InitializeComponent();` + data binding + event wiring of logic. No layout code remains (`new Label`, `Controls.Add`, `Dock =` disappear from `<Form>.cs`, except for rows generated from data at runtime).
4. **Localizer.** Add `localizer` per the conventions.
5. **Texts.**
   - Text of a Designer control → stays as Polish Designer text, and the key `<control>.<Text|HeaderText|ToolTip>` goes into `forms.<Form>` in `pl.json` (Polish, identical) and `en.json` (English translation).
   - Any other user-visible literal (message boxes, status/labels computed at runtime, list items, file filters, captions with values) → a `strings` key `<Form>.<Purpose>` (e.g. `Loans.BorrowerRequired`), read through `AppServices.Languages.Get(...)`; format placeholders `{0}` replace interpolation. Polish value = the current literal, English value = its translation.
   - Field captions use `CatalogLabels.Current.Field(tag)`; yes/no uses `CatalogLabels.Current.Yes/No`; status captions use `CatalogLabels.Current.Status(...)`.
   - Generic words reuse `Common.*` (`Common.Ok`, `Common.Cancel`, `Common.Error`, `Common.None`) — add `Common.Delete` = Usuń/Delete, `Common.Confirm` = Potwierdź/Confirm, `Common.SaveError` = `Błąd zapisu: {0}`/`Save failed: {0}` the first time they are needed.
6. **Comments** in English.
7. **Check.** `dotnet build`, open the dialog from the running app in Polish and in English (`"Language": "en"` in prefs.json): layout identical to before, every text translated.

---

### Task 12: Convert the simple dialogs

**Files (each → `.cs` + `.Designer.cs`, namespace `GameCatalog.App.Forms`):**
- `Forms/AboutForm`, `Forms/HelpForm`, `Forms/VersionsForm`, `Forms/HtmlEditorForm`, `Forms/ExtraEditForm`, `Forms/CustomFieldsForm`, `Forms/CustomFieldEditForm`, `Forms/NumberDialog`, `Forms/NumberConflictDialog`, `Forms/CatalogPropertiesDialog`, `Forms/PickForm`, `Forms/ScriptResultForm`
- Delete: `Forms/SmallDialogs.cs` (after splitting)
- Modify: `pl.json`, `en.json`; `MainForm*.cs` (`using GameCatalog.App.Forms;`)
- Test: `tests/GameCatalog.App.Tests/DialogSmokeTests.cs`

**Interfaces:**
- Consumes: recipe above; `GameExtraFrame` (Task 10).
- Produces: unchanged public APIs:
  - `AboutForm()`, `HelpForm()`, `VersionsForm()`, `HtmlEditorForm(string templateName)`
  - `ExtraEditForm(GameExtra extra)` with `Extra`
  - `CustomFieldsForm(List<CustomFieldDefinition> definitions)`, `CustomFieldEditForm(...)` (current signature)
  - `NumberDialog(int current, int max)` with `Value`
  - `NumberConflictDialog(int proposedNumber, int maxNumber, Func<int,bool>? isDuplicate = null, Func<int,string?>? describe = null)` with `ChosenNumber`, `Resolution`, `DontAskAgain`
  - `CatalogPropertiesDialog(string path, int games, int extras, double hours, int loans)`
  - `PickForm(string title, IList<string> options)` with `SelectedIndex`
  - `ScriptResultForm(Game game, GameInfoResult result)` with `CheckedFields`, `IncludeCover`

Specific notes:
- **AboutForm**: the multi-line description becomes `strings` key `About.Description` (Polish = current text; English translation). Version line built from `Application.ProductVersion`.
- **HelpForm / VersionsForm**: each help topic becomes `Help.<Topic>.Title` + `Help.<Topic>.Body` (topics: `Start`, `List`, `Find`, `ImportExport`, `Scripts`, `Loans`, `Stats`, `Fields`, `Shortcuts`); the version history becomes `Versions.Text`. The topic list is built in code from these keys.
- **ExtraEditForm**: body is a `GameExtraFrame` (`Dock = Fill`) plus the OK/Cancel bar; OK calls `extraFrame.Commit()` after validation; validation message `ExtraEdit.TitleRequired` (`Tytuł jest wymagany.` / `A title is required.`) with caption `ExtraEdit.ValidationTitle` (`Walidacja` / `Validation`). Form caption `Dodatek / DLC` / `Extra / DLC`.
- **NumberConflictDialog**: keep the AMC layout ("Wybór numeru gry"); radio buttons `rbAcceptDuplicate`, `rbRenumberExisting`, `rbShiftNew`; the duplicate description label text is computed at runtime → `strings` `Number.TakenBy` (`Numer {0} ma już: {1}` / `Number {0} is already used by: {1}`).
- **CatalogPropertiesDialog**: the summary text → `strings` `Properties.Summary` with placeholders for path, games, extras, hours, loans.
- **ScriptResultForm**: row text `"{name}: „{old}” → „{new}”"` → `strings` `ScriptResult.Change` (`{0}: „{1}” → „{2}”` / `{0}: “{1}” → “{2}”`); cover row → `ScriptResult.Cover` (`Okładka ({0} KB)` / `Cover ({0} KB)`); field names via `CatalogLabels.Current.Field(tag)`.

- [ ] **Step 1: Write the smoke test**

`tests/GameCatalog.App.Tests/DialogSmokeTests.cs`:
```csharp
using GameCatalog.App.Forms;
using GameCatalog.App.Services;
using GameCatalog.Core.Models;
using GameCatalog.Localization;
using GameCatalog.Plugins.Abstractions;
using GameCatalog.UI.Components;

namespace GameCatalog.App.Tests;

/// <summary>Every converted dialog can be constructed and translated without showing it.</summary>
public class DialogSmokeTests
{
    public DialogSmokeTests()
    {
        AppServices.Languages = new LanguageManager(Path.Combine(AppContext.BaseDirectory, "Languages"), "pl");
        AppServices.Prefs = new PrefsStore();
        Localizer.Translator = AppServices.Languages;
    }

    public static IEnumerable<object[]> Dialogs() => new[]
    {
        new object[] { (Func<Form>)(() => new AboutForm()) },
        new object[] { (Func<Form>)(() => new HelpForm()) },
        new object[] { (Func<Form>)(() => new VersionsForm()) },
        new object[] { (Func<Form>)(() => new ExtraEditForm(new GameExtra { Title = "DLC" })) },
        new object[] { (Func<Form>)(() => new CustomFieldsForm(new List<CustomFieldDefinition>())) },
        new object[] { (Func<Form>)(() => new NumberDialog(1, 10)) },
        new object[] { (Func<Form>)(() => new NumberConflictDialog(2, 10, n => n == 2, _ => "Doom")) },
        new object[] { (Func<Form>)(() => new CatalogPropertiesDialog("c.gc", 1, 2, 3.5, 0)) },
        new object[] { (Func<Form>)(() => new PickForm("Pick", new List<string> { "a", "b" })) },
        new object[] { (Func<Form>)(() => new ScriptResultForm(new Game { Title = "A" }, new GameInfoResult { Success = true, Fields = { ["Title"] = "B" } })) },
    };

    [Theory]
    [MemberData(nameof(Dialogs))]
    public void Dialog_CreatesAndTranslatesInBothLanguages(Func<Form> create)
    {
        foreach (var code in new[] { "pl", "en" })
        {
            AppServices.Languages.SetLanguage(code);
            using var form = create();
            form.CreateControl();          // triggers Localizer.Apply via HandleCreated
            Assert.False(string.IsNullOrWhiteSpace(form.Text));
        }
    }
}
```
Add `[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]` in `tests/GameCatalog.App.Tests/AssemblyInfo.cs` (static `AppServices`/`Localizer.Translator`).

- [ ] **Step 2: Run the test to see the current state**

Run: `dotnet test tests/GameCatalog.App.Tests --filter "FullyQualifiedName~DialogSmokeTests"`
Expected: build FAIL (namespace `GameCatalog.App.Forms` does not contain the split classes yet).

- [ ] **Step 3: Convert the dialogs one by one following the recipe**

Order (one commit per two or three dialogs is fine): `NumberDialog`, `NumberConflictDialog`, `CatalogPropertiesDialog` (split `SmallDialogs.cs`, then delete it) → `PickForm` → `AboutForm` → `HelpForm`, `VersionsForm` → `HtmlEditorForm` → `ExtraEditForm` → `CustomFieldsForm`, `CustomFieldEditForm` → `ScriptResultForm`. After each: `dotnet build GameCatalog.slnx` and `dotnet test GameCatalog.slnx` (the `LanguageFilesTests` catch missing English keys).

- [ ] **Step 4: Run tests**

Run: `dotnet test GameCatalog.slnx` → all PASS, including `DialogSmokeTests`.

- [ ] **Step 5: Manual check and commit**

Open each dialog in the running app in Polish and English. Then:
```bash
git add -A
git commit -m "refactor(app): simple dialogs rebuilt in the Designer with translations

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 13: Convert the complex dialogs and guard the rules

**Files (each → `.cs` + `.Designer.cs`, namespace `GameCatalog.App.Forms`):**
- `Forms/ScriptsForm`, `Forms/PicturesForm`, `Forms/LoanForm`, `Forms/StatsForm`, `Forms/PrefsForm`
- Modify: `pl.json`, `en.json`
- Test: `tests/GameCatalog.App.Tests/DialogSmokeTests.cs` (extend), `tests/GameCatalog.App.Tests/SourceRulesTests.cs`

**Interfaces:**
- Consumes: recipe; `StatsCanvas` (Task 9); `IconThemeImageList.Themes` (Task 7).
- Produces: unchanged public APIs:
  - `ScriptsForm(Game game, Func<GameScript, Task> runTest)`
  - `PicturesForm(GameList games)`
  - `LoanForm(GameList games, List<LoanRecord> loans)`
  - `StatsForm(GameList games, HashSet<int> visibleNumbers, int? selectedNumber, string catalogPath)`
  - `PrefsForm(PrefsStore prefs)` with all current result properties (`Theme`, `AutoNumbering`, `ShowGroupCount`, `HtmlTemplate`, `TitleFormat`, `NaturalSort`, `ForceFullRefresh`, `ReopenLastFile`, `FullRowSelect`, `ShowGridLines`, `ShowMenuIcons`, `CoverVisible`, `ShowExtraCategory`, `CsvDelimiter`, `BackupOnSave`, `MediaPatterns`, `MaxCoverWidth`, `ScriptTimeoutSec`, `CustomLists`, `CustomSearchSites`)

Specific notes:
- **PrefsForm** (AMC options.dfm): `tvPages` (TreeView, Dock Left, Width 190) + one `pnlPage<Name>` per page (`Dock = Fill`, only the selected one visible): `General`, `Display`, `List`, `Extras`, `Pictures`, `Import`, `Scripts`, `Lists`, `Search`. Tree node texts are translated in code from `Prefs.Page.<Name>` strings (TreeView nodes are not controls). The theme combo lists `IconThemeImageList.Themes` captions `Languages.Get(theme.NameKey)`. Title-format combo items come from `Prefs.TitleFormat.Both/Original/Localized`; CSV delimiter items from `Prefs.Delimiter.Semicolon/Comma/Tab`.
- **StatsForm**: category list items are field captions (`CatalogLabels.Current.Field`) plus `Stats.General` (`Informacje ogólne` / `General information`); summary rows `Stats.TotalGames`, `Stats.AveragePlaytime`, `Stats.TotalPlaytime` (`{0} dni {1} godz. {2} min` / `{0} days {1} h {2} min`), `Stats.TotalSize`, `Stats.ExtrasCount`; range radio buttons in the Designer.
- **LoanForm**: status texts `Loans.StatusLent` (`wypożyczona`/`lent`), `Loans.StatusReturned` (`zwrócona`/`returned`); validation `Loans.BorrowerRequired`.
- **PicturesForm**: `"Tak"/"Nie"` → `CatalogLabels.Current.Yes/No`; `Pictures.Exported` (`Wyeksportowano {0} okładek.` / `Exported {0} covers.`), folder prompt `Pictures.SelectFolder`.
- **ScriptsForm**: messages `Scripts.ConfirmDelete` (`Usunąć skrypt „{0}”?`), `Scripts.SaveChanges`; save error via `Common.SaveError`.

- [ ] **Step 1: Extend the smoke test and add the source rules test**

Append to `DialogSmokeTests.Dialogs()`:
```csharp
        new object[] { (Func<Form>)(() => new ScriptsForm(new Game { Title = "A" }, _ => Task.CompletedTask)) },
        new object[] { (Func<Form>)(() => new PicturesForm(new GameCatalog.Core.Catalog.GameList())) },
        new object[] { (Func<Form>)(() => new LoanForm(new GameCatalog.Core.Catalog.GameList(), new List<LoanRecord>())) },
        new object[] { (Func<Form>)(() => new StatsForm(new GameCatalog.Core.Catalog.GameList(), new HashSet<int>(), null, "c.gc")) },
        new object[] { (Func<Form>)(() => new PrefsForm(new PrefsStore())) },
        new object[] { (Func<Form>)(() => new ChoiceDialog("T", "P", new[] { "a" })) },
```

`tests/GameCatalog.App.Tests/SourceRulesTests.cs`:
```csharp
using System.Text.RegularExpressions;

namespace GameCatalog.App.Tests;

/// <summary>
/// Project rules from the spec, checked on the source tree:
/// comments are written in English, and UI text is not hard-coded outside Designer files.
/// Polish diacritics are used as the detector.
/// </summary>
public class SourceRulesTests
{
    private static readonly Regex PolishLetters = new("[ąćęłńóśźżĄĆĘŁŃÓŚŹŻ]", RegexOptions.Compiled);
    private static readonly Regex StringLiteral = new("\"(?:[^\"\\\\]|\\\\.)*\"", RegexOptions.Compiled);

    /// <summary>Files whose Polish literals are catalog data or user-editable content, not UI text.</summary>
    private static readonly string[] DataFiles =
    {
        "GameCatalog.Core/Models/Enums.cs",            // stored list values (stage 7 localizes defaults)
        "GameCatalog.Core/Models/Game.cs",             // default MediaType value
        "GameCatalog.Core/Services/SampleDataFactory.cs",
        "GameCatalog.App/Services/HtmlTemplates.cs",   // user-editable templates (stage 5)
        "GameCatalog.Storage/AmcXmlImporter.cs",       // stored media type values
        "GameCatalog.Scripting/ScriptSamples.cs",      // sample script bodies
        "GameCatalog.Storage/CatalogTransfer.cs",      // removed from this list in Task 15
    };

    private static string SrcRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "GameCatalog.slnx"))) dir = dir.Parent;
        Assert.NotNull(dir);
        return Path.Combine(dir!.FullName, "src");
    }

    private static IEnumerable<(string Relative, string[] Lines)> SourceFiles() =>
        Directory.EnumerateFiles(SrcRoot(), "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .Select(p => (Path.GetRelativePath(SrcRoot(), p).Replace('\\', '/'), File.ReadAllLines(p)));

    [Fact]
    public void Comments_AreInEnglish()
    {
        var offenders = new List<string>();
        foreach (var (file, lines) in SourceFiles())
        {
            for (int i = 0; i < lines.Length; i++)
            {
                int c = lines[i].IndexOf("//", StringComparison.Ordinal);
                if (c < 0) continue;
                if (lines[i][..c].Count(ch => ch == '"') % 2 == 1) continue;   // "//" inside a string literal
                var comment = lines[i][c..];
                if (PolishLetters.IsMatch(comment)) offenders.Add($"{file}:{i + 1}: {comment.Trim()}");
            }
        }
        Assert.True(offenders.Count == 0, "Polish comments:\n" + string.Join("\n", offenders));
    }

    [Fact]
    public void UiText_IsNotHardCoded()
    {
        var offenders = new List<string>();
        foreach (var (file, lines) in SourceFiles())
        {
            if (file.EndsWith(".Designer.cs", StringComparison.Ordinal) || DataFiles.Contains(file)) continue;
            for (int i = 0; i < lines.Length; i++)
            {
                var code = lines[i].Split("//")[0];
                foreach (Match m in StringLiteral.Matches(code))
                    if (PolishLetters.IsMatch(m.Value)) offenders.Add($"{file}:{i + 1}: {m.Value}");
            }
        }
        Assert.True(offenders.Count == 0, "Hard-coded Polish text (move it to Languages/*.json):\n" + string.Join("\n", offenders));
    }
}
```

- [ ] **Step 2: Run the new tests**

Run: `dotnet test tests/GameCatalog.App.Tests --filter "FullyQualifiedName~SourceRulesTests|FullyQualifiedName~DialogSmokeTests"`
Expected: FAIL — the five complex dialogs still contain hard-coded Polish texts and comments (the failure message lists file:line).

- [ ] **Step 3: Convert `ScriptsForm`, `PicturesForm`, `LoanForm`, `StatsForm`, `PrefsForm`** following the recipe and the notes above, one commit each.

- [ ] **Step 4: Fix every remaining offender reported by `SourceRulesTests`**

Typical leftovers: comments in Storage/Scripting/UI files missed earlier, `MessageBox` texts in helpers, `"–"` placeholders (allowed — no diacritics). Move texts to JSON, translate comments.

- [ ] **Step 5: Run all tests**

Run: `dotnet test GameCatalog.slnx` → all PASS.

- [ ] **Step 6: Manual check and commit**

Open Preferences, Statistics, Loans, Pictures and Scripts in both languages; change a preference and confirm it applies. Then:
```bash
git add -A
git commit -m "refactor(app): complex dialogs rebuilt in the Designer; enforce English comments and no hard-coded UI text

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 14: Plugin contracts, plugin host and the sample plugin

**Files:**
- Create in `src/GameCatalog.Plugins.Abstractions/`: `PluginApi.cs`, `GameCatalogPluginAttribute.cs`, `IPlugin.cs`, `IPluginHost.cs`, `IGameCatalog.cs`, `IGameSelection.cs`, `Exchange.cs` (importer/exporter contracts + contexts + options), `IGameInfoProvider.cs`, `ICatalogCommand.cs`
- Create in `src/GameCatalog.Plugins.Abstractions.WinForms/`: `IGamePanelPlugin.cs`, `IWinFormsPluginHost.cs`
- Create in `src/GameCatalog.Plugins.Hosting/`: `ExtensionRegistry.cs`, `PluginDescriptor.cs`, `PluginHostBase.cs`, `PluginLoadContext.cs`, `PluginManager.cs`
- Create: `samples/GameCatalog.Plugin.Sample/` (`.csproj`, `SamplePlugin.cs`, `JsonGameImporter.cs`, `JsonGameExporter.cs`, `SampleInfoProvider.cs`, `MarkGamesWithoutCoverCommand.cs`, `GameSummaryPanel.cs`, `Languages/pl.json`, `Languages/en.json`, `README.md`)
- Create: `tests/Fixtures/GameCatalog.Plugin.TestFixtures/` (`.csproj`, `Fixtures.cs`)
- Modify: `GameCatalog.slnx` (add sample + fixtures), `src/GameCatalog.Plugins.Abstractions/GameInfoResult.cs` (`cref`)
- Test: `tests/GameCatalog.Plugins.Hosting.Tests/PluginManagerTests.cs`, `TestHost.cs`

**Interfaces:**
- Consumes: `Game`, `CustomFieldDefinition`, `GameExtra` (Core); `ITranslator` (Task 5); `GameInfoResult` (Task 4).
- Produces: the contracts below (exact code), and in Hosting:
  - `ExtensionRegistry` — `Add(string pluginId, object extension)`, `All<T>() : IReadOnlyList<T>`, `AllWithOwner<T>() : IReadOnlyList<(string PluginId, T Extension)>`, `RemoveAll(string pluginId)`, const `BuiltInId = "builtin"`
  - `PluginState { Loaded, Disabled, Incompatible, Failed }`, `PluginDescriptor` (`Id`, `Name`, `Version`, `Author`, `Description`, `AssemblyPath`, `State`, `Error`)
  - `abstract PluginHostBase : IPluginHost` — ctor `(string pluginId, ExtensionRegistry registry, string dataDirectory, Action<string> log)`; abstract `Catalog`, `Selection`, `Translator`
  - `PluginManager(ExtensionRegistry registry, Func<PluginDescriptor, IPluginHost> hostFactory, Action<string>? log = null)` — `Plugins`, `LoadFrom(string pluginsDirectory, IReadOnlyCollection<string> disabledIds)`, `LoadAssembly(string path, IReadOnlyCollection<string> disabledIds)`, static `SharedAssemblies`

- [ ] **Step 1: Write the contracts** (`namespace GameCatalog.Plugins.Abstractions`)

```csharp
namespace GameCatalog.Plugins.Abstractions;

/// <summary>Version of the plugin contracts (counterpart of AMC interfaces.pas). A plugin built for another version is not loaded.</summary>
public static class PluginApi
{
    public const int Version = 1;
}
```

```csharp
namespace GameCatalog.Plugins.Abstractions;

/// <summary>Marks the plugin entry class. The class must implement <see cref="IPlugin"/> and have a public parameterless constructor.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class GameCatalogPluginAttribute(string id, string name, string version) : Attribute
{
    /// <summary>Stable identifier; also the prefix of the plugin's translation keys ("Plugin.&lt;Id&gt;.").</summary>
    public string Id { get; } = id;
    public string Name { get; } = name;
    public string Version { get; } = version;
    public string Author { get; set; } = "";
    public string Description { get; set; } = "";
    /// <summary>Contract version the plugin was built against; defaults to the version it was compiled with.</summary>
    public int ApiVersion { get; set; } = PluginApi.Version;
}
```

```csharp
namespace GameCatalog.Plugins.Abstractions;

/// <summary>Plugin entry point. Initialize registers the plugin's extensions with the host.</summary>
public interface IPlugin
{
    void Initialize(IPluginHost host);
}
```

```csharp
using GameCatalog.Localization;

namespace GameCatalog.Plugins.Abstractions;

/// <summary>What the application offers to a plugin.</summary>
public interface IPluginHost
{
    string PluginId { get; }
    IGameCatalog Catalog { get; }
    IGameSelection Selection { get; }
    /// <summary>Translations; the plugin's own keys are available as "Plugin.&lt;Id&gt;.&lt;Key&gt;".</summary>
    ITranslator Translator { get; }
    /// <summary>Writable folder for the plugin's settings and caches.</summary>
    string DataDirectory { get; }
    void Log(string message);

    void Register(IGameImporter importer);
    void Register(IGameExporter exporter);
    void Register(IGameInfoProvider provider);
    void Register(ICatalogCommand command);
}
```

```csharp
using GameCatalog.Core.Models;

namespace GameCatalog.Plugins.Abstractions;

/// <summary>Read/write access to the open catalog. Changes made through it are undoable and refresh the views.</summary>
public interface IGameCatalog
{
    string FilePath { get; }
    IReadOnlyList<Game> Games { get; }
    IReadOnlyList<CustomFieldDefinition> CustomFieldDefinitions { get; }
    Game? Find(int number);
    int NextNumber();
    /// <summary>Adds a game (a free number is assigned when Number is not positive).</summary>
    void Add(Game game);
    void Remove(Game game);
    /// <summary>Call after modifying games in place; null means "many games changed".</summary>
    void NotifyChanged(Game? game = null);
}
```

```csharp
using GameCatalog.Core.Models;

namespace GameCatalog.Plugins.Abstractions;

/// <summary>The user's current position in the catalog.</summary>
public interface IGameSelection
{
    Game? Current { get; }
    /// <summary>Games visible in the list, in display order.</summary>
    IReadOnlyList<Game> Visible { get; }
    IReadOnlyList<Game> Checked { get; }
    void Select(int number);
    event EventHandler? CurrentChanged;
}
```

`Exchange.cs`:
```csharp
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
```

```csharp
using GameCatalog.Core.Models;

namespace GameCatalog.Plugins.Abstractions;

/// <summary>Interaction available to an info provider while it runs.</summary>
public interface IInfoProviderUi
{
    /// <summary>Lets the user pick one option; -1 when cancelled.</summary>
    int PickList(string title, IList<string> options);
    void ShowMessage(string message);
}

/// <summary>
/// Fetches information about a game (an alternative to .gcsx scripts, e.g. a native API client).
/// Receives a copy of the game; the result is shown to the user before anything is applied.
/// </summary>
public interface IGameInfoProvider
{
    string Name { get; }
    Task<GameInfoResult> FetchAsync(Game game, IInfoProviderUi ui, CancellationToken cancellationToken);
}
```

```csharp
namespace GameCatalog.Plugins.Abstractions;

/// <summary>A command added to the menu (and to the plugin toolbar when it has an icon).</summary>
public interface ICatalogCommand
{
    string Id { get; }
    /// <summary>Menu path: "File", "Game", "View", "Tools" or "Help", optionally followed by submenus, e.g. "Tools/My plugin".</summary>
    string MenuPath { get; }
    string Text { get; }
    string? Hint { get; }
    /// <summary>16x16 PNG, or null.</summary>
    byte[]? Icon { get; }
    /// <summary>Shortcut such as "Ctrl+Shift+I", or null.</summary>
    string? Shortcut { get; }
    bool CanExecute(IPluginHost host);
    void Execute(IPluginHost host);
}
```

In `GameInfoResult.cs` change the summary comment to `<see cref="IGameInfoProvider"/>`.

`src/GameCatalog.Plugins.Abstractions.WinForms/IGamePanelPlugin.cs`:
```csharp
using GameCatalog.Core.Models;
using GameCatalog.Plugins.Abstractions;

namespace GameCatalog.Plugins.Abstractions.WinForms;

/// <summary>A tab added to the game info panel.</summary>
public interface IGamePanelPlugin
{
    string Title { get; }
    /// <summary>Called once; the returned control is docked to fill the tab.</summary>
    Control CreatePanel(IPluginHost host);
    void OnCurrentGameChanged(Game? game);
}
```
`IWinFormsPluginHost.cs`:
```csharp
using GameCatalog.Plugins.Abstractions;

namespace GameCatalog.Plugins.Abstractions.WinForms;

/// <summary>Host of the desktop application; plugins with UI cast <see cref="IPluginHost"/> to it.</summary>
public interface IWinFormsPluginHost : IPluginHost
{
    void Register(IGamePanelPlugin panel);
}
```

- [ ] **Step 2: Create the test fixtures project**

`tests/Fixtures/GameCatalog.Plugin.TestFixtures/GameCatalog.Plugin.TestFixtures.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <EnableDynamicLoading>true</EnableDynamicLoading>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\..\src\GameCatalog.Plugins.Abstractions\GameCatalog.Plugins.Abstractions.csproj" Private="false" ExcludeAssets="runtime" />
    <ProjectReference Include="..\..\..\src\GameCatalog.Core\GameCatalog.Core.csproj" Private="false" ExcludeAssets="runtime" />
    <ProjectReference Include="..\..\..\src\GameCatalog.Localization\GameCatalog.Localization.csproj" Private="false" ExcludeAssets="runtime" />
  </ItemGroup>
</Project>
```
`Fixtures.cs`:
```csharp
using GameCatalog.Core.Models;
using GameCatalog.Plugins.Abstractions;

namespace GameCatalog.Plugin.TestFixtures;

/// <summary>Registers an importer, then fails: the host must roll the registration back.</summary>
[GameCatalogPlugin("fixture.throws", "Throwing plugin", "1.0")]
public sealed class ThrowingPlugin : IPlugin
{
    public void Initialize(IPluginHost host)
    {
        host.Register(new NullImporter());
        throw new InvalidOperationException("Initialization failed on purpose.");
    }
}

/// <summary>Built for a future contract version: must be reported as incompatible.</summary>
[GameCatalogPlugin("fixture.future", "Future plugin", "1.0", ApiVersion = PluginApi.Version + 1)]
public sealed class FuturePlugin : IPlugin
{
    public void Initialize(IPluginHost host) => host.Register(new NullImporter());
}

/// <summary>No attribute: must be ignored.</summary>
public sealed class UnmarkedPlugin : IPlugin
{
    public void Initialize(IPluginHost host) => host.Register(new NullImporter());
}

internal sealed class NullImporter : IGameImporter
{
    public string Name => "Null";
    public ImportSourceKind SourceKind => ImportSourceKind.File;
    public string FileFilter => "*.*|*.*";
    public IReadOnlyList<Game> Import(ImportContext context) => Array.Empty<Game>();
}
```

- [ ] **Step 3: Create the sample plugin**

`samples/GameCatalog.Plugin.Sample/GameCatalog.Plugin.Sample.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
    <!-- Produces a .deps.json so the host can resolve the plugin's private dependencies. -->
    <EnableDynamicLoading>true</EnableDynamicLoading>
  </PropertyGroup>
  <ItemGroup>
    <!-- Contracts are provided by the host at runtime: compile against them, do not copy them. -->
    <ProjectReference Include="..\..\src\GameCatalog.Plugins.Abstractions.WinForms\GameCatalog.Plugins.Abstractions.WinForms.csproj" Private="false" ExcludeAssets="runtime" />
    <ProjectReference Include="..\..\src\GameCatalog.Plugins.Abstractions\GameCatalog.Plugins.Abstractions.csproj" Private="false" ExcludeAssets="runtime" />
    <ProjectReference Include="..\..\src\GameCatalog.Core\GameCatalog.Core.csproj" Private="false" ExcludeAssets="runtime" />
    <ProjectReference Include="..\..\src\GameCatalog.Localization\GameCatalog.Localization.csproj" Private="false" ExcludeAssets="runtime" />
  </ItemGroup>
  <ItemGroup>
    <None Include="Languages\*.json" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>
  <!-- Debug builds deploy the plugin next to the app: bin\Debug\net10.0-windows\Plugins\GameCatalog.Plugin.Sample -->
  <Target Name="DeployToApp" AfterTargets="Build" Condition="'$(Configuration)' == 'Debug'">
    <ItemGroup>
      <PluginFiles Include="$(OutDir)**\*.*" />
    </ItemGroup>
    <Copy SourceFiles="@(PluginFiles)"
          DestinationFolder="$(MSBuildThisFileDirectory)..\..\src\GameCatalog.App\bin\$(Configuration)\net10.0-windows\Plugins\$(MSBuildProjectName)\%(RecursiveDir)"
          SkipUnchangedFiles="true" />
  </Target>
</Project>
```

`SamplePlugin.cs`:
```csharp
using GameCatalog.Plugins.Abstractions;
using GameCatalog.Plugins.Abstractions.WinForms;

namespace GameCatalog.Plugin.Sample;

/// <summary>Demonstrates every extension point of GameCatalog.</summary>
[GameCatalogPlugin("sample", "Sample plugin", "1.0", Author = "GameCatalog", Description = "Demonstrates every extension point.")]
public sealed class SamplePlugin : IPlugin
{
    public void Initialize(IPluginHost host)
    {
        host.Register(new JsonGameImporter(host));
        host.Register(new JsonGameExporter(host));
        host.Register(new SampleInfoProvider(host));
        host.Register(new MarkGamesWithoutCoverCommand(host));
        if (host is IWinFormsPluginHost desktop) desktop.Register(new GameSummaryPanel(host));
        host.Log("Sample plugin initialized.");
    }
}
```

`JsonGameImporter.cs`:
```csharp
using System.Text.Json;
using GameCatalog.Core.Models;
using GameCatalog.Plugins.Abstractions;

namespace GameCatalog.Plugin.Sample;

/// <summary>Imports a JSON array of games (the format written by <see cref="JsonGameExporter"/>).</summary>
internal sealed class JsonGameImporter(IPluginHost host) : IGameImporter
{
    public string Name => host.Translator.Get("Plugin.sample.ImportJson");
    public ImportSourceKind SourceKind => ImportSourceKind.File;
    public string FileFilter => "JSON (*.json)|*.json";

    public IReadOnlyList<Game> Import(ImportContext context)
    {
        var games = JsonSerializer.Deserialize<List<Game>>(File.ReadAllText(context.Path)) ?? new List<Game>();
        int number = context.StartNumber;
        foreach (var game in games) game.Number = number++;
        return games;
    }
}
```

`JsonGameExporter.cs`:
```csharp
using System.Text.Json;
using GameCatalog.Core.Models;
using GameCatalog.Plugins.Abstractions;

namespace GameCatalog.Plugin.Sample;

/// <summary>Exports the visible games as an indented JSON array.</summary>
internal sealed class JsonGameExporter(IPluginHost host) : IGameExporter
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public string Name => host.Translator.Get("Plugin.sample.ExportJson");
    public string FileFilter => "JSON (*.json)|*.json";

    public void Export(IReadOnlyList<Game> games, ExportContext context) =>
        File.WriteAllText(context.Path, JsonSerializer.Serialize(games, Options));
}
```

`SampleInfoProvider.cs`:
```csharp
using GameCatalog.Core.Models;
using GameCatalog.Plugins.Abstractions;

namespace GameCatalog.Plugin.Sample;

/// <summary>Returns test data without network access — shows the info-provider flow end to end.</summary>
internal sealed class SampleInfoProvider(IPluginHost host) : IGameInfoProvider
{
    public string Name => host.Translator.Get("Plugin.sample.InfoProvider");

    public Task<GameInfoResult> FetchAsync(Game game, IInfoProviderUi ui, CancellationToken cancellationToken)
    {
        var result = new GameInfoResult { Success = true };
        result.Fields["Comments"] = host.Translator.Get("Plugin.sample.InfoComment", game.Title, DateTime.Today.ToString("yyyy-MM-dd"));
        result.Log.Add("Sample provider ran for " + game.Title);
        return Task.FromResult(result);
    }
}
```

`MarkGamesWithoutCoverCommand.cs`:
```csharp
using GameCatalog.Plugins.Abstractions;

namespace GameCatalog.Plugin.Sample;

/// <summary>Tools → "Check games without a cover".</summary>
internal sealed class MarkGamesWithoutCoverCommand(IPluginHost host) : ICatalogCommand
{
    public string Id => "sample.check-without-cover";
    public string MenuPath => "Tools";
    public string Text => host.Translator.Get("Plugin.sample.CheckWithoutCover");
    public string? Hint => host.Translator.Get("Plugin.sample.CheckWithoutCoverHint");
    public byte[]? Icon => null;
    public string? Shortcut => null;

    public bool CanExecute(IPluginHost h) => h.Catalog.Games.Count > 0;

    public void Execute(IPluginHost h)
    {
        foreach (var game in h.Catalog.Games) game.Checked = !game.HasCover;
        h.Catalog.NotifyChanged();
    }
}
```

`GameSummaryPanel.cs`:
```csharp
using GameCatalog.Core.Models;
using GameCatalog.Plugins.Abstractions;
using GameCatalog.Plugins.Abstractions.WinForms;

namespace GameCatalog.Plugin.Sample;

/// <summary>"Summary" tab showing a one-paragraph description of the current game.</summary>
internal sealed class GameSummaryPanel(IPluginHost host) : IGamePanelPlugin
{
    private Label? _label;

    public string Title => host.Translator.Get("Plugin.sample.PanelTitle");

    public Control CreatePanel(IPluginHost _)
    {
        _label = new Label { Dock = DockStyle.Fill, Padding = new Padding(12), Font = new Font(SystemFonts.MessageBoxFont!.FontFamily, 11f) };
        return _label;
    }

    public void OnCurrentGameChanged(Game? game)
    {
        if (_label is null) return;
        _label.Text = game is null
            ? ""
            : host.Translator.Get("Plugin.sample.PanelSummary", game.DisplayTitle, game.Extras.Count, game.PlaytimeHours);
    }
}
```

`Languages/pl.json`:
```json
{
  "language": { "code": "pl", "name": "Polski" },
  "strings": {
    "ImportJson": "Z pliku JSON (przykładowy plugin)…",
    "ExportJson": "Do pliku JSON (przykładowy plugin)…",
    "InfoProvider": "Przykładowy dostawca danych",
    "InfoComment": "Sprawdzono „{0}” przykładowym pluginem ({1}).",
    "CheckWithoutCover": "Zaznacz gry bez okładki",
    "CheckWithoutCoverHint": "Zaznacza gry, które nie mają okładki",
    "PanelTitle": "Podsumowanie",
    "PanelSummary": "{0}\n\nDodatki: {1}\nCzas gry: {2:0.#} h"
  }
}
```
`Languages/en.json`:
```json
{
  "language": { "code": "en", "name": "English" },
  "strings": {
    "ImportJson": "From a JSON file (sample plugin)…",
    "ExportJson": "To a JSON file (sample plugin)…",
    "InfoProvider": "Sample data provider",
    "InfoComment": "Checked “{0}” with the sample plugin ({1}).",
    "CheckWithoutCover": "Check games without a cover",
    "CheckWithoutCoverHint": "Checks the games that have no cover",
    "PanelTitle": "Summary",
    "PanelSummary": "{0}\n\nExtras: {1}\nPlaytime: {2:0.#} h"
  }
}
```
`README.md` (sample folder): how to build a plugin — target framework, `EnableDynamicLoading`, references with `Private="false" ExcludeAssets="runtime"`, the `Plugins\<Name>\<Name>.dll` layout, the `[GameCatalogPlugin]` attribute, translation files and the `Plugin.<Id>.` key prefix, `ApiVersion` rules.

Add both new projects to `GameCatalog.slnx` (`/samples/` and `/tests/` folders).

- [ ] **Step 4: Write failing host tests**

`tests/GameCatalog.Plugins.Hosting.Tests/GameCatalog.Plugins.Hosting.Tests.csproj` — references `Plugins.Hosting`, `Plugins.Abstractions.WinForms`, and builds (without referencing) the plugin projects:
```xml
<ItemGroup>
  <ProjectReference Include="..\..\src\GameCatalog.Plugins.Hosting\GameCatalog.Plugins.Hosting.csproj" />
  <ProjectReference Include="..\..\src\GameCatalog.Plugins.Abstractions.WinForms\GameCatalog.Plugins.Abstractions.WinForms.csproj" />
  <ProjectReference Include="..\..\samples\GameCatalog.Plugin.Sample\GameCatalog.Plugin.Sample.csproj" ReferenceOutputAssembly="false" />
  <ProjectReference Include="..\Fixtures\GameCatalog.Plugin.TestFixtures\GameCatalog.Plugin.TestFixtures.csproj" ReferenceOutputAssembly="false" />
</ItemGroup>
```

`TestHost.cs`:
```csharp
using GameCatalog.Localization;
using GameCatalog.Plugins.Abstractions;
using GameCatalog.Plugins.Abstractions.WinForms;
using GameCatalog.Plugins.Hosting;

namespace GameCatalog.Plugins.Hosting.Tests;

/// <summary>Host whose catalog/selection are not needed by Initialize of the tested plugins.</summary>
internal sealed class TestHost(string pluginId, ExtensionRegistry registry, ITranslator translator)
    : PluginHostBase(pluginId, registry, Path.GetTempPath(), _ => { }), IWinFormsPluginHost
{
    public override IGameCatalog Catalog => throw new NotSupportedException();
    public override IGameSelection Selection => throw new NotSupportedException();
    public override ITranslator Translator => translator;
    public void Register(IGamePanelPlugin panel) => Registry.Add(PluginId, panel);
}
```

`PluginManagerTests.cs`:
```csharp
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
```

Run: `dotnet test tests/GameCatalog.Plugins.Hosting.Tests` → Expected: build FAIL (Hosting types missing).

- [ ] **Step 5: Implement Hosting**

`ExtensionRegistry.cs`:
```csharp
namespace GameCatalog.Plugins.Hosting;

/// <summary>Every extension registered by plugins and by the application itself, with its owner.</summary>
public sealed class ExtensionRegistry
{
    /// <summary>Owner id used for the application's built-in extensions.</summary>
    public const string BuiltInId = "builtin";

    private readonly List<(string PluginId, object Extension)> _items = new();

    public void Add(string pluginId, object extension) => _items.Add((pluginId, extension));

    public IReadOnlyList<T> All<T>() => _items.Select(i => i.Extension).OfType<T>().ToList();

    public IReadOnlyList<(string PluginId, T Extension)> AllWithOwner<T>() =>
        _items.Where(i => i.Extension is T).Select(i => (i.PluginId, (T)i.Extension)).ToList();

    public void RemoveAll(string pluginId) => _items.RemoveAll(i => i.PluginId == pluginId);
}
```

`PluginDescriptor.cs`:
```csharp
namespace GameCatalog.Plugins.Hosting;

public enum PluginState { Loaded, Disabled, Incompatible, Failed }

/// <summary>What the Plugins window shows about one plugin.</summary>
public sealed class PluginDescriptor(string id, string name, string version, string author, string description, string assemblyPath)
{
    public string Id { get; } = id;
    public string Name { get; } = name;
    public string Version { get; } = version;
    public string Author { get; } = author;
    public string Description { get; } = description;
    public string AssemblyPath { get; } = assemblyPath;
    public PluginState State { get; set; }
    public string? Error { get; set; }
}
```

`PluginHostBase.cs`:
```csharp
using GameCatalog.Localization;
using GameCatalog.Plugins.Abstractions;

namespace GameCatalog.Plugins.Hosting;

/// <summary>Registration and logging shared by every host implementation; one instance per plugin.</summary>
public abstract class PluginHostBase(string pluginId, ExtensionRegistry registry, string dataDirectory, Action<string> log) : IPluginHost
{
    protected ExtensionRegistry Registry { get; } = registry;

    public string PluginId { get; } = pluginId;
    public string DataDirectory { get; } = dataDirectory;
    public abstract IGameCatalog Catalog { get; }
    public abstract IGameSelection Selection { get; }
    public abstract ITranslator Translator { get; }

    public void Log(string message) => log($"[{PluginId}] {message}");

    public void Register(IGameImporter importer) => Registry.Add(PluginId, importer);
    public void Register(IGameExporter exporter) => Registry.Add(PluginId, exporter);
    public void Register(IGameInfoProvider provider) => Registry.Add(PluginId, provider);
    public void Register(ICatalogCommand command) => Registry.Add(PluginId, command);
}
```

`PluginLoadContext.cs`:
```csharp
using System.Reflection;
using System.Runtime.Loader;

namespace GameCatalog.Plugins.Hosting;

/// <summary>
/// Isolated load context per plugin: the plugin's private dependencies come from its folder,
/// while the shared contract assemblies come from the application so types match.
/// </summary>
internal sealed class PluginLoadContext(string pluginPath, IReadOnlySet<string> sharedAssemblies)
    : AssemblyLoadContext(name: Path.GetFileNameWithoutExtension(pluginPath), isCollectible: false)
{
    private readonly AssemblyDependencyResolver _resolver = new(pluginPath);

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (assemblyName.Name is { } name && sharedAssemblies.Contains(name)) return null;   // use the host's copy
        var path = _resolver.ResolveAssemblyToPath(assemblyName);
        return path is null ? null : LoadFromAssemblyPath(path);
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var path = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        return path is null ? IntPtr.Zero : LoadUnmanagedDllFromPath(path);
    }
}
```

`PluginManager.cs`:
```csharp
using System.Reflection;
using GameCatalog.Plugins.Abstractions;

namespace GameCatalog.Plugins.Hosting;

/// <summary>
/// Discovers plugins in Plugins\&lt;Name&gt;\&lt;Name&gt;.dll, checks their contract version and initializes them.
/// A plugin that fails never stops the others or the application: it is reported with its error.
/// </summary>
public sealed class PluginManager(ExtensionRegistry registry, Func<PluginDescriptor, IPluginHost> hostFactory, Action<string>? log = null)
{
    /// <summary>Assemblies always taken from the application, never from a plugin folder.</summary>
    public static readonly IReadOnlySet<string> SharedAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "GameCatalog.Core",
        "GameCatalog.Localization",
        "GameCatalog.Plugins.Abstractions",
        "GameCatalog.Plugins.Abstractions.WinForms",
    };

    private readonly List<PluginDescriptor> _plugins = new();
    private readonly Action<string> _log = log ?? (_ => { });

    public IReadOnlyList<PluginDescriptor> Plugins => _plugins;

    public void LoadFrom(string pluginsDirectory, IReadOnlyCollection<string> disabledIds)
    {
        if (!Directory.Exists(pluginsDirectory)) return;
        foreach (var dir in Directory.GetDirectories(pluginsDirectory))
        {
            var dll = Path.Combine(dir, Path.GetFileName(dir) + ".dll");
            if (File.Exists(dll)) LoadAssembly(dll, disabledIds);
            else _log($"Plugins: '{dir}' has no {Path.GetFileName(dll)}, skipped.");
        }
    }

    public void LoadAssembly(string path, IReadOnlyCollection<string> disabledIds)
    {
        Assembly assembly;
        try
        {
            var context = new PluginLoadContext(Path.GetFullPath(path), SharedAssemblies);
            assembly = context.LoadFromAssemblyPath(Path.GetFullPath(path));
        }
        catch (Exception ex) when (ex is BadImageFormatException or FileLoadException or FileNotFoundException or InvalidOperationException)
        {
            var id = Path.GetFileNameWithoutExtension(path);
            _plugins.Add(new PluginDescriptor(id, id, "", "", "", path) { State = PluginState.Failed, Error = ex.Message });
            _log($"Plugins: cannot load '{path}': {ex.Message}");
            return;
        }

        foreach (var type in PluginTypes(assembly))
        {
            var info = type.GetCustomAttribute<GameCatalogPluginAttribute>();
            if (info is null)
            {
                _log($"Plugins: {type.FullName} has no [GameCatalogPlugin] attribute, skipped.");
                continue;
            }
            var descriptor = new PluginDescriptor(info.Id, info.Name, info.Version, info.Author, info.Description, path);
            _plugins.Add(descriptor);
            if (disabledIds.Contains(info.Id))
            {
                descriptor.State = PluginState.Disabled;
            }
            else if (info.ApiVersion != PluginApi.Version)
            {
                descriptor.State = PluginState.Incompatible;
                descriptor.Error = $"Plugin API {info.ApiVersion}, application API {PluginApi.Version}.";
            }
            else if (_plugins.Any(p => p != descriptor && p.Id == info.Id && p.State == PluginState.Loaded))
            {
                descriptor.State = PluginState.Failed;
                descriptor.Error = $"Another plugin already uses the id '{info.Id}'.";
            }
            else
            {
                Initialize(type, descriptor);
            }
            _log($"Plugins: {descriptor.Id} {descriptor.Version} — {descriptor.State}{(descriptor.Error is null ? "" : ": " + descriptor.Error)}");
        }
    }

    private void Initialize(Type type, PluginDescriptor descriptor)
    {
        try
        {
            var plugin = (IPlugin)Activator.CreateInstance(type)!;
            plugin.Initialize(hostFactory(descriptor));
            descriptor.State = PluginState.Loaded;
        }
        catch (Exception ex)   // isolation boundary: any plugin failure is reported, never propagated
        {
            registry.RemoveAll(descriptor.Id);
            descriptor.State = PluginState.Failed;
            descriptor.Error = (ex is TargetInvocationException { InnerException: { } inner } ? inner : ex).Message;
        }
    }

    private IEnumerable<Type> PluginTypes(Assembly assembly)
    {
        Type?[] types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            _log($"Plugins: some types of {assembly.GetName().Name} could not be loaded: {ex.LoaderExceptions.FirstOrDefault()?.Message}");
            types = ex.Types;
        }
        return types.OfType<Type>().Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IPlugin).IsAssignableFrom(t));
    }
}
```

- [ ] **Step 6: Run tests**

Run: `dotnet build GameCatalog.slnx` then `dotnet test tests/GameCatalog.Plugins.Hosting.Tests` → Expected: all PASS.
Run: `dotnet test GameCatalog.slnx` → all PASS.

- [ ] **Step 7: Commit**

```bash
git add -A
git commit -m "feat(plugins): contracts, isolated plugin host with version checks, sample plugin and fixtures

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 15: Built-in formats as extensions and plugins inside the application

**Files:**
- Create: `src/GameCatalog.Storage/Exchange/CsvGameImporter.cs`, `AmcXmlGameImporter.cs`, `FolderGameImporter.cs`, `CsvGameExporter.cs`, `HtmlGameExporter.cs`, `BuiltInExchange.cs`
- Modify: `src/GameCatalog.Storage/GameCatalog.Storage.csproj` (reference Plugins.Abstractions), `CatalogTransfer.cs` (HTML headers from `IReadOnlyList<string>` captions)
- Create: `src/GameCatalog.App/Plugins/AppPluginHost.cs`, `PluginMenuPath.cs`, `PluginShortcut.cs`, `DialogInfoProviderUi.cs`; `src/GameCatalog.App/MainForm.Plugins.cs`; `src/GameCatalog.App/Forms/PluginsForm` (+ Designer)
- Modify: `MainForm.Designer.cs` (`tsPlugins`, `actPlugins`), `MainForm.File.cs` (`ImportEntries`/`ExportEntries` from the registry), `Services/PrefsStore.cs` (`DisabledPlugins`), `pl.json`, `en.json`, `tests/GameCatalog.App.Tests/SourceRulesTests.cs` (remove `CatalogTransfer.cs` from `DataFiles`)
- Test: `tests/GameCatalog.Storage.Tests/BuiltInExchangeTests.cs`, `tests/GameCatalog.App.Tests/PluginMenuPathTests.cs`

**Interfaces:**
- Consumes: Tasks 3, 11, 14.
- Produces:
  - `BuiltInExchange.Register(ExtensionRegistry registry, ITranslator translator)` — registers the 3 importers and 2 exporters under `ExtensionRegistry.BuiltInId` (Storage references Plugins.Hosting? **No** — to keep Storage independent of Hosting, `BuiltInExchange.Create(ITranslator) : (IReadOnlyList<IGameImporter> Importers, IReadOnlyList<IGameExporter> Exporters)`; the App adds them to the registry)
  - `PluginMenuPath.Parse(string path) : (string Root, IReadOnlyList<string> Submenus)`; `Root` ∈ {`File`,`Game`,`View`,`Tools`,`Help`} (unknown → `Tools`)
  - `PluginShortcut.TryParse(string? text, out Keys keys) : bool`
  - `PrefsStore.DisabledPlugins : List<string>`

- [ ] **Step 1: Write failing tests**

`tests/GameCatalog.Storage.Tests/BuiltInExchangeTests.cs` (test project gets a reference to `GameCatalog.Localization` and `GameCatalog.Plugins.Abstractions` through Storage):
```csharp
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
}
```

`tests/GameCatalog.App.Tests/PluginMenuPathTests.cs`:
```csharp
using GameCatalog.App.Plugins;

namespace GameCatalog.App.Tests;

public class PluginMenuPathTests
{
    [Theory]
    [InlineData("Tools", "Tools", "")]
    [InlineData("game/Extras/More", "Game", "Extras|More")]
    [InlineData(" Help ", "Help", "")]
    [InlineData("Unknown/Sub", "Tools", "Sub")]
    [InlineData("", "Tools", "")]
    public void Parse_NormalizesRootAndSplitsSubmenus(string path, string root, string submenus)
    {
        var (r, subs) = PluginMenuPath.Parse(path);
        Assert.Equal(root, r);
        Assert.Equal(submenus, string.Join("|", subs));
    }

    [Theory]
    [InlineData("Ctrl+Shift+I", Keys.Control | Keys.Shift | Keys.I, true)]
    [InlineData("F7", Keys.F7, true)]
    [InlineData("nonsense", Keys.None, false)]
    [InlineData(null, Keys.None, false)]
    public void Shortcut_TryParse(string? text, Keys expected, bool ok)
    {
        Assert.Equal(ok, PluginShortcut.TryParse(text, out var keys));
        Assert.Equal(expected, keys);
    }
}
```
Run: `dotnet test GameCatalog.slnx` → Expected: build FAIL.

- [ ] **Step 2: Built-in exchange classes (Storage)**

Add `<ProjectReference Include="..\GameCatalog.Plugins.Abstractions\GameCatalog.Plugins.Abstractions.csproj" />` to Storage. In `CatalogTransfer.ExportHtml` add a parameter `IReadOnlyList<string> headers` (9 captions) instead of the Polish header row; the caller passes translated captions.

```csharp
using GameCatalog.Localization;
using GameCatalog.Plugins.Abstractions;

namespace GameCatalog.Storage.Exchange;

/// <summary>The application's own formats, exposed through the same contracts as plugin formats.</summary>
public static class BuiltInExchange
{
    public static (IReadOnlyList<IGameImporter> Importers, IReadOnlyList<IGameExporter> Exporters) Create(ITranslator t) =>
        (new IGameImporter[] { new FolderGameImporter(t), new CsvGameImporter(t), new AmcXmlGameImporter(t) },
         new IGameExporter[] { new CsvGameExporter(t), new HtmlGameExporter(t) });

    internal static char Delimiter(IReadOnlyDictionary<string, string> options) =>
        options.TryGetValue(ExchangeOptions.Delimiter, out var d) && d.Length > 0 ? (d == "TAB" ? '\t' : d[0]) : ';';
}
```

```csharp
using GameCatalog.Core.Models;
using GameCatalog.Localization;
using GameCatalog.Plugins.Abstractions;

namespace GameCatalog.Storage.Exchange;

/// <summary>Folder import: every subfolder (or matching file) becomes a game (AMC import2 directory engine).</summary>
public sealed class FolderGameImporter(ITranslator t) : IGameImporter
{
    public string Name => t.Get("Exchange.ImportFolder");
    public ImportSourceKind SourceKind => ImportSourceKind.Folder;
    public string FileFilter => "";

    public IReadOnlyList<Game> Import(ImportContext context) =>
        CatalogTransfer.ImportFromFolder(context.Path, context.StartNumber,
            context.Options.GetValueOrDefault(ExchangeOptions.FilePatterns));
}

/// <summary>CSV import (AMC import2 CSV engine).</summary>
public sealed class CsvGameImporter(ITranslator t) : IGameImporter
{
    public string Name => t.Get("Exchange.ImportCsv");
    public ImportSourceKind SourceKind => ImportSourceKind.File;
    public string FileFilter => t.Get("Filter.Csv");

    public IReadOnlyList<Game> Import(ImportContext context) =>
        CatalogTransfer.ImportCsv(context.Path, context.StartNumber, BuiltInExchange.Delimiter(context.Options));
}

/// <summary>Ant Movie Catalog XML import with movie → game field mapping.</summary>
public sealed class AmcXmlGameImporter(ITranslator t) : IGameImporter
{
    public string Name => t.Get("Exchange.ImportAmc");
    public ImportSourceKind SourceKind => ImportSourceKind.File;
    public string FileFilter => t.Get("Filter.AmcXml");

    public IReadOnlyList<Game> Import(ImportContext context) => AmcXmlImporter.Import(context.Path, context.StartNumber);
}

/// <summary>CSV export of the visible games.</summary>
public sealed class CsvGameExporter(ITranslator t) : IGameExporter
{
    public string Name => t.Get("Exchange.ExportCsv");
    public string FileFilter => t.Get("Filter.Csv");

    public void Export(IReadOnlyList<Game> games, ExportContext context) =>
        CatalogTransfer.ExportCsv(games, context.Path, BuiltInExchange.Delimiter(context.Options));
}

/// <summary>Simple HTML table export of the visible games (stage 5 brings AMC templates).</summary>
public sealed class HtmlGameExporter(ITranslator t) : IGameExporter
{
    public string Name => t.Get("Exchange.ExportHtml");
    public string FileFilter => t.Get("Filter.Html");

    public void Export(IReadOnlyList<Game> games, ExportContext context) =>
        CatalogTransfer.ExportHtml(games, context.Path,
            context.Options.GetValueOrDefault(ExchangeOptions.Title) ?? t.Get("Export.HtmlTitle"),
            new[] { "#", t.Get("Field.Title"), t.Get("Field.Developer"), t.Get("Field.ReleaseYear"), t.Get("Field.Genre"),
                    t.Get("Field.Platform"), t.Get("Field.CompletionStatus"), t.Get("Field.UserRating"), t.Get("Field.PlaytimeHours") });
}
```
(Each class in its own file as listed under **Files**.) Remove `GameCatalog.Storage/CatalogTransfer.cs` from `SourceRulesTests.DataFiles`.

- [ ] **Step 3: App-side helpers**

`src/GameCatalog.App/Plugins/PluginMenuPath.cs`:
```csharp
namespace GameCatalog.App.Plugins;

/// <summary>Parses ICatalogCommand.MenuPath ("Tools/My plugin") into a top-level menu and submenus.</summary>
public static class PluginMenuPath
{
    private static readonly string[] Roots = { "File", "Game", "View", "Tools", "Help" };

    public static (string Root, IReadOnlyList<string> Submenus) Parse(string? path)
    {
        var parts = (path ?? "").Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0) return ("Tools", Array.Empty<string>());
        var root = Roots.FirstOrDefault(r => r.Equals(parts[0], StringComparison.OrdinalIgnoreCase));
        return root is null ? ("Tools", parts[1..]) : (root, parts[1..]);
    }
}
```

`PluginShortcut.cs`:
```csharp
namespace GameCatalog.App.Plugins;

/// <summary>Parses shortcut text such as "Ctrl+Shift+I".</summary>
public static class PluginShortcut
{
    public static bool TryParse(string? text, out Keys keys)
    {
        keys = Keys.None;
        if (string.IsNullOrWhiteSpace(text)) return false;
        try
        {
            keys = (Keys)(new KeysConverter().ConvertFromInvariantString(text) ?? Keys.None);
            return keys != Keys.None;
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or NotSupportedException)
        {
            keys = Keys.None;
            return false;
        }
    }
}
```

`DialogInfoProviderUi.cs`:
```csharp
using GameCatalog.Plugins.Abstractions;

namespace GameCatalog.App.Plugins;

/// <summary>Info-provider interaction implemented with the application's dialogs.</summary>
internal sealed class DialogInfoProviderUi(Func<string, IList<string>, int> pick, Action<string> message) : IInfoProviderUi
{
    public int PickList(string title, IList<string> options) => pick(title, options);
    public void ShowMessage(string text) => message(text);
}
```

`AppPluginHost.cs`:
```csharp
using GameCatalog.Localization;
using GameCatalog.Plugins.Abstractions;
using GameCatalog.Plugins.Abstractions.WinForms;
using GameCatalog.Plugins.Hosting;

namespace GameCatalog.App.Plugins;

/// <summary>Host given to each plugin; catalog and selection are facades over the main window.</summary>
internal sealed class AppPluginHost(
    string pluginId, ExtensionRegistry registry, string dataDirectory, Action<string> log,
    IGameCatalog catalog, IGameSelection selection, ITranslator translator)
    : PluginHostBase(pluginId, registry, dataDirectory, log), IWinFormsPluginHost
{
    public override IGameCatalog Catalog => catalog;
    public override IGameSelection Selection => selection;
    public override ITranslator Translator => translator;
    public void Register(IGamePanelPlugin panel) => Registry.Add(PluginId, panel);
}
```

Add to `PrefsStore`:
```csharp
/// <summary>Ids of plugins switched off in Tools → Plugins.</summary>
public List<string> DisabledPlugins { get; set; } = new();
```

- [ ] **Step 4: `MainForm.Plugins.cs` — load plugins and expose extensions in the UI**

Add to the Designer: `tsPlugins` (ToolStrip in `toolStripContainer.TopToolStripPanel`, `UseThemeIcons`, `Visible = false`), action `actPlugins` (Key `Plugins`, Text `Plugin&y…`, Icon `None`, in `mnuTools` after `actHtmlEditor`), strings `Action.Plugins` = `Plugin&y…`/`Plu&gins…`, `Action.Plugins.Hint` = `Pluginy`/`Plugins`.

```csharp
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
    private readonly List<(string PluginId, IGamePanelPlugin Panel)> _panels = new();

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
        foreach (var failed in _plugins.Plugins.Where(p => p.State == PluginState.Failed))
            SetHint(L.Get("Plugin.LoadFailed", failed.Name, failed.Error ?? ""));
    }

    private IPluginHost CreateHost(PluginDescriptor plugin)
    {
        var languages = Path.Combine(Path.GetDirectoryName(plugin.AssemblyPath)!, "Languages");
        AppServices.Languages.AddPluginLanguages(plugin.Id, languages);
        return new AppPluginHost(plugin.Id, _extensions, AppPaths.PluginDataDirectory(plugin.Id), AppLog.Write, this, this, L);
    }

    /// <summary>Runs plugin code; failures are logged and shown, never propagated.</summary>
    private T? Guard<T>(string pluginId, Func<T> action, T? fallback = default)
    {
        try
        {
            return action();
        }
        catch (Exception ex)   // isolation boundary for third-party code
        {
            AppLog.Error(ex, $"Plugin {pluginId}");
            MessageBox.Show(this, L.Get("Plugin.Error", pluginId, ex.Message), L.Get("Common.Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return fallback;
        }
    }

    private void Guard(string pluginId, Action action) => Guard(pluginId, () => { action(); return true; });

    // --- Import / export entries from the registry (replaces the fixed lists of Task 11) ---

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

    private void RunImporter(string pluginId, IGameImporter importer)
    {
        string? path = null;
        if (importer.SourceKind == ImportSourceKind.Folder)
        {
            using var dialog = new FolderBrowserDialog { Description = importer.Name, UseDescriptionForTitle = true };
            if (dialog.ShowDialog(this) == DialogResult.OK) path = dialog.SelectedPath;
        }
        else
        {
            using var dialog = new OpenFileDialog { Filter = importer.FileFilter, Title = importer.Name };
            if (dialog.ShowDialog(this) == DialogResult.OK) path = dialog.FileName;
        }
        if (path is null) return;
        var context = new ImportContext { Path = path, StartNumber = _session.Games.NextNumber(), Options = ExchangeOptionsFromPrefs() };
        var games = Guard(pluginId, () => importer.Import(context).ToList());
        if (games is not null) AddImported(() => games);
    }

    private void RunExporter(string pluginId, IGameExporter exporter)
    {
        using var dialog = new SaveFileDialog { Filter = exporter.FileFilter, Title = exporter.Name };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        var context = new ExportContext { Path = dialog.FileName, Options = ExchangeOptionsFromPrefs() };
        if (Guard(pluginId, () => { exporter.Export(_presenter.CurrentView(), context); return true; }))
            SetHint(L.Get("Status.Exported", dialog.FileName));
    }

    // --- Info providers next to scripts in Game → Get information ---

    private IEnumerable<ToolStripItem> InfoProviderItems() =>
        _extensions.AllWithOwner<IGameInfoProvider>().Select(x =>
            (ToolStripItem)MenuItem(x.Extension.Name, false, () => _ = RunProviderAsync(x.PluginId, x.Extension), AmcIcon.MovieImportScript));

    private async Task RunProviderAsync(string pluginId, IGameInfoProvider provider)
    {
        if (_current is not { } game) return;
        CommitEditors();
        var ui = new DialogInfoProviderUi(PickFromList, text => SetHint(text));
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
        ApplyInfoResult(game, result, provider.Name);
    }

    // --- Commands ---

    private void AddPluginCommands()
    {
        foreach (var (pluginId, command) in _extensions.AllWithOwner<ICatalogCommand>())
        {
            var host = CreateHostForCommands(pluginId);
            var action = new UIAction(components)
            {
                Text = Guard(pluginId, () => command.Text, command.Id)!,
                ToolTipText = Guard(pluginId, () => command.Hint, null) ?? "",
                ShortcutKeys = PluginShortcut.TryParse(command.Shortcut, out var keys) ? keys : Keys.None,
            };
            action.Execute += (_, _) => Guard(pluginId, () => command.Execute(host));
            action.Update += (_, _) => action.Enabled = Guard(pluginId, () => command.CanExecute(host), false);
            Image? icon = command.Icon is { Length: > 0 } bytes ? CoverImages.TryLoad(bytes) : null;

            var (root, submenus) = PluginMenuPath.Parse(command.MenuPath);
            var menu = submenus.Aggregate(RootMenu(root), FindOrAddSubmenu);
            var item = new ToolStripMenuItem { Image = icon };
            menu.DropDownItems.Add(item);
            actionList.SetAction(item, action);
            if (icon is not null)
            {
                var button = new ToolStripButton { DisplayStyle = ToolStripItemDisplayStyle.Image, Image = icon };
                tsPlugins.Items.Add(button);
                actionList.SetAction(button, action);
                tsPlugins.Visible = true;
            }
        }
    }

    private IPluginHost CreateHostForCommands(string pluginId) =>
        new AppPluginHost(pluginId, _extensions, AppPaths.PluginDataDirectory(pluginId), AppLog.Write, this, this, L);

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
            var host = CreateHostForCommands(pluginId);
            var control = Guard(pluginId, () => panel.CreatePanel(host));
            if (control is null) continue;
            control.Dock = DockStyle.Fill;
            var tab = new TabPage(Guard(pluginId, () => panel.Title, pluginId)) { Tag = pluginId };
            tab.Controls.Add(control);
            tabsGame.TabPages.Add(tab);
            _panels.Add((pluginId, panel));
        }
    }

    private void NotifyPanels()
    {
        foreach (var (pluginId, panel) in _panels) Guard(pluginId, () => panel.OnCurrentGameChanged(_current));
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
        _session.Games.Add(game);
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
        _session.MarkDirty();
        RebindCurrent();
    }

    // --- IGameSelection ---

    Game? IGameSelection.Current => _current;
    IReadOnlyList<Game> IGameSelection.Visible => _presenter.CurrentView();
    IReadOnlyList<Game> IGameSelection.Checked => _session.Games.Games.Where(g => g.Checked).ToList();
    void IGameSelection.Select(int number) => SelectGame(number);
    public event EventHandler? CurrentChanged;
}
```

Wire-up changes in the existing partial files:
- `MainForm.File.cs`: delete the Task 11 versions of `ImportEntries()` and `ExportEntries()` (now in `MainForm.Plugins.cs`) together with `ImportFromCsv`, `ImportFromAmc`, `ExportToCsv`, `ExportToHtml`, `ExportTo` (replaced by importers/exporters). `ImportFromFolder()` becomes `RunImporter(ExtensionRegistry.BuiltInId, _extensions.All<IGameImporter>().OfType<FolderGameImporter>().First())`.
- `MainForm.cs` `OnLoad`: call `LoadExtensions();` after `OpenCatalog(...)`. `BindFrames()`: call `NotifyPanels();` at the end.
- `MainForm.View.cs` `HookDynamicMenus()`: add `mnuGameGetInfo.DropDownOpening += (_, _) => ReplaceDynamicItems(mnuGameGetInfo, InfoProviderItems());` — `ReplaceDynamicItems` inserts at index 0; change it to insert **after** the static items for this menu by giving `ReplaceDynamicItems` an optional `bool append = false` parameter (`int index = append ? menu.DropDownItems.Count : 0;`) and pass `append: true` here.

`PluginsForm` (Designer, namespace `GameCatalog.App.Forms`): `lvPlugins` (ListView, Details, `CheckBoxes = true`, FullRowSelect) with columns `colPluginName` (Nazwa/Name), `colPluginVersion` (Wersja/Version), `colPluginAuthor` (Autor/Author), `colPluginState` (Stan/State), `colPluginError` (Błąd/Error); `lblRestart` (`Zmiany zostaną zastosowane po ponownym uruchomieniu programu.` / `Changes take effect after restarting the program.`); OK/Cancel bar; `localizer`.
```csharp
using GameCatalog.App.Services;
using GameCatalog.Plugins.Hosting;

namespace GameCatalog.App.Forms;

/// <summary>Tools → Plugins: installed plugins with their state; check boxes enable/disable them.</summary>
public partial class PluginsForm : Form
{
    public PluginsForm(IReadOnlyList<PluginDescriptor> plugins, IReadOnlyCollection<string> disabledIds)
    {
        InitializeComponent();
        var t = AppServices.Languages;
        foreach (var p in plugins)
        {
            var item = new ListViewItem(p.Name) { Tag = p.Id, Checked = !disabledIds.Contains(p.Id) };
            item.SubItems.Add(p.Version);
            item.SubItems.Add(p.Author);
            item.SubItems.Add(t.Get("Plugin.State." + p.State));
            item.SubItems.Add(p.Error ?? "");
            lvPlugins.Items.Add(item);
        }
    }

    public IEnumerable<string> DisabledIds =>
        lvPlugins.Items.Cast<ListViewItem>().Where(i => !i.Checked).Select(i => (string)i.Tag!);
}
```
Strings: `Plugin.State.Loaded` Załadowany/Loaded, `Plugin.State.Disabled` Wyłączony/Disabled, `Plugin.State.Incompatible` Niezgodna wersja/Incompatible version, `Plugin.State.Failed` Błąd/Failed, `Plugin.Error` = `Plugin „{0}” zgłosił błąd:\n{1}` / `Plugin “{0}” reported an error:\n{1}`, `Plugin.LoadFailed` = `Nie załadowano pluginu „{0}”: {1}` / `Plugin “{0}” was not loaded: {1}`. Add `PluginsForm` to `DialogSmokeTests` with `new PluginsForm(Array.Empty<PluginDescriptor>(), Array.Empty<string>())`.

- [ ] **Step 5: Run tests and the app with the sample plugin**

Run: `dotnet build GameCatalog.slnx` (Debug deploys the sample into `src/GameCatalog.App/bin/Debug/net10.0-windows/Plugins/GameCatalog.Plugin.Sample`).
Run: `dotnet test GameCatalog.slnx` → all PASS.
Run the app and check: File → Import lists the three built-in formats plus "Z pliku JSON (przykładowy plugin)…"; Export lists CSV, HTML and JSON; JSON export→import round-trips; Game → Get information shows "Przykładowy dostawca danych" which fills Comments through the results window; Tools shows "Zaznacz gry bez okładki" and it checks the right games; the "Podsumowanie" tab follows the selection; Tools → Pluginy lists the sample as loaded; unchecking it and restarting removes all its entries. Delete `GameCatalog.Plugin.Sample.dll` and replace it with a text file → app starts, Plugins window shows the failure.

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "feat(app): built-in formats as extensions; plugins add formats, providers, commands and tabs

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 16: Language selection, translation template, documentation, final verification

**Files:**
- Create: `src/GameCatalog.App/Forms/LanguageSelectForm.cs` + `.Designer.cs`
- Modify: `src/GameCatalog.UI/Components/Localizer.cs` (registry of live localizers), `src/GameCatalog.Localization/LanguageManager.cs` (template also contains default-language form keys), `Program.cs` (first-run selection), `MainForm.Designer.cs` + `MainForm.Tools.cs` (`actLanguage`, `actHelpExportTranslation`), `pl.json`, `en.json`, `CLAUDE.md`
- Test: `tests/GameCatalog.Localization.Tests/LanguageManagerTests.cs` (+1), `tests/GameCatalog.UI.Tests/LocalizerTests.cs` (+1), `tests/GameCatalog.App.Tests/DialogSmokeTests.cs` (+1 row)

**Interfaces:**
- Consumes: `LanguageManager`, `Localizer`, `PrefsStore.Language`.
- Produces: `Localizer.CollectAllLiveKeys() : IReadOnlyList<FormKey>` (static); `LanguageSelectForm(IReadOnlyList<LanguageInfo> languages, string selectedCode)` with `SelectedCode`.

- [ ] **Step 1: Write failing tests**

Add to `LanguageManagerTests`:
```csharp
    [Fact]
    public void ExportTemplate_IncludesDefaultLanguageFormKeysNotCollectedLive()
    {
        var m = CreateDefault();
        m.SetLanguage("en");
        using var doc = JsonDocument.Parse(m.ExportTemplate(Array.Empty<FormKey>()));
        var main = doc.RootElement.GetProperty("forms").GetProperty("MainForm");
        Assert.Equal("&File", main.GetProperty("mnuFile.Text").GetString());
        Assert.Equal("Katalog", main.GetProperty("$this.Text").GetString());
    }
```
Add to `LocalizerTests`:
```csharp
    [Fact]
    public void CollectAllLiveKeys_IncludesEveryLiveContainer()
    {
        Localizer.Translator = new FakeTranslator();
        using var f = new SampleForm();
        Assert.Contains(Localizer.CollectAllLiveKeys(), k => k.Form == "SampleForm" && k.Key == "lblHello.Text");
    }
```
Add to `DialogSmokeTests.Dialogs()`:
```csharp
        new object[] { (Func<Form>)(() => new LanguageSelectForm(new[] { new LanguageInfo("pl", "Polski", "", ""), new LanguageInfo("en", "English", "", "") }, "pl")) },
```
Run: `dotnet test GameCatalog.slnx` → Expected: FAIL (template lacks default form keys; `CollectAllLiveKeys` and `LanguageSelectForm` missing).

- [ ] **Step 2: Implement**

`LanguageManager.ExportTemplate` — after the `formKeys` loop and before the strings loop insert:
```csharp
        // Keys known to the default language but not collected from live windows (closed dialogs).
        foreach (var (form, keys) in _default.Forms)
        {
            if (!template.Forms.TryGetValue(form, out var section))
                template.Forms[form] = section = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var (key, value) in keys)
                section.TryAdd(key, GetFormValue(form, key) ?? value);
        }
```
`Localizer` — add:
```csharp
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Localizer, object> Live = new();

    /// <summary>Translatable keys of every window currently alive (for the translation template).</summary>
    public static IReadOnlyList<FormKey> CollectAllLiveKeys() =>
        Live.Select(entry => entry.Key).SelectMany(l => l.CollectKeys()).Distinct().ToList();
```
and in the `ContainerControl` setter, after `_container = value;`: `if (_container is not null) Live.AddOrUpdate(this, this);`.

`LanguageSelectForm` (Designer, AMC languageselect.dfm): fixed dialog 360×300, `lblChoose` (`Wybierz język interfejsu:` / `Choose the interface language:`), `lvLanguages` (ListView Details, FullRowSelect, `MultiSelect = false`, columns `colLanguageName` Język/Language, `colLanguageAuthor` Autor/Author), OK/Cancel bar; `localizer`; caption `Język` / `Language`.
```csharp
using GameCatalog.Localization;

namespace GameCatalog.App.Forms;

/// <summary>Interface language selection (AMC languageselect.pas); shown at first start and from Tools → Language.</summary>
public partial class LanguageSelectForm : Form
{
    public LanguageSelectForm(IReadOnlyList<LanguageInfo> languages, string selectedCode)
    {
        InitializeComponent();
        foreach (var language in languages)
        {
            var item = new ListViewItem(language.Name) { Tag = language.Code };
            item.SubItems.Add(language.Author);
            lvLanguages.Items.Add(item);
            if (language.Code.Equals(selectedCode, StringComparison.OrdinalIgnoreCase)) item.Selected = true;
        }
        lvLanguages.DoubleClick += (_, _) =>
        {
            DialogResult = DialogResult.OK;
            Close();
        };
    }

    public string? SelectedCode => lvLanguages.SelectedItems.Count > 0 ? (string)lvLanguages.SelectedItems[0].Tag! : null;
}
```

`Program.Main` — after creating `AppServices.Languages`, before `SetLanguage`:
```csharp
        if (string.IsNullOrEmpty(AppServices.Prefs.Language))
        {
            // First start: preselect the system language when a translation exists.
            var system = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var preselect = AppServices.Languages.Available.Any(l => l.Code == system) ? system : LanguageManager.DefaultLanguageCode;
            AppServices.Languages.SetLanguage(preselect);
            GameCatalog.UI.Components.Localizer.Translator = AppServices.Languages;
            using var choose = new Forms.LanguageSelectForm(AppServices.Languages.Available, preselect);
            AppServices.Prefs.Language = choose.ShowDialog() == DialogResult.OK && choose.SelectedCode is { } code ? code : preselect;
            AppServices.Prefs.Save();
        }
```

Main window actions (Designer + `MainForm.Tools.cs`): `actLanguage` (Key `Language`, Icon `Languages`, in `mnuTools` before `actPreferences`), `actHelpExportTranslation` (Key `HelpExportTranslation`, Icon `Languages`, in `mnuHelp` after `actHelpVersions`).
```csharp
    private void actLanguage_Execute(object? sender, EventArgs e)
    {
        using var form = new LanguageSelectForm(AppServices.Languages.Available, AppServices.Languages.CurrentLanguage);
        if (form.ShowDialog(this) != DialogResult.OK || form.SelectedCode is not { } code) return;
        AppServices.Languages.SetLanguage(code);   // every open window re-translates itself
        _prefs.Language = code;
        _prefs.Save();
    }

    private void actHelpExportTranslation_Execute(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog
        {
            Filter = L.Get("Filter.Json"),
            FileName = AppServices.Languages.CurrentLanguage + ".json",
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        File.WriteAllText(dialog.FileName, AppServices.Languages.ExportTemplate(Localizer.CollectAllLiveKeys()));
        SetHint(L.Get("Status.Exported", dialog.FileName));
    }
```
Strings: `Action.Language` `&Język…`/`&Language…`, `Action.Language.Hint` `Język interfejsu`/`Interface language`, `Action.HelpExportTranslation` `Eksportuj szablon &tłumaczenia…`/`Export translation &template…`, `Action.HelpExportTranslation.Hint` `Zapisz plik językowy do przetłumaczenia`/`Save a language file for translators`, `Filter.Json` `JSON (*.json)|*.json`/`JSON (*.json)|*.json`.

- [ ] **Step 3: Update `CLAUDE.md`**

Replace the Architecture/Conventions sections to describe the new layout: project graph (spec §2), `Directory.Build.props`, test projects and commands (`dotnet test GameCatalog.slnx`, single test filter), Designer conventions (naming = translation keys, `Localizer` on every form/frame, `UIAction`s via `ActionList`), language files (`Languages/*.json`, `pl` default, `LanguageFilesTests`/`SourceRulesTests` rules), plugin model (contracts, `Plugins\<Name>\<Name>.dll`, `PluginApi.Version`, sample plugin), the "adding a field" checklist updated (`GameFields.All`, `GameFieldValues`, `GameFieldWriter`, SQLite schema + `ALTER TABLE`, `GameFrame` designer + `_editors` map, `pl.json`/`en.json` `Field.<Tag>`), and the staged port roadmap (stages 2–9 from the spec).

- [ ] **Step 4: Final verification**

Run: `dotnet build GameCatalog.slnx -c Release` → succeeds with 0 errors.
Run: `dotnet test GameCatalog.slnx` → all PASS; note the count per project in the commit message body.
Manual, with screenshots saved under `docs/screenshots/stage1/` (pl and en):
1. Delete `Language` from `prefs.json` → first start shows the language window, preselected by the system language.
2. Tools → Language switches PL ↔ EN live: main window, frames, open dialogs, status bar, menus of the sample plugin.
3. Help → Export translation template writes a JSON containing `forms.MainForm`, `forms.PrefsForm` (closed dialog, from the default language) and all `strings`.
4. Open the user's existing `Documents\GameCatalog\catalog.gc` → all games, extras, covers, loans and custom fields are present; YES/NO custom fields from the old version show as checked.
5. Every spec §9 criterion holds.

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "feat(app): language selection, translation template export; update CLAUDE.md

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

## Spec coverage map

| Spec section | Tasks |
|---|---|
| §2 solution structure, .NET 10, Directory.Build.props | 1 |
| §2 Core reorganization, labels as keys | 2 |
| §3 language files, LanguageManager, fallback, plugin keys | 5, 6, 15 |
| §3 Localizer component | 7 |
| §3 language selection, first run, template export | 16 |
| §4 contracts, hosting, version check, isolation, Plugins window | 14, 15 |
| §4 built-in formats as extensions | 15 |
| §4 sample plugin | 14 |
| §5 MainForm layout, frames, components, services | 7–11 |
| §5 dialogs in the Designer | 12, 13 |
| §6 error handling (global, plugins, saving, language files, WebView2) | 6, 9, 11, 14, 15 |
| §7 tests | every task; rules guarded in 6 and 13 |
| §9 completion criteria | 16 |
