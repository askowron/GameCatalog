namespace GameCatalog.App;

/// <summary>
/// Rysowanie wykresów statystyk (GDI+) — odpowiednik TChart z AMC stats.dfm:
/// seria słupkowa (pola liczbowe/daty, etykiety co 90°, ColorEachPoint)
/// lub seria kołowa (pola tekstowe, legenda, etykiety label+wartość).
/// </summary>
public sealed class StatsCanvas : Control
{
    public const int PageBars = 12;

    public enum ChartKind { Bar, Pie }

    public sealed record StatData(ChartKind Kind, string Title, List<string> Labels, List<double> Values);

    private static readonly Color[] Palette =
    {
        Color.FromArgb(0xC0, 0x00, 0x00), Color.FromArgb(0x00, 0x80, 0x00),
        Color.FromArgb(0x00, 0x00, 0xC0), Color.FromArgb(0xC0, 0xC0, 0x00),
        Color.FromArgb(0xC0, 0x00, 0xC0), Color.FromArgb(0x00, 0x80, 0x80),
        Color.FromArgb(0xFF, 0x80, 0x00), Color.FromArgb(0x80, 0x00, 0x80),
        Color.FromArgb(0x00, 0x80, 0xFF), Color.FromArgb(0x80, 0x80, 0x00),
        Color.FromArgb(0x00, 0xC0, 0x00), Color.FromArgb(0xFF, 0x00, 0x80),
    };

    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public StatData? Data { get; set; }
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool ShowLegend { get; set; } = true;
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool ShowLabels { get; set; } = true;
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public int ScrollOffset { get; set; }

    public StatsCanvas()
    {
        DoubleBuffered = true;
        BackColor = Color.White;
        Font = new Font("Arial", 9);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Render(e.Graphics, ClientRectangle, ScrollOffset);
    }

    /// <summary>Render pełnego wykresu (do zapisu/kopiowania, bez stronicowania).</summary>
    public void RenderAll(Graphics g, Size size) => Render(g, new Rectangle(Point.Empty, size), 0, showAll: true);

    private void Render(Graphics g, Rectangle bounds, int offset, bool showAll = false)
    {
        g.Clear(Color.White);
        if (Data is null || Data.Labels.Count == 0)
        {
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString("Brak danych", Font, Brushes.Gray, bounds, sf);
            return;
        }
        if (Data.Kind == ChartKind.Bar) RenderBar(g, bounds, offset, showAll);
        else RenderPie(g, bounds);
    }

    private void RenderBar(Graphics g, Rectangle bounds, int offset, bool showAll)
    {
        var titleFont = new Font(Font.FontFamily, 11, FontStyle.Bold);
        var smallFont = new Font(Font.FontFamily, 8);
        var lines = Data!.Title.Split('\n');
        float y = bounds.Top + 4;
        foreach (var line in lines)
        {
            var sz = g.MeasureString(line, titleFont);
            g.DrawString(line, titleFont, Brushes.Black, bounds.Left + (bounds.Width - sz.Width) / 2, y);
            y += sz.Height + 1;
        }
        y += 6;

        int legendW = ShowLegend ? 150 : 8;
        var labels = showAll ? Data.Labels : Data.Labels.Skip(offset).Take(PageBars).ToList();
        var values = showAll ? Data.Values : Data.Values.Skip(offset).Take(PageBars).ToList();
        float maxLabelW = labels.Count == 0 ? 0 : labels.Max(l => g.MeasureString(l, smallFont).Width);
        int bottomMargin = (int)Math.Clamp(maxLabelW + 12, 30, 140);
        var plot = new Rectangle(bounds.Left + 52, (int)y, bounds.Width - 52 - legendW, bounds.Bottom - (int)y - bottomMargin);
        if (plot.Width < 40 || plot.Height < 40) return;
        double max = Math.Max(values.DefaultIfEmpty(0).Max(), 1);
        max = NiceCeiling(max);

        using var gridPen = new Pen(Color.LightGray);
        for (int i = 0; i <= 5; i++)
        {
            float gy = plot.Bottom - plot.Height * i / 5f;
            g.DrawLine(gridPen, plot.Left, gy, plot.Right, gy);
            string t = (max * i / 5).ToString("0.##");
            var sz = g.MeasureString(t, smallFont);
            g.DrawString(t, smallFont, Brushes.Black, plot.Left - 4 - sz.Width, gy - sz.Height / 2);
        }
        g.DrawRectangle(Pens.Black, plot);

        int n = labels.Count;
        if (n == 0) return;
        float slot = plot.Width / (float)n;
        float barW = Math.Min(slot * 0.6f, 60);
        for (int i = 0; i < n; i++)
        {
            float h = (float)(values[i] / max * plot.Height);
            float x = plot.Left + slot * i + (slot - barW) / 2;
            using var br = new SolidBrush(Palette[(offset + i) % Palette.Length]);
            g.FillRectangle(br, x, plot.Bottom - h, barW, h);
            g.DrawRectangle(Pens.Black, x, plot.Bottom - h, barW, h);
            if (ShowLabels)
            {
                string v = values[i].ToString("0.##");
                var sz = g.MeasureString(v, smallFont);
                g.DrawString(v, smallFont, Brushes.Black, x + (barW - sz.Width) / 2, plot.Bottom - h - sz.Height - 1);
            }
            // etykieta pionowo (jak BottomAxis.LabelsAngle = 90 w AMC)
            var state = g.Save();
            g.TranslateTransform(plot.Left + slot * i + slot / 2, plot.Bottom + 3);
            g.RotateTransform(-90);
            g.DrawString(labels[i], smallFont, Brushes.Black, -g.MeasureString(labels[i], smallFont).Width, 0);
            g.Restore(state);
        }

        if (ShowLegend)
        {
            float ly = plot.Top;
            for (int i = 0; i < n && ly < bounds.Bottom - 14; i++)
            {
                using var br = new SolidBrush(Palette[(offset + i) % Palette.Length]);
                g.FillRectangle(br, bounds.Right - legendW + 4, ly, 12, 12);
                g.DrawRectangle(Pens.Black, bounds.Right - legendW + 4, ly, 12, 12);
                string t = $"{labels[i]} ({values[i]:0.##})";
                if (g.MeasureString(t, smallFont).Width > legendW - 22)
                    t = t[..Math.Max(0, Math.Min(t.Length, (legendW - 30) / 6))] + "…";
                g.DrawString(t, smallFont, Brushes.Black, bounds.Right - legendW + 20, ly - 1);
                ly += 17;
            }
        }
    }

