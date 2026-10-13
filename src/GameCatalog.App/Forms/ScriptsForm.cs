using GameCatalog.Core.Models;
using GameCatalog.Scripting;

namespace GameCatalog.App;

/// <summary>Dialog wyboru z listy dla skryptów (jak PickList w AMC).</summary>
public sealed class PickForm : Form
{
    public int SelectedIndex { get; private set; } = -1;
    private readonly ListBox _list = new() { Dock = DockStyle.Fill };

    public PickForm(string title, IList<string> options)
    {
        Text = title;
        Width = 420; Height = 340;
        StartPosition = FormStartPosition.CenterParent;
        foreach (var o in options) _list.Items.Add(o);
        if (_list.Items.Count > 0) _list.SelectedIndex = 0;
        _list.DoubleClick += (_, _) => { SelectedIndex = _list.SelectedIndex; DialogResult = DialogResult.OK; Close(); };
        var bar = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 44, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(8) };
        var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Width = 80 };
        var cancel = new Button { Text = "Anuluj", DialogResult = DialogResult.Cancel, Width = 80 };
        ok.Click += (_, _) => SelectedIndex = _list.SelectedIndex;
        bar.Controls.Add(ok); bar.Controls.Add(cancel);
        Controls.Add(_list);
        Controls.Add(bar);
        AcceptButton = ok; CancelButton = cancel;
    }
}

/// <summary>
/// Skrypty — menedżer + edytor + uruchamianie (odpowiednik AMC getscript.pas).
/// Skrypty C# (.gcsx) pobierają dane o grach z internetu przez obiekt Api.
/// </summary>
public sealed class ScriptsForm : Form
{
    private readonly Game _game;
    private readonly Func<GameScript, Task> _runTest;
    private readonly ListBox _scripts = new() { Dock = DockStyle.Fill };
    private readonly TextBox _editor = new()
    {
        Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Both,
        Font = new Font("Consolas", 10), WordWrap = false, AcceptsTab = true,
    };
    private readonly TextBox _log = new()
    {
        Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
    };
    private readonly Label _props = new() { Dock = DockStyle.Fill, Padding = new Padding(6) };
    private bool _modified;

    public ScriptsForm(Game game, Func<GameScript, Task> runTest)
    {
        _game = game;
        _runTest = runTest;
        Text = $"Skrypty — {game.Title}";
        Width = 900; Height = 600;
        StartPosition = FormStartPosition.CenterParent;

        var left = new Panel { Dock = DockStyle.Left, Width = 220 };
        left.Controls.Add(_scripts);
        _scripts.SelectedIndexChanged += (_, _) => LoadSelected();
        var leftBar = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 40, Padding = new Padding(4) };
        var bNew = new Button { Text = "Nowy", AutoSize = true };
        var bDel = new Button { Text = "Usuń", AutoSize = true };
        bNew.Click += (_, _) => NewScript();
        bDel.Click += (_, _) => DeleteScript();
        leftBar.Controls.Add(bNew); leftBar.Controls.Add(bDel);
        left.Controls.Add(leftBar);

