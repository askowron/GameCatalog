using GameCatalog.App.Services;

namespace GameCatalog.App.Forms;

/// <summary>About box (AMC about.pas).</summary>
public partial class AboutForm : Form
{
    public AboutForm()
    {
        InitializeComponent();
        // ProductVersion may carry "+<commit>" build metadata; show the version only.
        string version = Application.ProductVersion.Split('+')[0];
        lblDescription.Text = $"GameCatalog {version}\n\n" + AppServices.Languages.Get("About.Description");
    }
}
