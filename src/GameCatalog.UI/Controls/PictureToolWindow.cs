namespace GameCatalog.UI.Controls;

/// <summary>Cover panel under the list with its mini toolbar (AMC ToolbarPictureWindow).</summary>
public partial class PictureToolWindow : UserControl
{
    public PictureToolWindow() => InitializeComponent();

    public ToolStrip Toolbar => tsPicture;
    public ToolStripButton LoadButton => btnPicLoad;
    public ToolStripButton SaveAsButton => btnPicSaveAs;
    public ToolStripButton CopyButton => btnPicCopy;
    public ToolStripButton PasteButton => btnPicPaste;
    public ToolStripButton DeleteButton => btnPicDelete;

    public Image? CurrentImage => picCover.Image;

    public void ShowCover(byte[]? bytes)
    {
        var old = picCover.Image;
        picCover.Image = null;
        old?.Dispose();
        if (bytes is not { Length: > 0 }) return;
        try
        {
            using var ms = new MemoryStream(bytes);
            using var img = Image.FromStream(ms);
            picCover.Image = new Bitmap(img);
        }
        catch (ArgumentException) { /* unreadable cover: show nothing */ }
    }
}
