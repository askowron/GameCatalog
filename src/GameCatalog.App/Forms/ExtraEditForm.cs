using GameCatalog.Core.Models;

namespace GameCatalog.App;

/// <summary>Edytor dodatku/DLC — odpowiednik AMC extrasedit.pas.</summary>
public sealed class ExtraEditForm : Form
{
    public GameExtra Extra { get; }

    public ExtraEditForm(GameExtra extra)
    {
        Extra = extra;
        Text = "Dodatek / DLC";
        Width = 480; Height = 400;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; MinimizeBox = false;

        var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(10) };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var txtTitle = new TextBox { Dock = DockStyle.Fill, Text = extra.Title };
        var cbCat = new ComboBox { Dock = DockStyle.Fill, Text = extra.Category };
        cbCat.Items.AddRange(ExtraCategories.All.Cast<object>().ToArray());
        var txtTag = new TextBox { Dock = DockStyle.Fill, Text = extra.Tag };
        var txtBy = new TextBox { Dock = DockStyle.Fill, Text = extra.CreatedBy };
        var txtUrl = new TextBox { Dock = DockStyle.Fill, Text = extra.Url };
        var txtDesc = new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical, Height = 80, Dock = DockStyle.Fill, Text = extra.Description };
        var txtComm = new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical, Height = 60, Dock = DockStyle.Fill, Text = extra.Comments };
        int r = 0;
        void Row(string l, Control c) { t.Controls.Add(new Label { Text = l, AutoSize = true }, 0, r); t.Controls.Add(c, 1, r++); }
        Row("Tytuł:", txtTitle);
        Row("Kategoria:", cbCat);
        Row("Etykieta:", txtTag);
        Row("Twórca:", txtBy);
        Row("URL:", txtUrl);
        Row("Opis:", txtDesc);
        Row("Komentarze:", txtComm);
        Controls.Add(t);

        var bar = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 44, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(8) };
        var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Width = 90 };
        var cancel = new Button { Text = "Anuluj", DialogResult = DialogResult.Cancel, Width = 90 };
        ok.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show(this, "Tytuł jest wymagany.", "Walidacja", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }
            Extra.Title = txtTitle.Text.Trim();
            Extra.Category = cbCat.Text.Trim();
            Extra.Tag = txtTag.Text.Trim();
            Extra.CreatedBy = txtBy.Text.Trim();
            Extra.Url = txtUrl.Text.Trim();
            Extra.Description = txtDesc.Text;
            Extra.Comments = txtComm.Text;
        };
        bar.Controls.Add(ok); bar.Controls.Add(cancel);
        Controls.Add(bar);
        AcceptButton = ok; CancelButton = cancel;
    }
}
