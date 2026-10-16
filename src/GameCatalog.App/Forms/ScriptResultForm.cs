using GameCatalog.App.Services;
using GameCatalog.Core.Catalog;
using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;
using GameCatalog.Plugins.Abstractions;

namespace GameCatalog.App.Forms;

/// <summary>Preview of script results before applying them (fields, cover, extras) — AMC getscript_results.</summary>
public partial class ScriptResultForm : Form
{
    private readonly GameInfoResult _result;
    // One entry per row of clbFields, in the same order.
    private readonly List<(string Tag, string Value)> _changes = new();
    private readonly List<(string Tag, string Value)> _checked = new();

    public IReadOnlyList<(string Tag, string Value)> CheckedFields => _checked;
    public bool IncludeCover { get; private set; }

    /// <summary>True when the user selected anything to apply.</summary>
    public bool HasSelection =>
        _checked.Count > 0 || _result.Extras.Count > 0 || (IncludeCover && _result.Cover is not null);

    public ScriptResultForm(Game game, GameInfoResult result)
    {
        InitializeComponent();
        _result = result;
        var l = AppServices.Languages;

        foreach (var kv in result.Fields)
        {
            string old = GameFieldValues.Get(game, kv.Key);
            if (old == kv.Value) continue;
            AddChange(kv.Key, CatalogLabels.Current.Field(kv.Key), old, kv.Value);
        }
        foreach (var kv in result.CustomFields)
        {
            game.CustomValues.TryGetValue(kv.Key, out var old);
            if ((old ?? "") == kv.Value) continue;
            AddChange("custom:" + kv.Key, kv.Key, old ?? "", kv.Value);
        }
        _checked.AddRange(_changes);
        lblFields.Text = l.Get("ScriptResult.FieldsCount", _changes.Count);

        if (result.Cover is { Length: > 0 })
        {
            chkCover.Text = l.Get("ScriptResult.Cover", result.Cover.Length / 1024);
            chkCover.Checked = true;
        }
        else
        {
            chkCover.Text = l.Get("ScriptResult.NoCover");
            chkCover.Enabled = false;
        }

        lblExtras.Text = l.Get("ScriptResult.ExtrasCount", result.Extras.Count);
        foreach (var e in result.Extras) lstExtras.Items.Add($"{e.Title} [{e.Category}]");
        txtLog.Text = string.Join(Environment.NewLine, result.Log);

        btnApply.Click += (_, _) =>
        {
            _checked.Clear();
            foreach (int i in clbFields.CheckedIndices) _checked.Add(_changes[i]);
            IncludeCover = chkCover.Enabled && chkCover.Checked;
        };
    }

    private void AddChange(string tag, string name, string oldValue, string newValue)
    {
        _changes.Add((tag, newValue));
        clbFields.Items.Add(AppServices.Languages.Get("ScriptResult.Change", name, Trunc(oldValue), Trunc(newValue)), true);
    }

    private static string Trunc(string s) =>
        string.IsNullOrEmpty(s) ? "—" : (s.Length > 60 ? s[..57] + "…" : s);
}
