using GameCatalog.Core.Catalog;
using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;

namespace GameCatalog.App;

/// <summary>
/// Statystyki — wierny port AMC stats.pas/stats.dfm:
/// lista kategorii + zakres (Wszystkie/Wybrane/Zaznaczone/Widoczne),
/// wykres słupkowy (pola liczbowe/daty) lub kołowy (pola tekstowe),
/// pasek narzędzi (Zapisz jako, Kopiuj, Opcje: Legenda/Etykiety/Puste miesiące/Grupuj),
/// widok informacji ogólnych.
/// </summary>
public sealed class StatsForm : Form
{
    private enum Scope { All, Selected, Checked, Visible }

    private sealed class Category
    {
        public string Tag = "";
        public string Name = "";
        public bool IsGeneral;
        public bool IsExtra;
        public bool IsDate;
        public bool IsNumeric;
        public int Increment = 5;
        public bool IntLabels = true;
        public override string ToString() => Name;
    }

    private readonly GameList _model;
    private readonly HashSet<int> _visible;
    private readonly int? _selected;
    private readonly string _catalogPath;
    private bool _showingGeneral = true;

    private readonly ListBox _lstCategories = new() { Dock = DockStyle.Fill };
    private readonly RadioButton _rbAll = new() { Text = "Wszystkie", AutoSize = true, Checked = true };
    private readonly RadioButton _rbSelected = new() { Text = "Wybrane", AutoSize = true };
    private readonly RadioButton _rbChecked = new() { Text = "Zaznaczone", AutoSize = true };
    private readonly RadioButton _rbVisible = new() { Text = "Widoczne", AutoSize = true };
    private readonly ToolStrip _chartTools = new();
    private readonly StatsCanvas _canvas = new() { Dock = DockStyle.Fill };
    private readonly VScrollBar _scroll = new() { Dock = DockStyle.Right, Visible = false };
    private readonly ListView _lstGeneral = new() { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true };
    private readonly ToolStripMenuItem _optLegend = new("Legenda") { CheckOnClick = true, Checked = true };
    private readonly ToolStripMenuItem _optLabels = new("Etykiety") { CheckOnClick = true, Checked = true };
    private readonly ToolStripMenuItem _optEmpty = new("Puste miesiące") { CheckOnClick = true, Checked = true };
    private readonly ToolStripMenuItem _optGroup = new("Grupuj wartości") { CheckOnClick = true, Checked = true };
    private ImageList _icons = null!;

    public StatsForm(GameList model, HashSet<int> visibleNumbers, int? selectedNumber, string catalogPath)
    {
        _model = model;
        _visible = visibleNumbers;
        _selected = selectedNumber;
        _catalogPath = catalogPath;

        Text = "Statystyki";
        Width = 680; Height = 560;
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;

        _icons = AmcIcons.LoadStrip();
        _chartTools.ImageList = _icons;

        // --- lewy panel: kategorie + zakres (jak Panel2 + IncMovies) ---
        var left = new Panel { Dock = DockStyle.Left, Width = 160 };
        _lstCategories.SelectedIndexChanged += (_, _) => RefreshStats();
        left.Controls.Add(_lstCategories);
        var grp = new GroupBox { Text = "Uwzględnij gry", Dock = DockStyle.Bottom, Height = 120 };
        var gp = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown };
        foreach (var rb in new[] { _rbAll, _rbSelected, _rbChecked, _rbVisible })
        {
            rb.Click += (_, _) => RefreshStats();
            gp.Controls.Add(rb);
        }
        grp.Controls.Add(gp);
        left.Controls.Add(grp);

