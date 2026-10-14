namespace GameCatalog.UI.Controls;

/// <summary>Color tag palette from AMC movieclass.pas DefaultColorsTag (values are Delphi TColor = $BBGGRR).</summary>
public static class ColorTags
{
    private static readonly int[] Values =
    {
        0xFFFFFF, 0x241CED, 0x277FFF, 0x00F2FF, 0x1DE6B5, 0x4CB122, 0xEEEE1C,
        0xFFB040, 0xFF80A0, 0xE060FF, 0xE00ED0, 0x5786B9, 0x999999,
    };

    public static int Count => Values.Length;

    /// <summary>Row color for a tag; <see cref="Color.Empty"/> for "no tag" (0) or unknown tags.</summary>
    public static Color ToColor(int tag)
    {
        if (tag <= 0 || tag >= Values.Length) return Color.Empty;
        int c = Values[tag];
        return Color.FromArgb(c & 0xFF, (c >> 8) & 0xFF, (c >> 16) & 0xFF);
    }

    /// <summary>16x16 menu swatch of a tag color.</summary>
    public static Bitmap Swatch(int tag)
    {
        var bitmap = new Bitmap(16, 16);
        using var g = Graphics.FromImage(bitmap);
        g.Clear(ToColor(tag) is { IsEmpty: false } c ? c : Color.White);
        g.DrawRectangle(Pens.Black, 0, 0, 15, 15);
        return bitmap;
    }
}
