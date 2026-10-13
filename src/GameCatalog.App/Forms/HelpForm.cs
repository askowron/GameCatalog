namespace GameCatalog.App;

/// <summary>Pomoc (indeks) i historia wersji — odpowiednik AMC help/about.</summary>
public sealed class HelpForm : Form
{
    public HelpForm()
    {
        Text = "Indeks pomocy";
        Width = 640; Height = 480;
        StartPosition = FormStartPosition.CenterParent;
        var topics = new ListBox { Dock = DockStyle.Left, Width = 170 };
        var text = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true };
        var help = new Dictionary<string, string>
        {
            ["Pierwsze kroki"] = "GameCatalog przechowuje gry w pliku .gc (SQLite, domyślnie Dokumenty\\GameCatalog\\catalog.gc).\n\n• Gra > Dodaj (Alt+Insert) — nowa gra, edycja pól bezpośrednio w zakładce „Pola gry”.\n• Zmiany zapisuj Plik > Zapisz (Ctrl+S).\n• Okładkę dodasz w panelu pod listą (Obrazki > Wczytaj).",
            ["Pola gry"] = "Pola przeportowano z Ant Movie Catalog:\nDirector→Developer, Producer→Wydawca, Writer→Projektant, Actors→Aktorzy głosowi, Category→Gatunek, Certification→PEGI, Length→Czas gry, Extras→Dodatki/DLC.\nNowe pola: Platforma, Status (Do zagrania/Ogrywana/Ukończona…), Ostatnio grano.\nPola własne zdefiniujesz w Narzędzia > Zarządzanie polami.",
            ["Lista i grupy"] = "• Kliknij nagłówek kolumny, aby sortować; Wyświetlaj > Sortuj wg zmienia pole.\n• Wyświetlaj > Grupuj wg grupuje gry jak drzewo w AMC.\n• Checkbox = zaznaczenie gry; menu kontekstowe = etykieta koloru (jak w AMC).\n• Gra > Losowy (F8) losuje grę, Gra > Zmień numer (F2) zmienia numer.",
            ["Szukanie"] = "Gra > Znajdź (Ctrl+F) pokazuje pasek szukania jak w AMC:\n• wybierz pole i wpisz wartość (szukanie wyrażeń nie jest wspierane),\n• „Całe pole” = dokładne dopasowanie, „Pokaż wyniki” = filtr listy,\n• „Odwróć wyniki” = negacja, „Znajdź następny” = kolejna pozycja.",
            ["Import i eksport"] = "• Plik > Import: z folderu (podfolder = gra), z CSV, z AMC XML (mapowanie pól filmowych na growe).\n• Gra > Pobierz informacje > Z plików — uzupełnia dane z folderu.\n• Gra > Szukaj w Internecie — wyszukiwanie tytułu w Google/Steam/MobyGames/HLTB.\n• Plik > Eksport: CSV, HTML. Szablon karty: Wyświetlaj > Wybierz HTML, edycja: Narzędzia > Edytor HTML.",
            ["Statystyki"] = "Narzędzia > Statystyki: Informacje ogólne albo wykres dla każdego pola (słupkowy dla liczb i dat, kołowy dla tekstów).\nZakres: Wszystkie/Wybrane/Zaznaczone/Widoczne. Opcje: legenda, etykiety, puste miesiące, grupowanie wartości. Wykres zapiszesz (PNG/BMP/JPEG/WMF) lub skopiujesz.",
            ["Wypożyczenia"] = "Narzędzia > Pożyczone (F9): pożyczanie gier ze zwrotem i historią — jak loan.pas w AMC.",
            ["Skrypty"] = "Skrypty C# (.gcsx w Dokumenty\\GameCatalog\\Scripts) pobierają dane z internetu — jak skrypty .ifs w AMC.\n\n• Gra > Pobierz informacje > Ze skryptu (F6) — uruchom dla bieżącej gry, potem zaznacz zmiany do zastosowania.\n• Narzędzia > Skrypty (Shift+F6) — menedżer: nowy/edycja/usuwanie + uruchomienie testowe z dziennikiem.\n• W skrypcie dostępny jest obiekt Api: GetPage/PostPage/GetPicture, SetField/GetField, AddExtra, PickList, RegexAll/Between, Log, SearchLinks (DuckDuckGo + Bing).\n• JSON: var root = Api.JsonRoot(json, out var doc); ...; doc.Dispose(); (using var nie działa w skryptach).\n• Dołączone skrypty: Steam, Wikipedia PL i GRY-Online.pl (działają od razu), IGDB (oficjalne API — wymaga darmowych kluczy Twitch wpisanych w skrypcie).",
            ["Skróty"] = "Ctrl+N/O/S, Ctrl+Shift+S, Ctrl+P, Ctrl+I, Ctrl+E, Alt+Enter, Alt+F4, Alt+Ins, Alt+Del, Ctrl+F, F8, F2, Ctrl+Shift+C/V/Z, F11, Shift+F4, F4, F12, Ctrl+F12, F10, F9, Shift+Ctrl+F12, F1.",
        };
        foreach (var k in help.Keys) topics.Items.Add(k);
        topics.SelectedIndexChanged += (_, _) =>
        {
            if (topics.SelectedItem is string k && help.TryGetValue(k, out var v)) text.Text = v;
        };
        topics.SelectedIndex = 0;
        var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 170 };
        split.Panel1.Controls.Add(topics);
        split.Panel2.Controls.Add(text);
        var b = new Button { Text = "Zamknij", DialogResult = DialogResult.Cancel, Dock = DockStyle.Bottom, Height = 36 };
        Controls.Add(split);
        Controls.Add(b);
        CancelButton = b;
    }
}

public sealed class VersionsForm : Form
{
    public VersionsForm()
    {
        Text = "Wersje";
        Width = 520; Height = 360;
        StartPosition = FormStartPosition.CenterParent;
        var text = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, Text =
            "GameCatalog 1.x — port Ant Movie Catalog na C# (.NET + WinForms + SQLite).\n\n" +
            "1.5 — Mechanizm skryptów jak w AMC (getscript.pas): skrypty C# (.gcsx)\n" +
            "      z obiektem Api (GetPage/SetField/AddExtra/PickList/SearchLinks),\n" +
            "      menedżer + edytor + podgląd zmian, skrypty Steam, Wikipedia PL,\n" +
            "      GRY-Online.pl oraz IGDB (oficjalne API, klucze Twitch).\n" +
            "1.4 — Menu jak w AMC: ostatnie pliki, Pobierz informacje, Szukaj w Internecie,\n" +
            "      Formatowanie grup, Pola tabeli, Wybierz HTML, Zmień numer (zamiana).\n" +
            "      Nowe okna: pola własne, obrazki, edytor HTML, preferencje (motywy ikon),\n" +
            "      pomoc, wersje. Rozszerzenie katalogu .gc.\n" +
            "1.3 — Statystyki jak w AMC (kategorie, zakres, wykresy słupkowe/kołowe).\n" +
            "1.2 — Układ AMC: lista z grupami, okładka, pola inline, dodatki,\n" +
            "      pasek szukania, oryginalne ikony (6 motywów).\n" +
            "1.1 — WinForms + SQLite: gry, DLC, wypożyczenia, CSV/HTML, import AMC XML.\n" +
            "1.0 — Pierwsza wersja: model danych growych (mapowanie pól filmowych)." };
        var b = new Button { Text = "Zamknij", DialogResult = DialogResult.Cancel, Dock = DockStyle.Bottom, Height = 36 };
        Controls.Add(text);
        Controls.Add(b);
        CancelButton = b;
    }
}
