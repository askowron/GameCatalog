using GameCatalog.UI.Controls;

namespace GameCatalog.UI.Tests;

public class ThumbnailViewTests
{
    [Fact]
    public void Display_WithoutHandle_KeepsCoverImagesAndDoesNotDisposePlaceholder()
    {
        using var placeholder = new Bitmap(32, 32);
        using var cover = new Bitmap(40, 40);
        using var ms = new MemoryStream();
        cover.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

        using var tv = new ThumbnailView { Placeholder = placeholder };
        tv.Display(new (int, string, byte[]?)[] { (1, "A", ms.ToArray()), (2, "B", null) });

        Assert.Equal(2, tv.LargeImageList!.Images.Count);   // placeholder + one cover
        Assert.NotNull(tv.LargeImageList.Images[1]);
        Assert.Equal(1, tv.Items[0].ImageIndex);
        Assert.Equal(0, tv.Items[1].ImageIndex);
        Assert.Equal(32, placeholder.Width);                 // caller's placeholder is still alive
    }

    [Fact]
    public void FitInto_PortraitCover_KeepsAspectRatioAndCentersIt()
    {
        using var cover = new Bitmap(30, 60);
        using (var g = Graphics.FromImage(cover)) g.Clear(Color.Red);

        using var thumb = ThumbnailView.FitInto(cover, new Size(64, 64));

        Assert.Equal(new Size(64, 64), thumb.Size);
        Assert.Equal(Color.Red.ToArgb(), thumb.GetPixel(32, 32).ToArgb());   // cover in the middle
        Assert.Equal(Color.Red.ToArgb(), thumb.GetPixel(32, 1).ToArgb());    // full height used
        Assert.Equal(0, thumb.GetPixel(2, 32).A);                            // transparent side bars
        Assert.Equal(0, thumb.GetPixel(61, 32).A);
    }
}
