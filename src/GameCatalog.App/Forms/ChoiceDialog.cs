namespace GameCatalog.App.Forms;

/// <summary>Pick one entry from a list (import/export format, script to run).</summary>
public partial class ChoiceDialog : Form
{
    public ChoiceDialog(string title, string prompt, IReadOnlyList<string> items)
    {
        InitializeComponent();
        Text = title;
        lblPrompt.Text = prompt;
        lstChoices.Items.AddRange(items.Cast<object>().ToArray());
        if (lstChoices.Items.Count > 0) lstChoices.SelectedIndex = 0;
        lstChoices.DoubleClick += (_, _) =>
        {
            DialogResult = DialogResult.OK;
            Close();
        };
    }

    public int SelectedIndex => lstChoices.SelectedIndex;
}
