using GameCatalog.App.Services;
using GameCatalog.UI.Components;
using GameCatalog.UI.Icons;
namespace GameCatalog.App;

/// <summary>Małe dialogi w stylu AMC: skok do numeru, kolumny listy, właściwości katalogu.</summary>
public sealed class NumberDialog : Form
{
    public int Value { get; private set; }

    public NumberDialog(int current, int max)
    {
        Text = "Numer gry";
        Width = 280; Height = 150;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; MinimizeBox = false;
        var num = new NumericUpDown { Minimum = 1, Maximum = Math.Max(1, max), Value = Math.Clamp(current, 1, Math.Max(1, max)), Dock = DockStyle.Top };
        var bar = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 44, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(8) };
        var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Width = 80 };
        var cancel = new Button { Text = "Anuluj", DialogResult = DialogResult.Cancel, Width = 80 };
        ok.Click += (_, _) => Value = (int)num.Value;
        bar.Controls.Add(ok); bar.Controls.Add(cancel);
        Controls.Add(new Label { Text = "Idź do numeru:", Dock = DockStyle.Top, Padding = new Padding(10, 10, 10, 0) });
        Controls.Add(num);
        Controls.Add(bar);
        AcceptButton = ok; CancelButton = cancel;
    }
}


/// <summary>Dialog wyboru numeru gry w stylu AMC (jak na zrzucie „Wybór numeru filmu”).</summary>
public sealed class NumberConflictDialog : Form
{
    private readonly NumericUpDown _num;
    private readonly RadioButton _rbAccept;
    private readonly RadioButton _rbRenumber;
    private readonly RadioButton _rbShift;
    private readonly CheckBox _cbAuto;

    public int ChosenNumber { get; private set; }
    public NumberConflictResolution Resolution { get; private set; } = NumberConflictResolution.ShiftNew;
    public bool DontAskAgain => _cbAuto.Checked;

    public NumberConflictDialog(int proposedNumber, int maxNumber, Func<int, bool>? isDuplicate = null, Func<int, string?>? describe = null)
    {
        Text = "Wybór numeru gry";
        Width = 385; Height = 262;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; MinimizeBox = false;
        ShowInTaskbar = false;

        // Ikona okna jak w AMC (taśma filmowa) — z bieżącego motywu ikon.
        try
        {
            using var strip = IconThemeImageList.LoadStrip(IconThemeImageList.DefaultThemeId, Path.Combine(AppContext.BaseDirectory, "Resources"));
            var bmp = (Bitmap)strip.Images[(int)AmcIcon.MovieNumber];
            ShowIcon = true;
            Icon = (Icon)Icon.FromHandle(bmp.GetHicon()).Clone();
        }
        catch { ShowIcon = false; }

        ChosenNumber = proposedNumber;

        var lbl = new Label { Text = "Wprowadź numer dla gry:", AutoSize = true, Location = new Point(12, 14) };
        _num = new NumericUpDown
        {
            Minimum = 1,
            Maximum = Math.Max(Math.Max(1, maxNumber), proposedNumber + 1000),
            Value = Math.Clamp(proposedNumber, 1, int.MaxValue),
            Location = new Point(170, 11),
            Width = 130,
        };
        // Przycisk-lupa obok numeru jak w AMC: znajduje pierwszy wolny numer.
        var btnFind = new Button { Location = new Point(306, 10), Size = new Size(26, 24) };
        try
        {
            using var strip = IconThemeImageList.LoadStrip(IconThemeImageList.DefaultThemeId, Path.Combine(AppContext.BaseDirectory, "Resources"));
            btnFind.Image = (Image)strip.Images[(int)AmcIcon.MovieFind].Clone();
        }
        catch { btnFind.Text = "?"; }
        var tips = new ToolTip();
        tips.SetToolTip(btnFind, "Znajdź pierwszy wolny numer");
        btnFind.Click += (_, _) =>
        {
            if (isDuplicate is null) return;
            int n = Math.Max(1, (int)_num.Value);
            int guard = 0;
            while (isDuplicate(n) && guard++ < 100000) n++;
            if (n > _num.Maximum) _num.Maximum = n + 100;
            _num.Value = n;
        };

        var box = new GroupBox { Location = new Point(12, 42), Size = new Size(345, 108), TabStop = false, Text = string.Empty };
        _rbAccept = new RadioButton { Text = "Akceptuj duplikat", Location = new Point(10, 14), AutoSize = true };
        _rbRenumber = new RadioButton { Text = "Nadaj inny numer istniejącej grze", Location = new Point(10, 40), AutoSize = true };
        _rbShift = new RadioButton { Text = "Przesuwaj numer gry do czasu, aż nie będzie zduplikowany", Location = new Point(10, 66), AutoSize = true };
        _rbShift.Checked = true;
        box.Controls.AddRange(new Control[] { _rbAccept, _rbRenumber, _rbShift });

        void RefreshRadios()
        {
            // Opcje rozstrzygania konfliktu mają sens tylko, gdy numer jest zajęty (jak w AMC).
            bool dup = isDuplicate is not null && isDuplicate((int)_num.Value);
            foreach (var rb in new[] { _rbAccept, _rbRenumber, _rbShift }) rb.Enabled = dup;
        }

        RefreshRadios();
        _num.ValueChanged += (_, _) => RefreshRadios();

        _cbAuto = new CheckBox
        {
            Text = "Nie pytaj ponownie; użyj automatycznego numerowania",
            Location = new Point(14, 158),
            AutoSize = true,
        };

        // Przyciski jak w AMC: wyśrodkowane, OK po lewej, Anuluj po prawej.
        var bar = new Panel { Dock = DockStyle.Bottom, Height = 44 };
        var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Width = 80, Height = 24 };
        var cancel = new Button { Text = "Anuluj", DialogResult = DialogResult.Cancel, Width = 80, Height = 24 };
        ok.Location = new Point(100, 10);
        cancel.Location = new Point(190, 10);
        bar.Controls.Add(ok); bar.Controls.Add(cancel);
        ok.Click += (_, _) =>
        {
            ChosenNumber = (int)_num.Value;
            Resolution = _rbAccept.Checked ? NumberConflictResolution.AcceptDuplicate
                : _rbRenumber.Checked ? NumberConflictResolution.RenumberExisting
                : NumberConflictResolution.ShiftNew;
        };

        Controls.AddRange(new Control[] { lbl, _num, btnFind, box, _cbAuto, bar });
        AcceptButton = ok; CancelButton = cancel;
    }
}

/// <summary>Właściwości katalogu — odpowiednik AMC FileProperties.</summary>
public sealed class CatalogPropertiesDialog : Form
{
    public CatalogPropertiesDialog(string dbPath, int games, int extras, double hours, int loans)
    {
        Text = "Właściwości katalogu";
        Width = 420; Height = 260;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; MinimizeBox = false;
        var fi = new FileInfo(dbPath);
        var info = new Label
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            Text = $"Plik bazy:\n{dbPath}\n\n" +
                   $"Rozmiar: {(fi.Exists ? $"{fi.Length / 1024.0:0.#} KB" : "–")}\n" +
                   $"Gier: {games}\nDodatki/DLC: {extras}\n" +
                   $"Czas gry łącznie: {hours:0.#} h\nAktywne wypożyczenia: {loans}",
        };
        var b = new Button { Text = "OK", DialogResult = DialogResult.OK, Dock = DockStyle.Bottom, Height = 36 };
        Controls.Add(info);
        Controls.Add(b);
        AcceptButton = b;
    }
}
