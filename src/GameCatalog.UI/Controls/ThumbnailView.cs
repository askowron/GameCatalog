using System.ComponentModel;

namespace GameCatalog.UI.Controls;

/// <summary>Cover thumbnails of the visible games (AMC ThumbsViewer / rkSmartView).</summary>
public sealed class ThumbnailView : ListView
{
    private bool _filling;
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
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Image? Placeholder { get; set; }

    public event EventHandler? SelectedGameChanged;

    public int? SelectedNumber => SelectedItems.Count > 0 && SelectedItems[0].Tag is int n ? n : null;

    public void Display(IReadOnlyList<(int Number, string Caption, byte[]? Cover)> items)
    {
        int? before = SelectedNumber;
        _filling = true;
        BeginUpdate();
        try
        {
            Items.Clear();
            _images.Images.Clear();
            if (Placeholder is not null) _images.Images.Add(Placeholder);
            else
            {
                using var blank = new Bitmap(64, 64);
                _images.Images.Add(blank);   // ImageList copies the image
            }
            foreach (var (number, caption, cover) in items)
            {
                int index = 0;
                if (TryDecode(cover) is { } img)
                {
                    using (img) _images.Images.Add(img);   // ImageList copies the image
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
            return new Bitmap(img);
        }
        catch (ArgumentException) { return null; }
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
