using GameCatalog.App.Services;

namespace GameCatalog.App.Forms;

/// <summary>Game card HTML template editor (simplified AMC HTMLEditor.pas).</summary>
public partial class HtmlEditorForm : Form
{
    private readonly string _name;

    public HtmlEditorForm(string name)
    {
        InitializeComponent();
        _name = name;
        Text = AppServices.Languages.Get("HtmlEditor.Caption", HtmlTemplates.DisplayName(name));
        txtEditor.Text = HtmlTemplates.Load(name);
        lstTags.DoubleClick += (_, _) => InsertTag();
        btnReset.Click += (_, _) => txtEditor.Text = HtmlTemplates.Default(_name);
        btnSave.Click += (_, _) => Save();
    }

    private void InsertTag()
    {
        if (lstTags.SelectedItem is not string tag) return;
        int pos = txtEditor.SelectionStart;
        txtEditor.Text = txtEditor.Text.Insert(pos, tag);
        txtEditor.SelectionStart = pos + tag.Length;
        txtEditor.Focus();
    }

    private void Save()
    {
        try
        {
            File.WriteAllText(HtmlTemplates.CustomPath(_name), txtEditor.Text);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, AppServices.Languages.Get("Common.SaveError", ex.Message));
            DialogResult = DialogResult.None;
        }
    }
}
