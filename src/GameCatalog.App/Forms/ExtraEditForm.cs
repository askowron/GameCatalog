using GameCatalog.App.Services;
using GameCatalog.Core.Models;

namespace GameCatalog.App.Forms;

/// <summary>Extra/DLC editor (AMC extrasedit.pas).</summary>
public partial class ExtraEditForm : Form
{
    public GameExtra Extra { get; }

    public ExtraEditForm(GameExtra extra)
    {
        InitializeComponent();
        Extra = extra;
        extraFrame.Bind(extra);
        btnOk.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(extraFrame.TitleText))
            {
                var l = AppServices.Languages;
                MessageBox.Show(this, l.Get("ExtraEdit.TitleRequired"), l.Get("ExtraEdit.ValidationTitle"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                extraFrame.FocusTitle();
                return;
            }
            extraFrame.Commit();
        };
    }
}
