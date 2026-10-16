using GameCatalog.App.Services;

namespace GameCatalog.App.Forms;

/// <summary>Version history.</summary>
public partial class VersionsForm : Form
{
    public VersionsForm()
    {
        InitializeComponent();
        rtbText.Text = AppServices.Languages.Get("Versions.Text");
    }
}
