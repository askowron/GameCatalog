# Etap 1 — Fundament portu AMC → GameCatalog

Data: 2026-10-14 · Status: do akceptacji

## 1. Cel i kontekst

GameCatalog to port Ant Movie Catalog (AMC, Delphi 7, źródła: `C:\Users\Adam\Downloads\amc_sources`) do C#/WinForms, przeniesiony z filmów na gry. Docelowo ma być lustrzanym odpowiednikiem AMC — maksymalnie podobnym wizualnie i funkcjonalnie.

Stan wyjściowy (commit `f632388`): ~6,8 tys. linii C# wobec ~64 tys. linii Pascala w AMC. Szkielet menu jest niemal kompletny, ale UI powstaje w kodzie, prawie cała logika siedzi w `MainForm.cs` (2,4 tys. linii), teksty są wpisane na sztywno po polsku, nie ma pluginów ani testów.

Cały port podzielono na etapy, każdy z osobnym spec → plan → implementacja:

1. **Fundament** (ten dokument)
2. Pola, dodatki (DLC), obrazki
3. Wyszukiwanie, filtr wyrażeń, sortowanie zaawansowane, formaty grup
4. Import (kreator `import2`)
5. Eksport (szablony HTML, CSV, XML, SQL, obrazki)
6. Skrypty (właściwości, debugger, picktree, wyniki dodatków)
7. Opcje programu i wartości domyślne
8. Wydruk i raporty (odpowiednik FreeReport)
9. Wypożyczenia, statystyki, numeracja, właściwości katalogu

Poza zakresem całego portu: AMCExchange, AMCReport.

### Wymagania użytkownika

- Prawidłowy hierarchiczny podział plików i klas.
- Kontrolki i komponenty wszędzie, gdzie to możliwe.
- Otwartość na pluginy i wielojęzyczność.
- Komentarze w kodzie po angielsku (istniejące polskie zostaną przetłumaczone).
- Lokalne repozytorium git (utworzone).

### Decyzje

| Temat | Decyzja |
|---|---|
| Platforma | `net10.0` / `net10.0-windows` (LTS) |
| Podejście | Stopniowa przebudowa (wariant A) — po każdym kroku program się buduje i działa |
| Lokalizacja | Zewnętrzne pliki JSON w `Languages\`, przełączanie w locie |
| UI | Formy i UserControle w Designerze VS (`.Designer.cs` + `.resx`) |
| Podgląd HTML | WebView2 (`Microsoft.Web.WebView2`) |
| Format katalogu | Bez zmian: SQLite `.gc`; AMC XML przez import/eksport |
| Pluginy | Importery/eksportery, komendy menu/paska, źródła danych o grach, panele/zakładki UI |
| Testy | xUnit dla warstw bez UI; UI weryfikowane ręcznie |

## 2. Struktura solucji

```
GameCatalog.slnx
Directory.Build.props                  wspólne: Nullable, ImplicitUsings, LangVersion, wersja
src/
  GameCatalog.Core/                     net10.0, bez UI
    Models/        Game, GameExtra, LoanRecord, CustomFieldDefinition, Enums
    Fields/        GameFields, GameFieldValues            (= AMC fields.pas)
    Catalog/       GameList (= TMovieList): filtr / sortowanie / grupowanie, NaturalStringComparer
    Services/      StatsService, SampleDataFactory
  GameCatalog.Localization/             net10.0, bez UI
  GameCatalog.Plugins.Abstractions/     net10.0              (= AMC interfaces.pas)
  GameCatalog.Plugins.Abstractions.WinForms/  net10.0-windows
  GameCatalog.Plugins.Hosting/          net10.0
  GameCatalog.Storage/                  SQLite, AMC XML, CSV, HTML
  GameCatalog.Scripting/                Roslyn, .gcsx
  GameCatalog.UI/                       net10.0-windows — biblioteka kontrolek
    Components/    ActionList/UIAction, Localizer, IconThemeImageList
    Controls/      GameListView, HtmlPreview, ThumbnailView, …
    Frames/        GameFrame, GameCustomFrame, GameExtrasFrame, GameExtraFrame, PictureToolWindow
  GameCatalog.App/                      exe (WinExe)
    Program.cs, MainForm
    Forms/         okna dialogowe (= dialogi AMC)
    Services/      CatalogSession, UndoService, GameListPresenter
    Languages/     pl.json, en.json
    Resources/     motywy ikon AMC (*.bmp)
