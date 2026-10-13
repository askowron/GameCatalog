namespace GameCatalog.App;

/// <summary>Edytor szablonu HTML karty gry — odpowiednik AMC HTMLEditor.pas (w wersji uproszczonej).</summary>
public sealed class HtmlEditorForm : Form
{
    private readonly string _name;
    private readonly TextBox _editor = new() { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Both, Font = new Font("Consolas", 9), WordWrap = false };
    private readonly ListBox _tags = new() { Dock = DockStyle.Fill };

    public HtmlEditorForm(string name)
    {
        _name = name;
        Text = $"Edytor HTML — {name}";
        Width = 760; Height = 540;
        StartPosition = FormStartPosition.CenterParent;
        _editor.Text = HtmlTemplates.Load(name);
        foreach (var t in new[] { "Title", "LocalizedTitle", "Developer", "Publisher", "Designer", "Composer",
                     "VoiceActors", "Country", "ReleaseYear", "PlaytimeHours", "Genre", "Pegi", "Platform",
                     "Store", "Engine", "GameMode", "Status", "UserRating", "CriticRating", "Url",
                     "Description", "Comments", "Number", "Cover", "Extras" })
            _tags.Items.Add("{" + t + "}");
        _tags.DoubleClick += (_, _) =>
        {
            if (_tags.SelectedItem is string tag)
            {
                int pos = _editor.SelectionStart;
                _editor.Text = _editor.Text.Insert(pos, tag);
                _editor.SelectionStart = pos + tag.Length;
                _editor.Focus();
            }
        };
        var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 560 };
        split.Panel1.Controls.Add(_editor);
        var tagPanel = new Panel { Dock = DockStyle.Fill };
        tagPanel.Controls.Add(_tags);
        tagPanel.Controls.Add(new Label { Text = "Znaczniki (2×klik wstawia):", Dock = DockStyle.Top, Height = 30 });
        split.Panel2.Controls.Add(tagPanel);
        var bar = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 44, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(8) };
        var ok = new Button { Text = "Zapisz", DialogResult = DialogResult.OK, Width = 90 };
        var reset = new Button { Text = "Przywróć domyślny", AutoSize = true };
        var cancel = new Button { Text = "Anuluj", DialogResult = DialogResult.Cancel, Width = 90 };
        reset.Click += (_, _) => _editor.Text = HtmlTemplates.Default(_name);
        ok.Click += (_, _) =>
        {
            try { File.WriteAllText(HtmlTemplates.CustomPath(_name), _editor.Text); }
            catch (Exception ex) { MessageBox.Show(this, "Błąd zapisu: " + ex.Message); DialogResult = DialogResult.None; }
        };
        bar.Controls.Add(ok); bar.Controls.Add(reset); bar.Controls.Add(cancel);
        Controls.Add(split);
        Controls.Add(bar);
        AcceptButton = ok; CancelButton = cancel;
    }
}
