using GameCatalog.App.Services;

namespace GameCatalog.App.Forms;

/// <summary>Help index (AMC help).</summary>
public partial class HelpForm : Form
{
    // Order of the topic list; each topic has "Help.<Topic>.Title" and "Help.<Topic>.Body" strings.
    private static readonly string[] Topics =
        { "Start", "Fields", "List", "Find", "ImportExport", "Stats", "Loans", "Scripts", "Shortcuts" };

    public HelpForm()
    {
        InitializeComponent();
        var l = AppServices.Languages;
        foreach (var topic in Topics) lstTopics.Items.Add(l.Get($"Help.{topic}.Title"));
        lstTopics.SelectedIndexChanged += (_, _) =>
        {
            if (lstTopics.SelectedIndex >= 0)
                rtbText.Text = l.Get($"Help.{Topics[lstTopics.SelectedIndex]}.Body");
        };
        lstTopics.SelectedIndex = 0;
    }
}
