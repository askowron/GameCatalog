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
        lblDescription.Text = $"GameCatalog {version}\n" +
                              AppServices.Languages.Get("About.Credits", Application.CompanyName, ContactEmail, RepositoryUrl) +
                              "\n\n" + AppServices.Languages.Get("About.Description");
    }

    public const string ContactEmail = "info@appit.pl";
    public const string RepositoryUrl = "https://github.com/askowron/GameCatalog";
}
