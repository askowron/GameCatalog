using GameCatalog.Core.Models;

namespace GameCatalog.App;

/// <summary>Zarządzanie polami własnymi — odpowiednik AMC customfieldsmanager.pas.</summary>
public sealed class CustomFieldsForm : Form
{
    private readonly List<CustomFieldDefinition> _defs;
    private readonly ListBox _list = new() { Dock = DockStyle.Fill };

    public CustomFieldsForm(List<CustomFieldDefinition> defs)
    {
        _defs = defs;
        Text = "Zarządzanie polami";
        Width = 460; Height = 360;
        StartPosition = FormStartPosition.CenterParent;
        RefreshList();
        var bar = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 44, Padding = new Padding(8) };
        var bAdd = new Button { Text = "Dodaj…", AutoSize = true };
        var bEdit = new Button { Text = "Edytuj…", AutoSize = true };
        var bDel = new Button { Text = "Usuń", AutoSize = true };
        var bClose = new Button { Text = "Zamknij", DialogResult = DialogResult.OK, AutoSize = true };
        bAdd.Click += (_, _) =>
        {
            var d = new CustomFieldDefinition { Tag = "Pole" + (_defs.Count + 1), Name = "Nowe pole" };
            using var f = new CustomFieldEditForm(d, _defs.Select(x => x.Tag));
            if (f.ShowDialog(this) == DialogResult.OK) { _defs.Add(d); RefreshList(); }
        };
        bEdit.Click += (_, _) =>
        {
            if (_list.SelectedItem is not CustomFieldDefinition d) return;
            using var f = new CustomFieldEditForm(d, _defs.Where(x => x != d).Select(x => x.Tag));
            if (f.ShowDialog(this) == DialogResult.OK) RefreshList();
        };
        bDel.Click += (_, _) =>
        {
            if (_list.SelectedItem is CustomFieldDefinition d
                && MessageBox.Show(this, $"Usunąć pole „{d.Name}”?", "Potwierdź",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            { _defs.Remove(d); RefreshList(); }
        };
        bar.Controls.AddRange(new Control[] { bAdd, bEdit, bDel, bClose });
        Controls.Add(_list);
        Controls.Add(bar);
        _list.DoubleClick += (_, _) => bEdit.PerformClick();
    }

    private void RefreshList()
    {
        _list.Items.Clear();
        foreach (var d in _defs) _list.Items.Add(d);
    }
}

public sealed class CustomFieldEditForm : Form
{
    public CustomFieldEditForm(CustomFieldDefinition def, IEnumerable<string> takenTags)
    {
        Text = "Pole własne";
        Width = 400; Height = 280;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; MinimizeBox = false;
        var taken = new HashSet<string>(takenTags, StringComparer.OrdinalIgnoreCase);
        var txtTag = new TextBox { Dock = DockStyle.Fill, Text = def.Tag };
        var txtName = new TextBox { Dock = DockStyle.Fill, Text = def.Name };
        var cbType = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        cbType.Items.AddRange(new object[] { "Text", "Number", "Date", "List", "Url", "YesNo" });
        cbType.SelectedItem = def.FieldType;
        if (cbType.SelectedIndex < 0) cbType.SelectedIndex = 0;
        var txtList = new TextBox { Dock = DockStyle.Fill, Text = def.ListValues };
        var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(10) };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        int r = 0;
        void Row(string l, Control c) { t.Controls.Add(new Label { Text = l, AutoSize = true }, 0, r); t.Controls.Add(c, 1, r++); }
        Row("Tag:", txtTag);
        Row("Nazwa:", txtName);
        Row("Typ:", cbType);
        Row("Wartości listy (;):", txtList);
        Controls.Add(t);
        var bar = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 44, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(8) };
        var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Width = 80 };
        var cancel = new Button { Text = "Anuluj", DialogResult = DialogResult.Cancel, Width = 80 };
        ok.Click += (_, _) =>
        {
            var tag = txtTag.Text.Trim();
            if (tag == "" || taken.Contains(tag)) { MessageBox.Show(this, "Tag musi być niepusty i unikalny."); DialogResult = DialogResult.None; return; }
            if (txtName.Text.Trim() == "") { MessageBox.Show(this, "Nazwa jest wymagana."); DialogResult = DialogResult.None; return; }
            def.Tag = tag; def.Name = txtName.Text.Trim();
            def.FieldType = cbType.SelectedItem!.ToString()!;
            def.ListValues = txtList.Text.Trim();
        };
        bar.Controls.Add(ok); bar.Controls.Add(cancel);
        Controls.Add(bar);
        AcceptButton = ok; CancelButton = cancel;
    }
}
