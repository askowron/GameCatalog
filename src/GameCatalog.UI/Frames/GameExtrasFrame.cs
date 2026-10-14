using GameCatalog.Core.Models;
using GameCatalog.UI.Components;

namespace GameCatalog.UI.Frames;

/// <summary>Extras/DLC list of the current game (AMC framemovieextras.pas).</summary>
public partial class GameExtrasFrame : UserControl
{
    private Game? _game;
    private bool _showCategory = true;

    public GameExtrasFrame()
    {
        InitializeComponent();
        lvExtras.DoubleClick += (_, _) => ExtraActivated?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? ExtraActivated;

    public ToolStrip Toolbar => tsExtras;
    public ToolStripButton AddButton => btnExtraAdd;
    public ToolStripButton EditButton => btnExtraEdit;
    public ToolStripButton DeleteButton => btnExtraDelete;

    public GameExtra? SelectedExtra => lvExtras.SelectedItems.Count > 0 ? lvExtras.SelectedItems[0].Tag as GameExtra : null;

    [System.ComponentModel.Browsable(false), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool ShowCategory
    {
        get => _showCategory;
        set { _showCategory = value; colExtraCategory.Width = value ? 120 : 0; }
    }

    public void Bind(Game? game)
    {
        _game = game;
        lvExtras.BeginUpdate();
        lvExtras.Items.Clear();
        foreach (var extra in game?.Extras ?? Enumerable.Empty<GameExtra>())
        {
            var item = new ListViewItem(extra.Title) { Tag = extra };
            item.SubItems.Add(extra.Category);
            lvExtras.Items.Add(item);
        }
        lvExtras.EndUpdate();
        RefreshCaptions();
    }

    /// <summary>Title with the extras count; call after a language change.</summary>
    public void RefreshCaptions()
    {
        var t = Localizer.Translator;
        lblExtrasTitle.Text = _game is null
            ? t?.Get("Extras.Title") ?? lblExtrasTitle.Text
            : t?.Get("Extras.TitleCount", _game.Extras.Count) ?? $"{lblExtrasTitle.Text} ({_game.Extras.Count})";
    }
}