tests/
  GameCatalog.Core.Tests
  GameCatalog.Storage.Tests
  GameCatalog.Localization.Tests
  GameCatalog.Plugins.Hosting.Tests
samples/
  GameCatalog.Plugin.Sample/
```

Zależności (A → B: A zależy od B):

```
Localization                 → (nic)
Core                         → (nic)
Plugins.Abstractions         → Core, Localization
Plugins.Abstractions.WinForms→ Plugins.Abstractions
Plugins.Hosting              → Plugins.Abstractions
Storage                      → Core, Plugins.Abstractions   (wbudowane importery/eksportery)
Scripting                    → Core, Plugins.Abstractions   (wspólny GameInfoResult)
UI                           → Core, Localization
App                          → wszystkie powyższe
```

`Core` i `Localization` nie znają się nawzajem; etykiety zależne od języka (nazwy pól, statusów, „(brak)”) Core udostępnia jako **klucze** (`Field.Developer`, `Status.Backlog`, `Group.None`), a tłumaczy je warstwa wyżej.

Nazewnictwo ramek odpowiada plikom AMC, co ułatwia weryfikację wierności portu (mapowanie w sekcji 5).

## 3. Lokalizacja

Odwzorowanie mechanizmu AMC (`TAntTranslator` + pliki `.lng`, tłumaczenie form po nazwach komponentów, ponowne tłumaczenie otwartych okien po zmianie języka).

### Pliki

`Languages\<kod>.json` obok exe. Na start `pl.json` (domyślny) i `en.json`.

```json
{
  "language": { "name": "Polski", "code": "pl", "author": "…", "version": "1.0" },
  "forms": {
    "MainForm":  { "mnuFile.Text": "&Plik", "tbAdd.ToolTipText": "Dodaj grę" },
    "GameFrame": { "lblDeveloper.Text": "Developer" }
  },
  "strings": { "Msg.SaveChanges": "Zapisać zmiany w katalogu?" }
}
```

### `GameCatalog.Localization`

- `LanguageManager` — wczytuje pliki z katalogu, lista dostępnych języków, bieżący język, zdarzenie `LanguageChanged`.
- `ITranslator`:
  - `string Get(string key, params object[] args)` — sekcja `strings`, z formatowaniem.
  - `string? GetFormValue(string form, string key)` — sekcja `forms`.
- Fallback: bieżący język → język domyślny (`pl`) → sam klucz (w `Get`) lub `null` (w `GetFormValue`, wtedy zostaje tekst z Designera).
- Brakujący/uszkodzony plik → fallback + wpis w logu, bez wyjątku na zewnątrz.
- Scalanie kluczy pluginów pod prefiksem `Plugin.<Id>.`.

### `Localizer` (komponent w `GameCatalog.UI`)

- Upuszczany na formę/ramkę w Designerze; nazwa formy = klucz sekcji w `forms`.
- Na `LanguageChanged` (i przy pierwszym pokazaniu) przechodzi rekursywnie po: kontrolkach, `MenuStrip`/`ToolStrip`/`ContextMenuStrip` (z podmenu), `StatusStrip`, kolumnach `ListView`, zakładkach `TabControl`; ustawia `Text` i `ToolTipText` wg klucza `NazwaKontrolki.Właściwość`.
- Tekst z Designera = wartość zapasowa.
- Ramki (UserControle) mają własny `Localizer` z własną sekcją.

### UI

- Narzędzia → Język… oraz okno wyboru przy pierwszym uruchomieniu (= `languageselect.pas`); zmiana bez restartu; wybór zapisywany w preferencjach.
- Pomoc → „Eksportuj szablon tłumaczenia” — zapisuje JSON ze wszystkimi kluczami form (zebranymi przez `Localizer`) i `strings`, z wartościami bieżącego języka.

## 4. Pluginy

### Ładowanie (`GameCatalog.Plugins.Hosting`)

- Układ: `Plugins\<Nazwa>\<Nazwa>.dll` (+ zależności, opcjonalnie `Languages\`).
- Każdy plugin we własnym `AssemblyLoadContext`; `GameCatalog.Core`, `GameCatalog.Localization`, `GameCatalog.Plugins.Abstractions(.WinForms)` i `System.Windows.Forms` rozwiązywane z kontekstu domyślnego (wspólne typy).
- Wykrywanie: klasy implementujące `IPlugin` z atrybutem `[GameCatalogPlugin(id, name, version)]`.
- `PluginApi.Version` w Abstractions; plugin deklaruje wymaganą wersję — niezgodny jest pomijany (wpis w logu, widoczny w oknie Pluginy).
- Każde wywołanie kodu pluginu owinięte w obsługę wyjątków: komunikat z nazwą pluginu, program działa dalej.
- Włączanie/wyłączanie w oknie Narzędzia → Pluginy… (lista: nazwa, wersja, autor, stan, błąd); stan w preferencjach, skutek po restarcie.

### Kontrakty (`GameCatalog.Plugins.Abstractions`)

```csharp
[AttributeUsage(AttributeTargets.Class)]
public sealed class GameCatalogPluginAttribute(string id, string name, string version) : Attribute;

