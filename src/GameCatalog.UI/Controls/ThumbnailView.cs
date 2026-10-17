using System.ComponentModel;

namespace GameCatalog.UI.Controls;

/// <summary>Cover thumbnails of the visible games (AMC ThumbsViewer / rkSmartView).</summary>
public sealed class ThumbnailView : ListView
{
    private bool _filling;
    private readonly ImageList _images = new() { ImageSize = ThumbSize, ColorDepth = ColorDepth.Depth32Bit };

    public ThumbnailView()
    {
        View = View.LargeIcon;
        MultiSelect = false;
        HideSelection = false;
        LargeImageList = _images;
        DoubleBuffered = true;
    }

    /// <summary>Image shown for games without a cover.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Image? Placeholder { get; set; }

    public event EventHandler? SelectedGameChanged;

    public int? SelectedNumber => SelectedItems.Count > 0 && SelectedItems[0].Tag is int n ? n : null;

    public void Display(IReadOnlyList<(int Number, string Caption, byte[]? Cover)> items)
    {
        // Creating the ImageList handle first makes Images.Add copy the bitmap right away (before that it
        // keeps a reference), so the decoded covers and the blank image can be disposed below.
        _ = _images.Handle;
        int? before = SelectedNumber;
        _filling = true;
        BeginUpdate();
        try
        {
            Items.Clear();
            _images.Images.Clear();
            if (Placeholder is not null)
            {
                using var fitted = FitInto(Placeholder, ThumbSize);
                _images.Images.Add(fitted);   // copied by the ImageList (its handle exists)
            }
            else
            {
                using var blank = new Bitmap(ThumbSize.Width, ThumbSize.Height);
                _images.Images.Add(blank);   // copied by the ImageList (its handle exists)
            }
            foreach (var (number, caption, cover) in items)
            {
                int index = 0;
                if (TryDecode(cover) is { } img)
                {
                    using (img) _images.Images.Add(img);   // copied by the ImageList (its handle exists)
                    index = _images.Images.Count - 1;
                }
                Items.Add(new ListViewItem(caption, index) { Tag = number });
            }
            if (before is int n) SelectNumber(n);
        }
        finally
        {
            EndUpdate();
            _filling = false;
        }
        if (SelectedNumber != before) SelectedGameChanged?.Invoke(this, EventArgs.Empty);
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
            return FitInto(img, ThumbSize);
        }
        catch (ArgumentException) { return null; }
    }

    private static readonly Size ThumbSize = new(64, 64);

    /// <summary>
    /// Scales the image to fit the box, keeping its aspect ratio, centered on a transparent
    /// background (the ImageList would otherwise stretch portrait covers to a square).
    /// </summary>
    public static Bitmap FitInto(Image image, Size box)
    {
        double scale = Math.Min((double)box.Width / image.Width, (double)box.Height / image.Height);
        int w = Math.Max(1, (int)Math.Round(image.Width * scale));
        int h = Math.Max(1, (int)Math.Round(image.Height * scale));
        var result = new Bitmap(box.Width, box.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(result);
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        using var attributes = new System.Drawing.Imaging.ImageAttributes();
        attributes.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);   // no faded edges
        g.DrawImage(image, new Rectangle((box.Width - w) / 2, (box.Height - h) / 2, w, h),
            0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
        return result;
    }

    protected override void OnSelectedIndexChanged(EventArgs e)
    {
        base.OnSelectedIndexChanged(e);
        if (!_filling) SelectedGameChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _images.Dispose();
        base.Dispose(disposing);
    }
}