        // --- prawy panel: toolbar + wykres/lista (jak Panel1) ---
        var right = new Panel { Dock = DockStyle.Fill };
        static ToolStripButton TBtn(AmcIcon icon, EventHandler onClick, string tip)
        {
            var b = new ToolStripButton
            {
                ImageIndex = (int)icon,
                DisplayStyle = ToolStripItemDisplayStyle.Image,
                ToolTipText = tip,
            };
            b.Click += onClick;
            return b;
        }
        _chartTools.Items.Add(TBtn(AmcIcon.StatsSave, (_, _) => SaveChart(), "Zapisz wykres…"));
        _chartTools.Items.Add(TBtn(AmcIcon.StatsCopy, (_, _) => CopyChart(), "Kopiuj wykres do schowka"));
        _chartTools.Items.Add(new ToolStripSeparator());
        var ddOpt = new ToolStripDropDownButton("Opcje") { ImageIndex = (int)AmcIcon.StatsOptions, DisplayStyle = ToolStripItemDisplayStyle.ImageAndText, ToolTipText = "Opcje wykresu" };
        ddOpt.DropDown.ImageList = _icons;
        _optLegend.ImageIndex = (int)AmcIcon.StatsOptions;
        _optLabels.ImageIndex = (int)AmcIcon.StatsOptions;
        _optEmpty.ImageIndex = (int)AmcIcon.StatsOptions;
        _optGroup.ImageIndex = (int)AmcIcon.StatsOptions;
        _optLegend.CheckedChanged += (_, _) => { _canvas.ShowLegend = _optLegend.Checked; _canvas.Invalidate(); };
        _optLabels.CheckedChanged += (_, _) => { _canvas.ShowLabels = _optLabels.Checked; _canvas.Invalidate(); };
        _optEmpty.CheckedChanged += (_, _) => RefreshStats();
        _optGroup.CheckedChanged += (_, _) => RefreshStats();
        ddOpt.DropDownItems.AddRange(new ToolStripItem[] { _optLegend, _optLabels, _optEmpty, _optGroup });
        _chartTools.Items.Add(ddOpt);
        _chartTools.Dock = DockStyle.Top;
        right.Controls.Add(_chartTools);

