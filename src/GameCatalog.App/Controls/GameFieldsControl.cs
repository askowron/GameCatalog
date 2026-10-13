using GameCatalog.Core.Models;

namespace GameCatalog.App;

/// <summary>Ramka pól gry (zakładka „Pola gry”) jako komponent — płaski formularz w stylu AMC.
/// Zgłasza <see cref="FieldCommitted"/> zamiast bezpośrednio zapisywać do modelu;
/// edytory dostępne przez <see cref="Editors"/> (klucze-tagy jak w <c>GameFields</c>).</summary>
public sealed class GameFieldsControl : UserControl
{
    public Dictionary<string, Control> Editors { get; } = new();

    /// <summary>Użytkownik opuścił/zmienił pole — argument to tag pola (np. „Title”).</summary>
    public event Action<string>? FieldCommitted;

    /// <summary>Podpięty przez okno główne — przycisk „…” przy adresie WWW.</summary>
    public Action? OpenUrlRequested;

    public GameFieldsControl()
    {
        Dock = DockStyle.Fill;
        AutoScroll = true;
        BackColor = SystemColors.Window;
        Padding = new Padding(8);
    }

    public void FocusTitle()
    {
        if (Editors.TryGetValue("Title", out var fc)) { fc.Focus(); if (fc is TextBox ft) ft.SelectAll(); }
    }

    private void Raise(string tag) => FieldCommitted?.Invoke(tag);

    private Panel FieldRow(Control host, int height = 26)
    {
        var p = new Panel { Dock = DockStyle.Top, Height = height };
        host.Controls.Add(p);
        return p;
    }

    private static Label FieldLabel(Panel row, string text, int width = 130)
    {
        var l = new Label { Text = text, Dock = DockStyle.Left, Width = width, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };
        row.Controls.Add(l);
        return l;
    }

    private void FieldFill<T>(Panel row, string tag, T editor) where T : Control
    {
        editor.Dock = DockStyle.Fill;
        row.Controls.Add(editor);
        Editors[tag] = editor;
        if (editor is CheckBox cb) cb.CheckedChanged += (_, _) => Raise(tag);
        else editor.Leave += (_, _) => Raise(tag);
    }

    private static Panel RightGroup(Panel row, int width, int gap = 8)
    {
        var g = new Panel { Dock = DockStyle.Right, Width = width, Height = row.Height };
        row.Controls.Add(g);
        // Odstęp między polem a grupką (dodawany po grupce, by po FixDockOrder wypadł z jej lewej).
        var s = new Panel { Dock = DockStyle.Right, Width = gap, Height = row.Height };
        row.Controls.Add(s);
        return g;
    }

    private static ComboBox DropList(IEnumerable<string>? items = null)
    {
        var c = new ComboBox { DropDownStyle = ComboBoxStyle.DropDown };
        if (items is not null) foreach (var x in items) c.Items.Add(x);
        return c;
    }

    private Control WithNone(NumericUpDown num, string checkTag)
    {
        var p = new Panel { Height = 26, Dock = DockStyle.Fill };
        num.Dock = DockStyle.Left;
        num.Width = 70;
        var suffix = new Label { Text = "/ 10", Dock = DockStyle.Left, Width = 30, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(2, 4, 0, 0) };
        var chk = new CheckBox { Text = "brak", AutoSize = true, Dock = DockStyle.Left };
        p.Controls.Add(num); p.Controls.Add(suffix); p.Controls.Add(chk);
        Editors[checkTag] = chk;
        chk.CheckedChanged += (_, _) => Raise(checkTag == "UserRatingNone" ? "UserRating" : "CriticRating");
        num.Leave += (_, _) => Raise(checkTag == "UserRatingNone" ? "UserRating" : "CriticRating");
        return p;
    }

