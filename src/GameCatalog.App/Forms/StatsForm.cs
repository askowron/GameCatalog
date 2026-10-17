using System.Globalization;
using GameCatalog.App.Services;
using GameCatalog.UI.Controls;
using GameCatalog.UI.Components;
using GameCatalog.UI.Icons;
using GameCatalog.Core.Catalog;
using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;

namespace GameCatalog.App.Forms;

/// <summary>
/// Statistics — port of AMC stats.pas/stats.dfm: category list + scope (All/Selected/Checked/Visible),
/// bar chart (numeric/date fields) or pie chart (text fields), chart toolbar
/// (Save as, Copy, Options: Legend/Labels/Empty months/Group values) and a general information view.
/// </summary>
public partial class StatsForm : Form
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

    public StatsForm(GameList model, HashSet<int> visibleNumbers, int? selectedNumber, string catalogPath)
    {
        InitializeComponent();
        _model = model;
        _visible = visibleNumbers;
        _selected = selectedNumber;
        _catalogPath = catalogPath;
        LoadIcons();

        lstCategories.SelectedIndexChanged += (_, _) => RefreshStats();
        foreach (var rb in new[] { rbAll, rbSelected, rbChecked, rbVisible })
            rb.Click += (_, _) => RefreshStats();
        btnSaveChart.Click += (_, _) => SaveChart();
        btnCopyChart.Click += (_, _) => CopyChart();
        mnuOptLegend.CheckedChanged += (_, _) => { chartCanvas.ShowLegend = mnuOptLegend.Checked; chartCanvas.Invalidate(); };
        mnuOptLabels.CheckedChanged += (_, _) => { chartCanvas.ShowLabels = mnuOptLabels.Checked; chartCanvas.Invalidate(); };
        mnuOptEmptyMonths.CheckedChanged += (_, _) => RefreshStats();
        mnuOptGroup.CheckedChanged += (_, _) => RefreshStats();
        scrChart.Scroll += (_, _) => { chartCanvas.ScrollOffset = scrChart.Value; chartCanvas.Invalidate(); };

        FillCategories();
        lstCategories.SelectedIndex = 0;
    }

    /// <summary>Toolbar icons from the default AMC icon theme; the strip is disposed with the form.</summary>
    private void LoadIcons()
    {
        var icons = IconThemeImageList.LoadStrip(IconThemeImageList.DefaultThemeId, Path.Combine(AppContext.BaseDirectory, "Resources"));
        components.Add(icons);
        tsChart.ImageList = icons;
        btnOptions.DropDown.ImageList = icons;
        btnSaveChart.ImageIndex = (int)AmcIcon.StatsSave;
        btnCopyChart.ImageIndex = (int)AmcIcon.StatsCopy;
        foreach (var item in new ToolStripItem[] { btnOptions, mnuOptLegend, mnuOptLabels, mnuOptEmptyMonths, mnuOptGroup })
            item.ImageIndex = (int)AmcIcon.StatsOptions;
    }

    private void FillCategories()
    {
        var l = AppServices.Languages;
        lstCategories.Items.Add(new Category { IsGeneral = true, Name = l.Get("Stats.General") });
        var skip = new HashSet<string> { "Number", "Comments", "Description" };
        foreach (var f in GameFields.All)
        {
            if (skip.Contains(f.Tag)) continue;
            lstCategories.Items.Add(new Category
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
        lstCategories.Items.Add(new Category { Tag = "E Category", Name = l.Get("Stats.ExtraCategory"), IsExtra = true });
        lstCategories.Items.Add(new Category { Tag = "E CreatedBy", Name = l.Get("Stats.ExtraCreatedBy"), IsExtra = true });
    }

    private IEnumerable<Game> ScopeGames()
    {
        var scope = rbSelected.Checked ? Scope.Selected
            : rbChecked.Checked ? Scope.Checked
            : rbVisible.Checked ? Scope.Visible : Scope.All;
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
        if (lstCategories.SelectedItem is not Category cat) return;
        var games = ScopeGames().ToList();

        mnuOptEmptyMonths.Enabled = cat.IsDate;
        mnuOptGroup.Enabled = !cat.IsDate && !cat.IsGeneral;

        if (cat.IsGeneral)
        {
            _showingGeneral = true;
            scrChart.Visible = false;
            lvGeneral.BringToFront();
            FillGeneral(games);
            return;
        }
        _showingGeneral = false;
        chartCanvas.BringToFront();
        scrChart.BringToFront();

        var (labels, values) = ComputeField(games, cat);
        chartCanvas.Data = new StatsCanvas.StatData(
            cat.IsNumeric || cat.IsDate ? StatsCanvas.ChartKind.Bar : StatsCanvas.ChartKind.Pie,
            cat.Name + "\n" + Path.GetFileName(_catalogPath),
            labels, values);
        chartCanvas.ScrollOffset = 0;
        scrChart.Value = 0;
        if (chartCanvas.Data.Kind == StatsCanvas.ChartKind.Bar && labels.Count > StatsCanvas.PageBars)
        {
            scrChart.Visible = true;
            scrChart.Minimum = 0;
            scrChart.Maximum = labels.Count - 1;
            scrChart.LargeChange = StatsCanvas.PageBars;
        }
        else scrChart.Visible = false;
        chartCanvas.Invalidate();
    }

    private void FillGeneral(List<Game> games)
    {
        var l = AppServices.Languages;
        lvGeneral.Items.Clear();
        int count = games.Count;
        var hours = games.Where(g => g.PlaytimeHours > 0).Select(g => g.PlaytimeHours).ToList();
        double avg = hours.Count > 0 ? hours.Average() : 0;
        double totalMin = games.Sum(g => g.PlaytimeHours) * 60;
        int days = (int)(totalMin / (60 * 24));
        int hrs = (int)((totalMin % (60 * 24)) / 60);
        int min = (int)(totalMin % 60);
        double totalGb = games.Sum(g => ParseSizeGb(g.SizeGb));
        int dlcs = games.Sum(g => g.Extras.Count);
        void Row(string key, string value) => lvGeneral.Items.Add(new ListViewItem(new[] { l.Get(key), value }));
        Row("Stats.TotalGames", count.ToString());
        Row("Stats.AveragePlaytime", $"{avg:0.0} h");
        Row("Stats.TotalPlaytime", l.Get("Stats.PlaytimeValue", days, hrs, min));
        Row("Stats.TotalSize", $"{totalGb:0.#} GB");
        Row("Stats.ExtrasCount", dlcs.ToString());
    }

    private static double ParseSizeGb(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return 0;
        var num = new string(s.Trim().TakeWhile(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());
        if (double.TryParse(num.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var v)) return v;
        return 0;
    }

    private (List<string> Labels, List<double> Values) ComputeField(List<Game> games, Category cat)
    {
        if (cat.IsExtra) return ComputeExtras(games, cat);
        if (cat.IsDate) return ComputeDates(games, cat);
        if (cat.IsNumeric) return ComputeNumeric(games, cat);
        // Text fields → pie chart, grouped like the AMC pie series.
        string none = AppServices.Languages.Get("Stats.NoValue");
        var groups = games
            .Select(g => GameFieldValues.Get(g, cat.Tag))
            .Select(v => string.IsNullOrWhiteSpace(v) ? none : v)
            .GroupBy(v => v)
            .Select(gr => (Label: gr.Key, Count: (double)gr.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();
        if (mnuOptGroup.Checked)
        {
            double total = groups.Sum(x => x.Count);
            var big = groups.Where(x => x.Count / total >= 0.005).ToList();
            double rest = total - big.Sum(x => x.Count);
            groups = big;
            if (rest > 0) groups.Add((AppServices.Languages.Get("Stats.Other"), rest));
        }
        return (groups.Select(x => x.Label).ToList(), groups.Select(x => x.Count).ToList());
    }

    private (List<string>, List<double>) ComputeExtras(List<Game> games, Category cat)
    {
        string none = AppServices.Languages.Get("Stats.NoValue");
        var vals = games.SelectMany(g => g.Extras)
            .Select(e => cat.Tag == "E Category" ? e.Category : e.CreatedBy)
            .Select(v => string.IsNullOrWhiteSpace(v) ? none : v)
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
        // Keyed by month start so the bars stay in chronological order.
        var counts = new SortedDictionary<DateTime, double>();
        if (mnuOptEmptyMonths.Checked && dates.Count > 0)
        {
            var m = new DateTime(dates[0].Year, dates[0].Month, 1);
            var last = new DateTime(dates[^1].Year, dates[^1].Month, 1);
            for (; m <= last; m = m.AddMonths(1)) counts[m] = 0;
        }
        foreach (var d in dates)
        {
            var k = new DateTime(d.Year, d.Month, 1);
            counts[k] = counts.TryGetValue(k, out var c) ? c + 1 : 1;
        }
        var culture = LanguageCulture();
        return (counts.Keys.Select(k => k.ToString("MMM yyyy", culture)).ToList(), counts.Values.ToList());
    }

    /// <summary>Culture of the UI language, for month names on the date charts.</summary>
    private static CultureInfo LanguageCulture()
    {
        try { return CultureInfo.GetCultureInfo(AppServices.Languages.CurrentLanguage); }
        catch (CultureNotFoundException) { return CultureInfo.CurrentCulture; }
    }

    private (List<string>, List<double>) ComputeNumeric(List<Game> games, Category cat)
    {
        var vals = games
            .Select(g => GetFloat(g, cat.Tag))
            .Where(v => v >= 0)
            .OrderBy(v => v)
            .ToList();
        if (vals.Count == 0) return (new List<string>(), new List<double>());
        if (!mnuOptGroup.Checked)
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
        if (chartCanvas.Data is null || _showingGeneral) return;
        var l = AppServices.Languages;
        using var s = new SaveFileDialog
        {
            Filter = l.Get("Stats.ChartFilter"),
            FileName = l.Get("Stats.ChartFileName"),
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
                    chartCanvas.RenderAll(mg, new Size(1024, 768));
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
                    chartCanvas.RenderAll(g, new Size(1024, 768));
                }
                bmp.Save(s.FileName, fmt);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, l.Get("Common.SaveError", ex.Message), l.Get("Common.Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CopyChart()
    {
        if (chartCanvas.Data is null || _showingGeneral) return;
        using var bmp = new Bitmap(chartCanvas.Width, chartCanvas.Height);
        using (var g = Graphics.FromImage(bmp))
            chartCanvas.RenderAll(g, new Size(chartCanvas.Width, chartCanvas.Height));
        Clipboard.SetImage(bmp);
    }
}
