using GameCatalog.App.Services;
using GameCatalog.Core.Models;

namespace GameCatalog.App.Forms;

/// <summary>Edits one custom field definition (AMC customfieldsmanager.pas, field properties).</summary>
public partial class CustomFieldEditForm : Form
{
    public CustomFieldEditForm(CustomFieldDefinition def, IEnumerable<string> takenTags)
    {
        InitializeComponent();
        var taken = new HashSet<string>(takenTags, StringComparer.OrdinalIgnoreCase);
        txtTag.Text = def.Tag;
        txtName.Text = def.Name;
        cbxType.SelectedItem = def.FieldType;
        if (cbxType.SelectedIndex < 0) cbxType.SelectedIndex = 0;
        txtListValues.Text = def.ListValues;

        btnOk.Click += (_, _) =>
        {
            var l = AppServices.Languages;
            var tag = txtTag.Text.Trim();
            if (tag == "" || taken.Contains(tag))
            {
                MessageBox.Show(this, l.Get("CustomFieldEdit.TagInvalid"));
                DialogResult = DialogResult.None;
                return;
            }
            if (txtName.Text.Trim() == "")
            {
                MessageBox.Show(this, l.Get("CustomFieldEdit.NameRequired"));
                DialogResult = DialogResult.None;
                return;
            }
            def.Tag = tag;
            def.Name = txtName.Text.Trim();
            def.FieldType = cbxType.SelectedItem!.ToString()!;
            def.ListValues = txtListValues.Text.Trim();
        };
    }
}