    public void Build()
    {
        Controls.Clear();
        Editors.Clear();

        // --- Góra jak w AMC: lewe pola + prawy blok (nośnik/daty/oceny) ---
        var top = new Panel { Dock = DockStyle.Top, Height = 130 };
        Controls.Add(top);
        var topRight = new Panel { Dock = DockStyle.Right, Width = 320 };
        top.Controls.Add(topRight);
        top.Controls.Add(new Panel { Dock = DockStyle.Right, Width = 12 });
        var topLeft = new Panel { Dock = DockStyle.Fill };
        top.Controls.Add(topLeft);

        var r0 = FieldRow(topLeft); FieldLabel(r0, "Etykieta gry:"); FieldFill(r0, "MediaLabel", new TextBox());
        var r1 = FieldRow(topLeft); FieldLabel(r1, "Sklep / źródło:"); FieldFill(r1, "Store", DropList(Stores.All));
        var r2 = FieldRow(topLeft); FieldLabel(r2, "Pożyczona komu:"); FieldFill(r2, "Borrower", new TextBox());
        var r3 = FieldRow(topLeft); FieldLabel(r3, "Tytuł oryginalny:"); FieldFill(r3, "Title", new TextBox());
        var r4 = FieldRow(topLeft); FieldLabel(r4, "Tytuł polski:"); FieldFill(r4, "LocalizedTitle", new TextBox());

        var m0 = FieldRow(topRight); FieldLabel(m0, "Typ nośnika:", 108); FieldFill(m0, "MediaType", DropList(MediaTypes.All));
        var m1 = FieldRow(topRight); FieldLabel(m1, "Data dodania:", 108); FieldFill(m1, "DateAdded", new DateTimePicker { ShowCheckBox = true, Format = DateTimePickerFormat.Short });
        var m2 = FieldRow(topRight); FieldLabel(m2, "Data ukończenia:", 108); FieldFill(m2, "DateFinished", new DateTimePicker { ShowCheckBox = true, Format = DateTimePickerFormat.Short });
        var m3 = FieldRow(topRight); FieldLabel(m3, "Ocena własna:", 108); FieldFill(m3, "UserRating", WithNone(new NumericUpDown { Maximum = 10, DecimalPlaces = 1, Increment = 0.1M }, "UserRatingNone"));
        var m4 = FieldRow(topRight); FieldLabel(m4, "Ocena recenzji:", 108); FieldFill(m4, "CriticRating", WithNone(new NumericUpDown { Maximum = 10, DecimalPlaces = 1, Increment = 0.1M }, "CriticRatingNone"));

        // --- Ekipa jak Reżyser/… + Obsada (wąskie pola + wysokie memo obok) ---
        var crew = new Panel { Dock = DockStyle.Top, Height = 104 };
        Controls.Add(crew);
        var crewLeft = new Panel { Dock = DockStyle.Left, Width = 340 };
        crew.Controls.Add(crewLeft);
        crew.Controls.Add(new Panel { Dock = DockStyle.Left, Width = 12 });
        var crewRight = new Panel { Dock = DockStyle.Fill };
        crew.Controls.Add(crewRight);
        var c0 = FieldRow(crewLeft); FieldLabel(c0, "Developer:"); FieldFill(c0, "Developer", DropList());
        var c1 = FieldRow(crewLeft); FieldLabel(c1, "Wydawca:"); FieldFill(c1, "Publisher", DropList());
        var c2 = FieldRow(crewLeft); FieldLabel(c2, "Projektant:"); FieldFill(c2, "Designer", new TextBox());
        var c3 = FieldRow(crewLeft); FieldLabel(c3, "Kompozytor:"); FieldFill(c3, "Composer", new TextBox());
        crewRight.Controls.Add(new Label { Text = "Aktorzy głosowi:", Dock = DockStyle.Top, Height = 18 });
        var actors = new TextBox { Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical };
        crewRight.Controls.Add(actors);
        crewRight.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 4, BackColor = SystemColors.Window });
        Editors["VoiceActors"] = actors;
        actors.Leave += (_, _) => Raise("VoiceActors");

        // --- Kraj + rok + czas (jak Kraj / Rok prod. / Czas trwania) ---
        var rk = FieldRow(this); FieldLabel(rk, "Kraj:");
        var gk = RightGroup(rk, 275);
        FieldLabel(gk, "Rok wyd.:", 62);
        var year = new NumericUpDown { Maximum = 2100, Minimum = 0, Dock = DockStyle.Left, Width = 65 };
        gk.Controls.Add(year);
        Editors["ReleaseYear"] = year;
        year.Leave += (_, _) => Raise("ReleaseYear");
        FieldLabel(gk, "Czas gry:", 62);
        var hours = new NumericUpDown { Maximum = 100000, DecimalPlaces = 1, Dock = DockStyle.Left, Width = 65 };
        gk.Controls.Add(hours);
        Editors["PlaytimeHours"] = hours;
        hours.Leave += (_, _) => Raise("PlaytimeHours");
        var hLbl = new Label { Text = "h", Dock = DockStyle.Left, Width = 16, TextAlign = ContentAlignment.MiddleLeft };
        gk.Controls.Add(hLbl);
        FieldFill(rk, "Country", DropList());

        // --- Gatunek + PEGI (jak Gatunek / Certyfikacja) ---
        var rg = FieldRow(this); FieldLabel(rg, "Gatunek:");
        var gg = RightGroup(rg, 275);
        FieldLabel(gg, "PEGI:", 62);
        var pegi = DropList(new[] { "", "PEGI 3", "PEGI 7", "PEGI 12", "PEGI 16", "PEGI 18" });
        pegi.Dock = DockStyle.Fill;
        gg.Controls.Add(pegi);
        Editors["Pegi"] = pegi;
        pegi.Leave += (_, _) => Raise("Pegi");
        FieldFill(rg, "Genre", DropList());

        // --- Strona WWW ---
        var ru = FieldRow(this); FieldLabel(ru, "Strona WWW:");
        var urlHolder = new Panel { Dock = DockStyle.Fill };
        ru.Controls.Add(urlHolder);
        var urlBox = new TextBox { Dock = DockStyle.Fill };
        urlHolder.Controls.Add(urlBox);
        var urlBtn = new Button { Text = "…", Width = 30, Dock = DockStyle.Right };
        urlBtn.Click += (_, _) => OpenUrlRequested?.Invoke();
        urlHolder.Controls.Add(urlBtn);
        Editors["URL"] = urlBox;
        urlBox.Leave += (_, _) => Raise("URL");

        // --- Opis + komentarze (etykieta z lewej, memo wyrównane do reszty pól) ---
        var descBox = new Panel { Dock = DockStyle.Top, Height = 110 };
        Controls.Add(descBox);
        descBox.Controls.Add(new Label { Text = "Opis:", Dock = DockStyle.Left, Width = 130, Padding = new Padding(0, 3, 0, 0) });
        var d = new TextBox { Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical };
        descBox.Controls.Add(d);
        Editors["Description"] = d;
        d.Leave += (_, _) => Raise("Description");

        var cmtBox = new Panel { Dock = DockStyle.Top, Height = 120 };
        Controls.Add(cmtBox);
        cmtBox.Controls.Add(new Label { Text = "Komentarze:", Dock = DockStyle.Left, Width = 130, Padding = new Padding(0, 3, 0, 0) });
        var c = new TextBox { Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical };
        cmtBox.Controls.Add(c);
        Editors["Comments"] = c;
        c.Leave += (_, _) => Raise("Comments");

        var rchk = FieldRow(this); FieldLabel(rchk, "Znacznik:");
        var chk = new CheckBox { Text = "Zaznaczona", AutoSize = true, Dock = DockStyle.Left };
        rchk.Controls.Add(chk);
        Editors["Checked"] = chk;
        chk.CheckedChanged += (_, _) => Raise("Checked");

        // --- Technika jak dół formularza AMC (Ścieżka / Silnik / Tryb / …) ---
        var rt0 = FieldRow(this); FieldLabel(rt0, "Ścieżka instalacji:", 130); FieldFill(rt0, "InstallPath", new TextBox());
        var rt1 = FieldRow(this); FieldLabel(rt1, "Silnik:", 130);
        var g1 = RightGroup(rt1, 320);
        FieldLabel(g1, "Wersja:", 80);
        var ver = new TextBox { Dock = DockStyle.Fill };
        g1.Controls.Add(ver);
        Editors["Version"] = ver;
        ver.Leave += (_, _) => Raise("Version");
        FieldFill(rt1, "Engine", DropList());
        var rt2 = FieldRow(this); FieldLabel(rt2, "Tryb gry:", 130);
        var g2 = RightGroup(rt2, 320);
        FieldLabel(g2, "Platforma:", 80);
        var plat = DropList(Platforms.All);
        plat.Dock = DockStyle.Fill;
        g2.Controls.Add(plat);
        Editors["Platform"] = plat;
        plat.Leave += (_, _) => Raise("Platform");
        FieldFill(rt2, "GameMode", DropList(GameModes.All));
        var rt3 = FieldRow(this); FieldLabel(rt3, "Rozdzielczość:", 130);
        var g3 = RightGroup(rt3, 320);
        FieldLabel(g3, "Klatkaż:", 80);
        var fps = new TextBox { Dock = DockStyle.Fill };
        g3.Controls.Add(fps);
        Editors["Framerate"] = fps;
        fps.Leave += (_, _) => Raise("Framerate");
        FieldFill(rt3, "Resolution", new TextBox());
        var rt4 = FieldRow(this); FieldLabel(rt4, "Dubbing:", 130); FieldFill(rt4, "AudioLanguages", new TextBox());
        var rt5 = FieldRow(this); FieldLabel(rt5, "Napisy:", 130); FieldFill(rt5, "Subtitles", new TextBox());
        var rt6 = FieldRow(this); FieldLabel(rt6, "Rozmiar (GB):", 130);
        var g6 = RightGroup(rt6, 320);
        FieldLabel(g6, "Liczba płyt:", 80);
        var disks = new NumericUpDown { Maximum = 100, Dock = DockStyle.Fill };
        g6.Controls.Add(disks);
        Editors["DiskCount"] = disks;
        disks.Leave += (_, _) => Raise("DiskCount");
        FieldFill(rt6, "SizeGb", new TextBox());
        var rt7 = FieldRow(this); FieldLabel(rt7, "Ostatnio grano:", 130);
        var g7 = RightGroup(rt7, 320);
        FieldLabel(g7, "Status:", 80);
        var st = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
        st.Items.AddRange(new object[] { "Do zagrania", "Ogrywana", "Ukończona", "Ukończona 100%", "Wstrzymana", "Porzucona" });
        g7.Controls.Add(st);
        Editors["CompletionStatus"] = st;
        st.SelectedIndexChanged += (_, _) => Raise("CompletionStatus");
        FieldFill(rt7, "LastPlayed", new DateTimePicker { ShowCheckBox = true, Format = DateTimePickerFormat.Short });
        ControlLayout.FixDockOrder(this);
    }
}
