namespace GameCatalog.App.Forms;

/// <summary>Pick one entry from a list on behalf of a script (AMC PickList).</summary>
public partial class PickForm : Form
{
    public int SelectedIndex { get; private set; } = -1;

    public PickForm(string title, IList<string> options)
    {
        InitializeComponent();
        Text = title;
        foreach (var o in options) lstOptions.Items.Add(o);
        if (lstOptions.Items.Count > 0) lstOptions.SelectedIndex = 0;
        lstOptions.DoubleClick += (_, _) =>
        {
            SelectedIndex = lstOptions.SelectedIndex;
            DialogResult = DialogResult.OK;
            Close();
        };
        btnOk.Click += (_, _) => SelectedIndex = lstOptions.SelectedIndex;
    }
}
