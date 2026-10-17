using GameCatalog.App.Services;
using GameCatalog.Core.Models;
using GameCatalog.Scripting;

namespace GameCatalog.App.Forms;

/// <summary>
/// Script manager, editor and test runner (AMC getscript.pas).
/// C# scripts (.gcsx) fetch game data from the Internet through the Api object.
/// </summary>
public partial class ScriptsForm : Form
{
    private readonly Func<GameScript, Task> _runTest;
    private bool _modified;

    public ScriptsForm(Game game, Func<GameScript, Task> runTest)
    {
        InitializeComponent();
        _runTest = runTest;
        Text = AppServices.Languages.Get("Scripts.Caption", game.Title);

        lstScripts.SelectedIndexChanged += (_, _) => LoadSelected();
        txtEditor.TextChanged += (_, _) => _modified = true;
        btnNew.Click += (_, _) => NewScript();
        btnDelete.Click += (_, _) => DeleteScript();
        btnRun.Click += async (_, _) => await RunCurrentAsync();
        btnSave.Click += (_, _) => SaveCurrent();
        RefreshScripts();
    }

    private void RefreshScripts()
    {
        int idx = lstScripts.SelectedIndex;
        lstScripts.Items.Clear();
        foreach (var s in GameScript.LoadAll()) lstScripts.Items.Add(s);
        lstScripts.SelectedIndex = Math.Clamp(idx, -1, lstScripts.Items.Count - 1);
        if (lstScripts.SelectedIndex < 0 && lstScripts.Items.Count > 0) lstScripts.SelectedIndex = 0;
    }

    private GameScript? Selected() => lstScripts.SelectedItem as GameScript;

    private DialogResult AskSaveChanges() =>
        MessageBox.Show(this, AppServices.Languages.Get("Scripts.SaveChanges"), AppServices.Languages.Get("Scripts.Title"),
            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

    private void LoadSelected()
    {
        if (_modified && Selected() is not null)
        {
            var r = AskSaveChanges();
            if (r == DialogResult.Yes) SaveCurrent();
            else if (r == DialogResult.Cancel) return;
        }
        var s = Selected();
        txtEditor.Text = s?.Code ?? "";
        _modified = false;
        ShowProperties(s);
    }

    private void ShowProperties(GameScript? s) =>
        lblProperties.Text = s is null ? "" :
            AppServices.Languages.Get("Scripts.Properties", s.Name, s.Description, s.Version, s.Author, s.Site);

    private void NewScript()
    {
        string name = AppServices.Languages.Get("Scripts.NewName");
        var path = Path.Combine(GameScript.ScriptsDir(), name + ".gcsx");
        int i = 2;
        while (File.Exists(path)) path = Path.Combine(GameScript.ScriptsDir(), $"{name} {i++}.gcsx");
        File.WriteAllText(path,
            $"// Name: {name}\n// Description: \n// Version: 1.0\n// Author: \n// Site: \nApi.Log(\"Hello \" + Api.Title);\n");
        RefreshScripts();
        for (int k = 0; k < lstScripts.Items.Count; k++)
            if ((lstScripts.Items[k] as GameScript)?.FilePath == path) { lstScripts.SelectedIndex = k; break; }
    }

    private void DeleteScript()
    {
        if (Selected() is not GameScript s) return;
        var l = AppServices.Languages;
        if (MessageBox.Show(this, l.Get("Scripts.ConfirmDelete", s.Name), l.Get("Common.Confirm"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        try { File.Delete(s.FilePath); } catch (Exception ex) { MessageBox.Show(this, ex.Message); return; }
        _modified = false;
        RefreshScripts();
    }

    private void SaveCurrent()
    {
        if (Selected() is not GameScript s) return;
        s.Code = txtEditor.Text;
        try
        {
            s.Save();
            _modified = false;
            var reloaded = GameScript.Load(s.FilePath);
            s.Name = reloaded.Name; s.Description = reloaded.Description;
            s.Version = reloaded.Version; s.Author = reloaded.Author; s.Site = reloaded.Site;
            ShowProperties(s);
            lstScripts.Refresh();
        }
        catch (Exception ex) { MessageBox.Show(this, AppServices.Languages.Get("Common.SaveError", ex.Message)); }
    }

    private async Task RunCurrentAsync()
    {
        if (Selected() is not GameScript s) return;
        SaveCurrent();
        txtLog.Clear();
        txtLog.Text = AppServices.Languages.Get("Scripts.Running") + "\r\n";
        await _runTest(new GameScript
        {
            FilePath = s.FilePath, Name = s.Name, Description = s.Description,
            Version = s.Version, Author = s.Author, Site = s.Site, Code = txtEditor.Text,
        });
    }

    public void AppendLog(IEnumerable<string> lines)
    {
        txtLog.AppendText(string.Join(Environment.NewLine, lines) + Environment.NewLine);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_modified)
        {
            var r = AskSaveChanges();
            if (r == DialogResult.Yes) SaveCurrent();
            else if (r == DialogResult.Cancel) e.Cancel = true;
        }
        base.OnFormClosing(e);
    }
}
