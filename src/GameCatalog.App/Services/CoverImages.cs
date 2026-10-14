using System.Drawing.Imaging;

namespace GameCatalog.App.Services;

/// <summary>Cover image helpers (AMC picture import options).</summary>
public static class CoverImages
{
    /// <summary>Scales the image down to <paramref name="maxWidth"/> (PNG); returns the input when no scaling is needed or possible.</summary>
    public static byte[] Downscale(byte[] bytes, int maxWidth)
    {
        if (maxWidth <= 0) return bytes;
        try
        {
            using var ms = new MemoryStream(bytes);
            using var img = Image.FromStream(ms);
            if (img.Width <= maxWidth) return bytes;
            int h = (int)Math.Round(img.Height * (maxWidth / (double)img.Width));
            using var bmp = new Bitmap(img, new Size(maxWidth, h));
            return ToPng(bmp);
        }
        catch (ArgumentException)
        {
            return bytes;
        }
    }

    public static byte[] ToPng(Image image)
    {
        using var ms = new MemoryStream();
        image.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }

    /// <summary>Decodes an image detached from its stream, or null when the bytes are not an image.</summary>
    public static Image? TryLoad(byte[]? bytes)
    {
        if (bytes is not { Length: > 0 }) return null;
        try
        {
            using var ms = new MemoryStream(bytes);
            using var img = Image.FromStream(ms);
            return new Bitmap(img);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    /// <summary>data: URI for embedding the cover in HTML previews (no temporary files).</summary>
    public static string? ToDataUri(byte[]? bytes)
    {
        if (bytes is not { Length: > 0 }) return null;
        string mime = bytes switch
        {
            [0x89, 0x50, 0x4E, 0x47, ..] => "image/png",
            [0xFF, 0xD8, ..] => "image/jpeg",
            [0x47, 0x49, 0x46, ..] => "image/gif",
            [0x42, 0x4D, ..] => "image/bmp",
            _ => "application/octet-stream",
        };
        return $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
    }
}