        var right = new Panel { Dock = DockStyle.Fill };
        var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 350 };
        split.Panel1.Controls.Add(_editor);
        _editor.TextChanged += (_, _) => _modified = true;
        var logPanel = new Panel { Dock = DockStyle.Fill };
        logPanel.Controls.Add(_log);
        logPanel.Controls.Add(new Label { Text = "Dziennik:", Dock = DockStyle.Top, Height = 20 });
        split.Panel2.Controls.Add(logPanel);
        var propsPanel = new Panel { Dock = DockStyle.Top, Height = 88 };
        propsPanel.Controls.Add(_props);
        var bar = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 44, Padding = new Padding(8) };
        var bRun = new Button { Text = "Uruchom testowo", AutoSize = true };
        var bSave = new Button { Text = "Zapisz", AutoSize = true };
        var bClose = new Button { Text = "Zamknij", DialogResult = DialogResult.Cancel, AutoSize = true };
        bRun.Click += async (_, _) => await RunCurrentAsync();
        bSave.Click += (_, _) => SaveCurrent();
        bar.Controls.AddRange(new Control[] { bRun, bSave, bClose });
        right.Controls.Add(split);
        right.Controls.Add(propsPanel);
        right.Controls.Add(bar);

        Controls.Add(right);
        Controls.Add(left);
        CancelButton = bClose;
        RefreshScripts();
    }

    private void RefreshScripts()
    {
        int idx = _scripts.SelectedIndex;
        _scripts.Items.Clear();
        foreach (var s in GameScript.LoadAll()) _scripts.Items.Add(s);
        _scripts.SelectedIndex = Math.Clamp(idx, -1, _scripts.Items.Count - 1);
        if (_scripts.SelectedIndex < 0 && _scripts.Items.Count > 0) _scripts.SelectedIndex = 0;
    }

    private GameScript? Selected() => _scripts.SelectedItem as GameScript;

    private void LoadSelected()
    {
        if (_modified && Selected() is not null)
        {
            var r = MessageBox.Show(this, "Zapisać zmiany w skrypcie?", "Skrypty",
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (r == DialogResult.Yes) SaveCurrent();
            else if (r == DialogResult.Cancel) return;
        }
        var s = Selected();
        _editor.Text = s?.Code ?? "";
        _modified = false;
        _props.Text = s is null ? "" :
            $"Nazwa: {s.Name}\nOpis: {s.Description}\nWersja: {s.Version}\nAutor: {s.Author}\nStrona: {s.Site}";
    }

    private void NewScript()
    {
        var path = Path.Combine(GameScript.ScriptsDir(), "Nowy skrypt.gcsx");
        int i = 2;
        while (File.Exists(path)) path = Path.Combine(GameScript.ScriptsDir(), $"Nowy skrypt {i++}.gcsx");
        File.WriteAllText(path,
            "// Name: Nowy skrypt\n// Description: \n// Version: 1.0\n// Author: \n// Site: \nApi.Log(\"Hello \" + Api.Title);\n");
        RefreshScripts();
        for (int k = 0; k < _scripts.Items.Count; k++)
            if ((_scripts.Items[k] as GameScript)?.FilePath == path) { _scripts.SelectedIndex = k; break; }
    }

    private void DeleteScript()
    {
        if (Selected() is not GameScript s) return;
        if (MessageBox.Show(this, $"Usunąć skrypt „{s.Name}”?", "Potwierdź",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        try { File.Delete(s.FilePath); } catch (Exception ex) { MessageBox.Show(this, ex.Message); return; }
        _modified = false;
        RefreshScripts();
    }

    private void SaveCurrent()
    {
        if (Selected() is not GameScript s) return;
        s.Code = _editor.Text;
        try
        {
            s.Save();
            _modified = false;
            var reloaded = GameScript.Load(s.FilePath);
            s.Name = reloaded.Name; s.Description = reloaded.Description;
            s.Version = reloaded.Version; s.Author = reloaded.Author; s.Site = reloaded.Site;
            _props.Text = $"Nazwa: {s.Name}\nOpis: {s.Description}\nWersja: {s.Version}\nAutor: {s.Author}\nStrona: {s.Site}";
            _scripts.Refresh();
        }
        catch (Exception ex) { MessageBox.Show(this, "Błąd zapisu: " + ex.Message); }
    }

    private async Task RunCurrentAsync()
    {
        if (Selected() is not GameScript s) return;
        SaveCurrent();
        _log.Clear();
        _log.Text = "Uruchamiam…\r\n";
        await _runTest(new GameScript
        {
            FilePath = s.FilePath, Name = s.Name, Description = s.Description,
            Version = s.Version, Author = s.Author, Site = s.Site, Code = _editor.Text,
        });
    }

    public void AppendLog(IEnumerable<string> lines)
    {
        _log.AppendText(string.Join(Environment.NewLine, lines) + Environment.NewLine);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_modified)
        {
            var r = MessageBox.Show(this, "Zapisać zmiany w skrypcie?", "Skrypty",
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (r == DialogResult.Yes) SaveCurrent();
            else if (r == DialogResult.Cancel) e.Cancel = true;
        }
        base.OnFormClosing(e);
    }
}
