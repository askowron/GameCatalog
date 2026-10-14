using System.Reflection;
using GameCatalog.Core.Catalog;
using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;
using GameCatalog.Plugins.Abstractions;

namespace GameCatalog.App;

/// <summary>Podgląd zmian ze skryptu przed zastosowaniem (pola + okładka + DLC).</summary>
public sealed class ScriptResultForm : Form
{
    private readonly CheckedListBox _fields = new() { Dock = DockStyle.Fill, CheckOnClick = true };
    private readonly CheckBox _chkCover = new() { Text = "Okładka", AutoSize = true, Dock = DockStyle.Top };
    private readonly ListBox _extras = new() { Dock = DockStyle.Fill };
    private readonly List<(string Tag, string Value)> _checked = new();

    public IReadOnlyList<(string Tag, string Value)> CheckedFields => _checked;
    public bool IncludeCover { get; private set; }

    public ScriptResultForm(Game game, GameInfoResult result)
    {
        Text = "Wyniki skryptu";
        Width = 560; Height = 480;
        StartPosition = FormStartPosition.CenterParent;

        foreach (var kv in result.Fields)
        {
            string old = GameFieldValues.Get(game, kv.Key);
            if (old == kv.Value) continue;
            string name = CatalogLabels.Current.Field(kv.Key);
            _fields.Items.Add($"{name}: „{Trunc(old)}” → „{Trunc(kv.Value)}”", true);
            _checked.Add((kv.Key, kv.Value));
        }
        foreach (var kv in result.CustomFields)
        {
            game.CustomValues.TryGetValue(kv.Key, out var old);
            if ((old ?? "") == kv.Value) continue;
            _fields.Items.Add($"{kv.Key}: „{Trunc(old ?? "")}” → „{Trunc(kv.Value)}”", true);
            _checked.Add(("custom:" + kv.Key, kv.Value));
        }

        var top = new Panel { Dock = DockStyle.Top, Height = 30 };
        top.Controls.Add(new Label { Text = $"Pola do zastosowania ({_checked.Count}):", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft });
        var mid = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 220 };
        mid.Panel1.Controls.Add(_fields);
        mid.Panel1.Controls.Add(top);
        var bottom = new Panel { Dock = DockStyle.Fill };
        if (result.Cover is { Length: > 0 })
        {
            _chkCover.Text = $"Okładka ({result.Cover.Length / 1024} KB)";
            _chkCover.Checked = true;
        }
        else _chkCover.Enabled = false;
        var exLabel = new Label { Text = $"Dodatki/DLC ({result.Extras.Count}):", Dock = DockStyle.Top, Height = 22 };
        foreach (var e in result.Extras) _extras.Items.Add($"{e.Title} [{e.Category}]");
        bottom.Controls.Add(_extras);
        bottom.Controls.Add(exLabel);
        bottom.Controls.Add(_chkCover);
        mid.Panel2.Controls.Add(bottom);

        var log = new TextBox
        {
            Dock = DockStyle.Bottom, Height = 90, Multiline = true,
            ReadOnly = true, ScrollBars = ScrollBars.Vertical,
            Text = string.Join(Environment.NewLine, result.Log),
        };
        var bar = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 44, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(8) };
        var ok = new Button { Text = "Zastosuj", DialogResult = DialogResult.OK, Width = 90 };
        var cancel = new Button { Text = "Anuluj", DialogResult = DialogResult.Cancel, Width = 90 };
        ok.Click += (_, _) =>
        {
            _checked.Clear();
            int i = 0;
            foreach (var kv in result.Fields)
            {
                if (GameFieldValues.Get(game, kv.Key) == kv.Value) continue;
                if (_fields.CheckedIndices.Contains(i)) _checked.Add((kv.Key, kv.Value));
                i++;
            }
            foreach (var kv in result.CustomFields)
            {
                game.CustomValues.TryGetValue(kv.Key, out var old);
                if ((old ?? "") == kv.Value) continue;
                if (_fields.CheckedIndices.Contains(i)) _checked.Add(("custom:" + kv.Key, kv.Value));
                i++;
            }
            IncludeCover = _chkCover.Enabled && _chkCover.Checked;
        };
        bar.Controls.Add(ok); bar.Controls.Add(cancel);
        Controls.Add(mid);
        Controls.Add(log);
        Controls.Add(bar);
        AcceptButton = ok; CancelButton = cancel;
    }

    private static string Trunc(string s) =>
        string.IsNullOrEmpty(s) ? "—" : (s.Length > 60 ? s[..57] + "…" : s);
}
