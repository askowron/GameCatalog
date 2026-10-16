using GameCatalog.App.Services;
using GameCatalog.Core.Models;

namespace GameCatalog.App.Forms;

/// <summary>Custom field manager (AMC customfieldsmanager.pas).</summary>
public partial class CustomFieldsForm : Form
{
    private readonly List<CustomFieldDefinition> _defs;

    public CustomFieldsForm(List<CustomFieldDefinition> defs)
    {
        InitializeComponent();
        _defs = defs;
        RefreshList();
        btnAdd.Click += (_, _) => AddField();
        btnEdit.Click += (_, _) => EditField();
        btnDelete.Click += (_, _) => DeleteField();
        lstFields.DoubleClick += (_, _) => EditField();
    }

    private void AddField()
    {
        var l = AppServices.Languages;
        var d = new CustomFieldDefinition
        {
            Tag = l.Get("CustomFields.NewTag", _defs.Count + 1),
            Name = l.Get("CustomFields.NewName"),
        };
        using var f = new CustomFieldEditForm(d, _defs.Select(x => x.Tag));
        if (f.ShowDialog(this) != DialogResult.OK) return;
        _defs.Add(d);
        RefreshList();
    }

    private void EditField()
    {
        if (lstFields.SelectedItem is not CustomFieldDefinition d) return;
        using var f = new CustomFieldEditForm(d, _defs.Where(x => x != d).Select(x => x.Tag));
        if (f.ShowDialog(this) == DialogResult.OK) RefreshList();
    }

    private void DeleteField()
    {
        var l = AppServices.Languages;
        if (lstFields.SelectedItem is CustomFieldDefinition d
            && MessageBox.Show(this, l.Get("CustomFields.ConfirmDelete", d.Name), l.Get("Common.Confirm"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            _defs.Remove(d);
            RefreshList();
        }
    }

    private void RefreshList()
    {
        lstFields.Items.Clear();
        foreach (var d in _defs) lstFields.Items.Add(d);
    }
}
