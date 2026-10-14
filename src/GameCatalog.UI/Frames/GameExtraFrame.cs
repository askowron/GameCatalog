using GameCatalog.Core.Models;

namespace GameCatalog.UI.Frames;

/// <summary>Editor of one extra/DLC (AMC frameextra.pas).</summary>
public partial class GameExtraFrame : UserControl
{
    private GameExtra? _extra;

    public GameExtraFrame()
    {
        InitializeComponent();
        cbxExtraCategory.Items.AddRange(ExtraCategories.All);
    }

    [System.ComponentModel.Browsable(false), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public string TitleText { get => txtExtraTitle.Text; set => txtExtraTitle.Text = value; }

    public void FocusTitle() => txtExtraTitle.Focus();

    public void Bind(GameExtra extra)
    {
        _extra = extra;
        txtExtraTitle.Text = extra.Title;
        cbxExtraCategory.Text = extra.Category;
        txtExtraTag.Text = extra.Tag;
        txtExtraCreatedBy.Text = extra.CreatedBy;
        txtExtraUrl.Text = extra.Url;
        txtExtraDescription.Text = extra.Description;
        txtExtraComments.Text = extra.Comments;
    }

    public void Commit()
    {
        if (_extra is null) return;
        _extra.Title = txtExtraTitle.Text.Trim();
        _extra.Category = cbxExtraCategory.Text.Trim();
        _extra.Tag = txtExtraTag.Text.Trim();
        _extra.CreatedBy = txtExtraCreatedBy.Text.Trim();
        _extra.Url = txtExtraUrl.Text.Trim();
        _extra.Description = txtExtraDescription.Text;
        _extra.Comments = txtExtraComments.Text;
    }
}
