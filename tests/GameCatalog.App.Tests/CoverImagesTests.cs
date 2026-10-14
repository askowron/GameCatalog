using System.Drawing.Imaging;
using GameCatalog.App.Services;

namespace GameCatalog.App.Tests;

public class CoverImagesTests
{
    private static byte[] Png(int w, int h)
    {
        using var bmp = new Bitmap(w, h);
        using var ms = new MemoryStream();
        bmp.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }

    [Fact]
    public void Downscale_ReducesWidthKeepingAspect()
    {
        using var img = CoverImages.TryLoad(CoverImages.Downscale(Png(400, 200), 100))!;
        Assert.Equal(100, img.Width);
        Assert.Equal(50, img.Height);
    }

    [Fact]
    public void Downscale_ZeroOrNarrowImage_ReturnsSameBytes()
    {
        var bytes = Png(50, 50);
        Assert.Same(bytes, CoverImages.Downscale(bytes, 0));
        Assert.Same(bytes, CoverImages.Downscale(bytes, 100));
    }

    [Fact]
    public void ToDataUri_DetectsPng_NullForEmpty()
    {
        Assert.StartsWith("data:image/png;base64,", CoverImages.ToDataUri(Png(2, 2)));
        Assert.Null(CoverImages.ToDataUri(null));
        Assert.Null(CoverImages.ToDataUri(Array.Empty<byte>()));
    }

    [Fact]
    public void TryLoad_InvalidBytes_ReturnsNull() => Assert.Null(CoverImages.TryLoad(new byte[] { 1, 2, 3 }));
}