        var body = new Panel { Dock = DockStyle.Fill };
        _scroll.Scroll += (_, _) => { _canvas.ScrollOffset = _scroll.Value; _canvas.Invalidate(); };
        _lstGeneral.Columns.Add("", 220);
        _lstGeneral.Columns.Add("", 200);
        _lstGeneral.HeaderStyle = ColumnHeaderStyle.None;
        body.Controls.Add(_canvas);
        body.Controls.Add(_scroll);
        body.Controls.Add(_lstGeneral);
        right.Controls.Add(body);

        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 42 };
        var btnClose = new Button { Text = "Zamknij", DialogResult = DialogResult.Cancel, Width = 90, Anchor = AnchorStyles.Right | AnchorStyles.Top, Left = 0, Top = 8 };
        btnClose.Left = bottom.Width - 100;
        btnClose.Anchor = AnchorStyles.Right | AnchorStyles.Top;
        bottom.Controls.Add(btnClose);
        bottom.Resize += (_, _) => btnClose.Left = bottom.Width - 100;

        Controls.Add(right);
        Controls.Add(left);
        Controls.Add(bottom);
        CancelButton = btnClose;

        FillCategories();
        _lstCategories.SelectedIndex = 0;
    }

    private void FillCategories()
    {
        _lstCategories.Items.Add(new Category { IsGeneral = true, Name = "Informacje ogólne" });
        var skip = new HashSet<string> { "Number", "Comments", "Description" };
        foreach (var f in GameFields.All)
        {
            if (skip.Contains(f.Tag)) continue;
            _lstCategories.Items.Add(new Category
            {
                Tag = f.Tag,
                Name = CatalogLabels.Current.Field(f.Tag),
                IsDate = f.Tag is "DateAdded" or "DateFinished" or "LastPlayed",
                IsNumeric = f.Tag is "ReleaseYear" or "PlaytimeHours" or "UserRating" or "CriticRating" or "DiskCount" or "ColorTag",
                Increment = f.Tag switch
                {
                    "ReleaseYear" => 5,
                    "PlaytimeHours" => 10,
                    "UserRating" or "CriticRating" or "DiskCount" or "ColorTag" => 1,
                    _ => 5,
                },
            });
        }
        _lstCategories.Items.Add(new Category { Tag = "E Category", Name = "Kategoria (DLC)", IsExtra = true });
        _lstCategories.Items.Add(new Category { Tag = "E CreatedBy", Name = "Twórca (DLC)", IsExtra = true });
    }

    private IEnumerable<Game> ScopeGames()
    {
        var scope = _rbSelected.Checked ? Scope.Selected
            : _rbChecked.Checked ? Scope.Checked
            : _rbVisible.Checked ? Scope.Visible : Scope.All;
        return scope switch
        {
            Scope.Selected => _model.Games.Where(g => _selected.HasValue && g.Number == _selected.Value),
            Scope.Checked => _model.Games.Where(g => g.Checked),
            Scope.Visible => _model.Games.Where(g => _visible.Contains(g.Number)),
            _ => _model.Games,
        };
    }

    private void RefreshStats()
    {
        if (_lstCategories.SelectedItem is not Category cat) return;
        var games = ScopeGames().ToList();

        _optEmpty.Enabled = cat.IsDate;
        _optGroup.Enabled = !cat.IsDate && !cat.IsGeneral;

        if (cat.IsGeneral)
        {
            _showingGeneral = true;
            _scroll.Visible = false;
            _lstGeneral.BringToFront();
            FillGeneral(games);
            return;
        }
        _showingGeneral = false;
        _canvas.BringToFront();
        _scroll.BringToFront();

        var (labels, values) = ComputeField(games, cat);
        _canvas.Data = new StatsCanvas.StatData(
            cat.IsNumeric || cat.IsDate ? StatsCanvas.ChartKind.Bar : StatsCanvas.ChartKind.Pie,
            cat.Name + "\n" + Path.GetFileName(_catalogPath),
            labels, values);
        _canvas.ScrollOffset = 0;
        _scroll.Value = 0;
        if (_canvas.Data.Kind == StatsCanvas.ChartKind.Bar && labels.Count > StatsCanvas.PageBars)
        {
            _scroll.Visible = true;
            _scroll.Minimum = 0;
            _scroll.Maximum = labels.Count - 1;
            _scroll.LargeChange = StatsCanvas.PageBars;
        }
        else _scroll.Visible = false;
        _canvas.Invalidate();
    }

    private void FillGeneral(List<Game> games)
    {
        _lstGeneral.Items.Clear();
        int count = games.Count;
        var hours = games.Where(g => g.PlaytimeHours > 0).Select(g => g.PlaytimeHours).ToList();
        double avg = hours.Count > 0 ? hours.Average() : 0;
        double totalMin = games.Sum(g => g.PlaytimeHours) * 60;
        int days = (int)(totalMin / (60 * 24));
        int hrs = (int)((totalMin % (60 * 24)) / 60);
        int min = (int)(totalMin % 60);
        double totalGb = games.Sum(g => ParseSizeGb(g.SizeGb));
        int dlcs = games.Sum(g => g.Extras.Count);
        _lstGeneral.Items.Add(new ListViewItem(new[] { "Całkowita liczba gier", count.ToString() }));
        _lstGeneral.Items.Add(new ListViewItem(new[] { "Średni czas gry", $"{avg:0.0} h" }));
        _lstGeneral.Items.Add(new ListViewItem(new[] { "Łączny czas gry", $"{days} dni {hrs} godz. {min} min" }));
        _lstGeneral.Items.Add(new ListViewItem(new[] { "Łączny rozmiar", $"{totalGb:0.#} GB" }));
        _lstGeneral.Items.Add(new ListViewItem(new[] { "Liczba dodatków (DLC)", dlcs.ToString() }));
    }

    private static double ParseSizeGb(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return 0;
        var num = new string(s.Trim().TakeWhile(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());
        if (double.TryParse(num.Replace(',', '.'), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var v)) return v;
        return 0;
    }

    private (List<string> Labels, List<double> Values) ComputeField(List<Game> games, Category cat)
    {
        if (cat.IsExtra) return ComputeExtras(games, cat);
        if (cat.IsDate) return ComputeDates(games, cat);
        if (cat.IsNumeric) return ComputeNumeric(games, cat);
        // tekstowe → kołowy (jak pie series z grupowaniem)
        var groups = games
            .Select(g => FieldValues.Get(g, cat.Tag))
            .Select(v => string.IsNullOrWhiteSpace(v) ? "< brak >" : v)
            .GroupBy(v => v)
            .Select(gr => (Label: gr.Key, Count: (double)gr.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();
        if (_optGroup.Checked)
        {
            double total = groups.Sum(x => x.Count);
            var big = groups.Where(x => x.Count / total >= 0.005).ToList();
            double rest = total - big.Sum(x => x.Count);
            groups = big;
            if (rest > 0) groups.Add(("< inne >", rest));
        }
        return (groups.Select(x => x.Label).ToList(), groups.Select(x => x.Count).ToList());
    }

    private (List<string>, List<double>) ComputeExtras(List<Game> games, Category cat)
    {
        var vals = games.SelectMany(g => g.Extras)
            .Select(e => cat.Tag == "E Category" ? e.Category : e.CreatedBy)
            .Select(v => string.IsNullOrWhiteSpace(v) ? "< brak >" : v)
            .GroupBy(v => v)
            .Select(gr => (gr.Key, (double)gr.Count()))
            .OrderByDescending(x => x.Item2)
            .ToList();
        return (vals.Select(x => x.Key).ToList(), vals.Select(x => x.Item2).ToList());
    }

    private (List<string>, List<double>) ComputeDates(List<Game> games, Category cat)
    {
        var dates = games
            .Select(g => cat.Tag switch
            {
                "DateAdded" => g.DateAdded,
                "DateFinished" => g.DateFinished,
                _ => g.LastPlayed,
            })
            .Where(d => d.HasValue)
            .Select(d => d!.Value)
            .OrderBy(d => d)
            .ToList();
        var pl = new System.Globalization.CultureInfo("pl-PL");
        var counts = new SortedDictionary<string, double>();
        if (_optEmpty.Checked && dates.Count > 0)
        {
            var m = new DateTime(dates[0].Year, dates[0].Month, 1);
            var last = new DateTime(dates[^1].Year, dates[^1].Month, 1);
            while (m <= last)
            {
                counts[m.ToString("MMM yyyy", pl)] = 0;
                m = m.AddMonths(1);
            }
        }
        foreach (var d in dates)
        {
            var k = new DateTime(d.Year, d.Month, 1).ToString("MMM yyyy", pl);
            counts[k] = counts.TryGetValue(k, out var c) ? c + 1 : 1;
        }
        return (counts.Keys.ToList(), counts.Values.ToList());
    }

    private (List<string>, List<double>) ComputeNumeric(List<Game> games, Category cat)
    {
        var vals = games
            .Select(g => GetFloat(g, cat.Tag))
            .Where(v => v >= 0)
            .OrderBy(v => v)
            .ToList();
        if (vals.Count == 0) return (new List<string>(), new List<double>());
        if (!_optGroup.Checked)
        {
            var groups = vals
                .GroupBy(v => v)
                .Select(gr => (Label: FormatNum(gr.Key, cat), Value: (double)gr.Count()))
                .ToList();
            return (groups.Select(x => x.Label).ToList(), groups.Select(x => x.Value).ToList());
        }
        int inc = cat.Increment;
        double first = Math.Floor(vals[0] / inc) * inc;
        double last = vals[^1];
        var buckets = new SortedDictionary<double, double>();
        for (double b = first; b <= last; b += inc) buckets[b] = 0;
        foreach (var v in vals) buckets[Math.Floor(v / inc) * inc]++;
        return (buckets.Keys.Select(k => FormatRange(k, inc, cat)).ToList(), buckets.Values.ToList());
    }

    private static double GetFloat(Game g, string tag) => tag switch
    {
        "ReleaseYear" => g.ReleaseYear > 0 ? g.ReleaseYear : -1,
        "PlaytimeHours" => g.PlaytimeHours > 0 ? g.PlaytimeHours : -1,
        "UserRating" => g.UserRating >= 0 ? g.UserRating / 10.0 : -1,
        "CriticRating" => g.CriticRating >= 0 ? g.CriticRating / 10.0 : -1,
        "DiskCount" => g.DiskCount,
        "ColorTag" => g.ColorTag,
        _ => -1,
    };

    private static string FormatNum(double v, Category cat) =>
        cat.IntLabels ? ((int)Math.Round(v)).ToString() : v.ToString("0.0");

    private static string FormatRange(double k, int inc, Category cat) =>
        cat.IntLabels ? $"{(int)k} -> {(int)k + inc - 1}" : $"{k:0.0} -> {k + inc - 0.1:0.0}";

    private void SaveChart()
    {
        if (_canvas.Data is null || _showingGeneral) return;
        using var s = new SaveFileDialog
        {
            Filter = "PNG (*.png)|*.png|Bitmap (*.bmp)|*.bmp|JPEG (*.jpg)|*.jpg|Metaplik (*.wmf)|*.wmf",
            FileName = "wykres.png",
        };
        if (s.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            if (s.FileName.EndsWith(".wmf", StringComparison.OrdinalIgnoreCase))
            {
                using var refG = CreateGraphics();
                IntPtr hdc = refG.GetHdc();
                try
                {
                    using var mf = new System.Drawing.Imaging.Metafile(s.FileName, hdc);
                    using var mg = Graphics.FromImage(mf);
                    _canvas.RenderAll(mg, new Size(1024, 768));
                }
                finally { refG.ReleaseHdc(hdc); }
            }
            else
            {
                var fmt = s.FileName.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase)
                    ? System.Drawing.Imaging.ImageFormat.Bmp
                    : s.FileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                        ? System.Drawing.Imaging.ImageFormat.Jpeg
                        : System.Drawing.Imaging.ImageFormat.Png;
                using var bmp = new Bitmap(1024, 768);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.White);
                    _canvas.RenderAll(g, new Size(1024, 768));
                }
                bmp.Save(s.FileName, fmt);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Błąd zapisu: " + ex.Message, "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CopyChart()
    {
        if (_canvas.Data is null || _showingGeneral) return;
        using var bmp = new Bitmap(_canvas.Width, _canvas.Height);
        using (var g = Graphics.FromImage(bmp))
            _canvas.RenderAll(g, new Size(_canvas.Width, _canvas.Height));
        Clipboard.SetImage(bmp);
    }
}