public interface IPlugin
{
    void Initialize(IPluginHost host);   // register importers, exporters, commands, providers
}

public interface IPluginHost
{
    IGameCatalog Catalog { get; }        // read/write facade over the catalog
    IGameSelection Selection { get; }    // current / selected / checked games + change events
    ITranslator Translator { get; }
    string DataDirectory { get; }        // per-plugin settings folder
    void Log(string message);
    void Register(IGameImporter importer);
    void Register(IGameExporter exporter);
    void Register(IGameInfoProvider provider);
    void Register(ICatalogCommand command);
}

public interface IGameImporter
{
    string Name { get; }
    string FileFilter { get; }
    IEnumerable<Game> Import(ImportContext context);      // path, start number, options
}

public interface IGameExporter
{
    string Name { get; }
    string FileFilter { get; }
    void Export(IEnumerable<Game> games, ExportContext context);
}

public interface IGameInfoProvider
{
    string Name { get; }
    Task<GameInfoResult> FetchAsync(Game game, IInfoProviderUi ui, CancellationToken ct);
}

public interface ICatalogCommand
{
    string Id { get; }
    string MenuPath { get; }             // e.g. "Tools", "Game/Extras"
    string Text { get; }
    byte[]? Icon { get; }                // PNG; no WinForms dependency here
    string? Shortcut { get; }            // e.g. "Ctrl+Shift+I"; no WinForms Keys type here
    bool CanExecute(IPluginHost host);
    void Execute(IPluginHost host);
}
```

- `GameInfoResult` = dzisiejszy `ScriptResult` (pola, pola własne, dodatki, okładka, log), przeniesiony do Abstractions i współdzielony ze Scripting — wyniki skryptów `.gcsx` i providerów trafiają do tego samego okna wyników.
- `IInfoProviderUi` — wybór z listy (`PickList`), komunikaty, postęp (bez typów WinForms).
- `IGameCatalog` — odczyt gier, dodawanie, modyfikacja z oznaczeniem zmian (dirty) i odświeżeniem widoku.

### `GameCatalog.Plugins.Abstractions.WinForms`

```csharp
public interface IGamePanelPlugin
{
    string Title { get; }
    Control CreatePanel(IPluginHost host);    // shown as a tab in the game info panel
    void OnCurrentGameChanged(Game? game);
}
```

Rejestracja przez `IWinFormsPluginHost : IPluginHost` z metodą `Register(IGamePanelPlugin panel)`, zdefiniowany w tym samym assembly. Host w App go implementuje; plugin z panelem rzutuje `host` na `IWinFormsPluginHost`.

### Funkcje wbudowane

Istniejące importery (CSV, AMC XML, folder) i eksportery (CSV, HTML) stają się wewnętrznymi implementacjami `IGameImporter`/`IGameExporter` w `GameCatalog.Storage`, rejestrowanymi tak jak pluginy. Menu Import/Eksport budowane jest z rejestru.

### Przykład

`samples/GameCatalog.Plugin.Sample` — po jednej implementacji każdego punktu rozszerzeń (importer JSON, eksporter JSON, komenda „Zaznacz gry bez okładki”, provider zwracający dane testowe, zakładka z podsumowaniem gry). Kopiowany do `Plugins\` przy buildzie Debug; używany w testach `Plugins.Hosting.Tests`.

## 5. Okno główne, ramki, komponenty

### Układ `MainForm` (Designer), wg `main.dfm`

- `MenuStrip` (Plik, Gra, Obrazek, Widok, Narzędzia, Pomoc — jak dziś).
- `ToolStripContainer` z przesuwalnymi paskami (= doki TBX): pasek główny, pasek obrazka, pasek dodatków; widoczność z menu Widok.
- Lewa strona (`PanelLeft`): `GameListView` + dolny dok z `PictureToolWindow` / `ThumbnailView`.
- Prawa strona (`PanelGameInfos`): zakładki Gra / Pola własne / Dodatki / zakładki pluginów, zamiennie `HtmlPreview`.
- `StatusStrip`.

### Ramki (`GameCatalog.UI/Frames`)

| AMC | GameCatalog |
|---|---|
| `framemovie.pas` (PanelMain / PanelMedia / PanelVideo) | `GameFrame` — sekcje: główne / nośnik i źródło / technikalia |
| `framemoviecustom.pas` | `GameCustomFrame` — pola własne z definicji |
| `framemovieextras.pas` | `GameExtrasFrame` — lista DLC + szczegóły |
| `frameextra.pas` | `GameExtraFrame` |
| okno obrazka (`ToolbarPictureWindow`) | `PictureToolWindow` |
| `rkSmartView` (`ThumbsViewer`) | `ThumbnailView` |
| `THTMLViewer` | `HtmlPreview` (WebView2) |
| `TElTree` (`ListView1`) | `GameListView` |

Ramki udostępniają `Bind(Game?)`, `Commit()` (zapis edycji do modelu) i zdarzenie `Modified`; nie znają `CatalogSession`.

`GameFieldsControl` (dziś) zostaje rozdzielony między `GameFrame` (pola stałe) i `GameCustomFrame` (pola własne).

### Komponenty (`GameCatalog.UI/Components`, w Toolbox)

- **`ActionList` + `UIAction`** (= `TActionList`): akcja ma `Text`, `Image`/`ImageIndex`, `ShortcutKeys`, `Enabled`, `Checked`, `Visible`, zdarzenia `Execute` i `Update`. `ActionList` jest `IExtenderProvider` dodającym `ToolStripItem.Action` w Designerze — powiązane pozycje dziedziczą tekst, ikonę, skrót i stan. `ActionList.UpdateActions()` wywoływane na `Application.Idle` (jak w Delphi). Komendy pluginów opakowywane w `UIAction`.
- **`Localizer`** — sekcja 3.
- **`IconThemeImageList`** — ładuje pasek ikon motywu AMC z `Resources\<motyw>.bmp`, kolejność wg `AmcIcon`; zmiana motywu podmienia obrazy we wszystkich powiązanych akcjach.

### Serwisy (`GameCatalog.App/Services`)

- **`CatalogSession`** — bieżący plik, `GameList`, wypożyczenia, `IsDirty`, `New/Open/Save/SaveAs`, kopia `.bak`, ostatnie pliki; zdarzenia `Opened`, `Saved`, `DirtyChanged`.
- **`UndoService`** — stos cofania (dziś w `MainForm`).
- **`GameListPresenter`** — z `GameList` + filtr/sortowanie/grupowanie/format tytułu wylicza widok dla `GameListView`, synchronizuje zaznaczenie i bieżącą grę.
- `IPluginHost` implementowany w App na bazie powyższych serwisów.

`MainForm` jedynie łączy akcje z serwisami. Cel: żaden plik formy nie przekracza kilkuset linii.

## 6. Obsługa błędów

- `Application.ThreadException` i `AppDomain.CurrentDomain.UnhandledException` → przetłumaczony komunikat + zapis do `Documents\GameCatalog\Logs\gamecatalog.log`.
- Błędy pluginów i skryptów izolowane (sekcja 4).
- Zapis katalogu w transakcji (jak dziś); `.bak` tworzony przed zapisem.
- Problemy z plikami językowymi → fallback + log (sekcja 3).
- WebView2 niedostępny (brak runtime) → `HtmlPreview` pokazuje komunikat z linkiem do instalacji; reszta programu działa.

## 7. Testy

xUnit w `tests/`:

- **Core.Tests**: filtr, sortowanie (w tym naturalne), grupowanie, `GameFieldValues`, `Game.Clone`, `NextNumber`.
- **Storage.Tests**: cykl zapis→odczyt SQLite (plik tymczasowy, wszystkie tabele), import AMC XML (mała próbka w zasobach testu), CSV eksport→import.
- **Localization.Tests**: wczytywanie, fallback, brakujący klucz, uszkodzony plik, scalanie kluczy pluginu.
- **Plugins.Hosting.Tests**: załadowanie przykładowego pluginu, rejestracja wszystkich typów rozszerzeń, odrzucenie niezgodnej wersji API, wyjątek w `Initialize` nie przerywa ładowania innych.

UI weryfikowane ręcznie: uruchomienie i zrzuty ekranu po każdym kroku.

Polecenia: `dotnet build GameCatalog.slnx`, `dotnet test GameCatalog.slnx`, pojedynczy test: `dotnet test --filter "FullyQualifiedName~<Nazwa>"`.

## 8. Kolejność prac

Każdy krok kończy się commitem; po każdym program się buduje, testy przechodzą, aplikacja się uruchamia.

1. `net10.0(-windows)`, `Directory.Build.props`, nowe (puste) projekty i projekty testów w solucji.
2. Reorganizacja Core (`Fields/`, `GameList`), angielskie komentarze w Core / Storage / Scripting; testy Core i Storage.
3. `GameCatalog.Localization` + testy; `pl.json` / `en.json`; klucze zamiast polskich etykiet w Core.
4. `GameCatalog.UI/Components`: `ActionList`/`UIAction`, `Localizer`, `IconThemeImageList`.
5. Serwisy `CatalogSession`, `UndoService`, `GameListPresenter` wydzielone z `MainForm`.
6. Kontrolki i ramki: `GameListView`, `HtmlPreview`, `ThumbnailView`, `PictureToolWindow`, `GameFrame`, `GameCustomFrame`, `GameExtrasFrame`, `GameExtraFrame`.
7. Nowa `MainForm` w Designerze z ramek i akcji; pozostałe dialogi przeniesione do Designera z `Localizer`; angielskie komentarze w App.
8. `Plugins.Abstractions(.WinForms)`, `Plugins.Hosting` + testy; wbudowane importery/eksportery jako implementacje kontraktów; przykładowy plugin; okno Pluginy….
9. Okno wyboru języka, eksport szablonu tłumaczenia, aktualizacja `CLAUDE.md`.

## 9. Kryteria ukończenia

- Wszystkie funkcje dostępne dziś działają co najmniej tak samo; istniejący `catalog.gc` otwiera się bez utraty danych.
- Wygląd okna głównego odpowiada układowi AMC (sekcja 5).
- Przełączanie PL ↔ EN w locie obejmuje okno główne, ramki i dialogi.
- Przykładowy plugin ładuje się, a każdy jego punkt rozszerzeń jest widoczny i działa w UI.
- `dotnet test` przechodzi.
- Wszystkie komentarze w kodzie po angielsku.
