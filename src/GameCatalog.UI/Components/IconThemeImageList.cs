using System.ComponentModel;

namespace GameCatalog.UI.Components;

/// <summary>
/// Provides the AMC toolbar icon themes (Resources\*.bmp strips of 16x16 icons) and assigns
/// the active theme to tool strips: set "UseThemeIcons" on a ToolStrip in the Designer.
/// </summary>
[ToolboxItem(true), ProvideProperty("UseThemeIcons", typeof(ToolStrip))]
public sealed class IconThemeImageList : Component, IExtenderProvider
{
    public sealed record IconTheme(string Id, string FileName, string NameKey);

    public const string DefaultThemeId = "WindowsXP";

    public static readonly IReadOnlyList<IconTheme> Themes = new[]
    {
        new IconTheme("Classic", "Classic.bmp", "Theme.Classic"),
        new IconTheme("Crystal", "Crystal.bmp", "Theme.Crystal"),
        new IconTheme("Fresh", "Fresh.bmp", "Theme.Fresh"),
        new IconTheme("Gnome", "Gnome.bmp", "Theme.Gnome"),
        new IconTheme("Scrows", "Scrows.bmp", "Theme.Scrows"),
        new IconTheme("WindowsXP", "Windows XP.bmp", "Theme.WindowsXP"),
    };

    /// <summary>Theme names stored in prefs.json by the Polish-only version.</summary>
    private static readonly Dictionary<string, string> LegacyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Klasyczny"] = "Classic",
        ["Kryształowy"] = "Crystal",
        ["Świeży"] = "Fresh",
        ["Gnom"] = "Gnome",
        ["Windows XP"] = "WindowsXP",
    };

    private readonly HashSet<ToolStrip> _strips = new();
    private string _theme = DefaultThemeId;

    public IconThemeImageList() => ImageList = NewImageList();

    public IconThemeImageList(IContainer container) : this() => container.Add(this);

    [Browsable(false)]
    public ImageList ImageList { get; private set; }

    /// <summary>Folder with the theme bitmaps, relative to the executable.</summary>
    [DefaultValue("Resources")]
    public string ResourceDirectory { get; set; } = "Resources";

    [DefaultValue(DefaultThemeId)]
    public string Theme
    {
        get => _theme;
        set
        {
            _theme = NormalizeThemeId(value);
            if (!(Site?.DesignMode ?? false)) Reload();
        }
    }

    public event EventHandler? ThemeChanged;

    public static string NormalizeThemeId(string? stored)
    {
        if (string.IsNullOrWhiteSpace(stored)) return DefaultThemeId;
        var byId = Themes.FirstOrDefault(t => t.Id.Equals(stored, StringComparison.OrdinalIgnoreCase));
        if (byId is not null) return byId.Id;
        return LegacyNames.GetValueOrDefault(stored, DefaultThemeId);
    }

    /// <summary>Cuts a theme strip into 16x16 icons; a missing or unreadable file gives an empty list.</summary>
    public static ImageList LoadStrip(string themeId, string resourceDirectory)
    {
        var images = NewImageList();
        var id = NormalizeThemeId(themeId);
        var theme = Themes.First(t => t.Id == id);
        var path = Path.Combine(resourceDirectory, theme.FileName);
        if (!File.Exists(path)) return images;
        try
        {
            using var strip = new Bitmap(path);
            for (int i = 0; i < strip.Width / 16; i++)
            {
                var icon = new Bitmap(16, 16);
                using (var g = Graphics.FromImage(icon))
                    g.DrawImage(strip, new Rectangle(0, 0, 16, 16), new Rectangle(i * 16, 0, 16, 16), GraphicsUnit.Pixel);
                images.Images.Add(icon);
            }
        }
        catch (ArgumentException) { /* not a valid bitmap: keep the list empty */ }
        return images;
    }

    public bool CanExtend(object extendee) => extendee is ToolStrip;

    [DefaultValue(false), Category("Appearance"), Description("Use the AMC icon theme for this strip.")]
    public bool GetUseThemeIcons(ToolStrip strip) => _strips.Contains(strip);

    public void SetUseThemeIcons(ToolStrip strip, bool value)
    {
        if (value)
        {
            _strips.Add(strip);
            Assign(strip);
        }
        else
        {
            _strips.Remove(strip);
        }
    }

    /// <summary>Re-assigns the image list (call after adding drop-down items at runtime).</summary>
    public void Refresh()
    {
        foreach (var strip in _strips) Assign(strip);
    }

    private void Reload()
    {
        var dir = Path.IsPathRooted(ResourceDirectory)
            ? ResourceDirectory
            : Path.Combine(AppContext.BaseDirectory, ResourceDirectory);
        var old = ImageList;
        ImageList = LoadStrip(_theme, dir);
        Refresh();
        old.Dispose();
        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Assign(ToolStrip strip)
    {
        strip.ImageList = ImageList;
        AssignDropDowns(strip.Items);
    }

    private void AssignDropDowns(ToolStripItemCollection items)
    {
        foreach (ToolStripItem item in items)
        {
            if (item is not ToolStripDropDownItem dd) continue;
            dd.DropDown.ImageList = ImageList;
            AssignDropDowns(dd.DropDownItems);
        }
    }

    private static ImageList NewImageList() => new()
    {
        ImageSize = new Size(16, 16),
        ColorDepth = ColorDepth.Depth32Bit,
        TransparentColor = Color.Magenta,
    };

    protected override void Dispose(bool disposing)
    {
        if (disposing) ImageList.Dispose();
        base.Dispose(disposing);
    }
}
