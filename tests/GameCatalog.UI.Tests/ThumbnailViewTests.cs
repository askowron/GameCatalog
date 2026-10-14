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
}