    private static double NiceCeiling(double v)
    {
        if (v <= 5) return Math.Ceiling(v);
        double p = Math.Pow(10, Math.Floor(Math.Log10(v)));
        double n = v / p;
        double nice = n <= 1 ? 1 : n <= 2 ? 2 : n <= 5 ? 5 : 10;
        return nice * p;
    }

    private void RenderPie(Graphics g, Rectangle bounds)
    {
        var titleFont = new Font(Font.FontFamily, 11, FontStyle.Bold);
        var smallFont = new Font(Font.FontFamily, 8);
        var lines = Data!.Title.Split('\n');
        float y = bounds.Top + 4;
        foreach (var line in lines)
        {
            var sz = g.MeasureString(line, titleFont);
            g.DrawString(line, titleFont, Brushes.Black, bounds.Left + (bounds.Width - sz.Width) / 2, y);
            y += sz.Height + 1;
        }
        y += 6;

        double total = Data.Values.Sum();
        if (total <= 0) return;
        int legendW = ShowLegend ? 170 : 8;
        int labelLane = 120;
        int d = Math.Min(bounds.Bottom - (int)y - 34, bounds.Width - legendW - labelLane - 20);
        if (d < 40) return;
        var rect = new Rectangle(bounds.Left + 10, (int)y, d, d);
        int labelRight = bounds.Right - legendW;

        double angle = -90;
        for (int i = 0; i < Data.Labels.Count; i++)
        {
            double sweep = Data.Values[i] / total * 360;
            using var br = new SolidBrush(Palette[i % Palette.Length]);
            g.FillPie(br, rect, (float)angle, (float)sweep);
            g.DrawPie(Pens.Black, rect, (float)angle, (float)sweep);
            double pct = Data.Values[i] / total;
            if (ShowLabels && pct >= 0.03)
            {
                double mid = (angle + sweep / 2) * Math.PI / 180;
                float lx = rect.Left + rect.Width / 2 + (float)Math.Cos(mid) * (d / 2 + 18);
                float ly = rect.Top + rect.Height / 2 + (float)Math.Sin(mid) * (d / 2 + 18);
                string t = $"{Data.Labels[i]} ({Data.Values[i]:0.##})";
                var sz = g.MeasureString(t, smallFont);
                lx = Math.Clamp(lx - sz.Width / 2, bounds.Left + 2, labelRight - sz.Width - 2);
                ly = Math.Clamp(ly, bounds.Top, bounds.Bottom - sz.Height - 2);
                float px = rect.Left + rect.Width / 2 + (float)Math.Cos(mid) * d / 2;
                float py = rect.Top + rect.Height / 2 + (float)Math.Sin(mid) * d / 2;
                g.DrawLine(Pens.Gray, px, py, lx + sz.Width / 2, ly + sz.Height / 2);
                g.DrawString(t, smallFont, Brushes.Black, lx, ly);
            }
            angle += sweep;
        }

        if (ShowLegend)
        {
            float ly = y;
            for (int i = 0; i < Data.Labels.Count && ly < bounds.Bottom - 14; i++)
            {
                using var br = new SolidBrush(Palette[i % Palette.Length]);
                g.FillRectangle(br, bounds.Right - legendW + 4, ly, 12, 12);
                g.DrawRectangle(Pens.Black, bounds.Right - legendW + 4, ly, 12, 12);
                double pct = Data.Values[i] / total * 100;
                g.DrawString($"{Data.Labels[i]} ({pct:0.#}%)", smallFont, Brushes.Black, bounds.Right - legendW + 20, ly - 1);
                ly += 17;
            }
        }
    }
}
