using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;

namespace GameCatalog.UI.Frames;

/// <summary>Fixed game fields editor (AMC framemovie.pas, "Game fields" tab).</summary>
public partial class GameFrame : UserControl, IGameFrame
{
    private readonly Dictionary<string, Control> _editors;
    private readonly Dictionary<string, string[]> _presets;
    private Game? _game;
    private bool _loading;

    public GameFrame()
    {
        InitializeComponent();
        _editors = new Dictionary<string, Control>(StringComparer.Ordinal)
        {
            ["MediaLabel"] = txtMediaLabel, ["MediaType"] = cbxMediaType, ["Store"] = cbxStore,
            ["DateAdded"] = dtpDateAdded, ["Borrower"] = txtBorrower, ["DateFinished"] = dtpDateFinished,
            ["UserRating"] = numUserRating, ["UserRatingNone"] = chkUserRatingNone,
            ["CriticRating"] = numCriticRating, ["CriticRatingNone"] = chkCriticRatingNone,
            ["Title"] = txtTitle, ["LocalizedTitle"] = txtLocalizedTitle, ["Developer"] = cbxDeveloper,
            ["Publisher"] = cbxPublisher, ["Designer"] = txtDesigner, ["Composer"] = txtComposer,
            ["VoiceActors"] = txtVoiceActors, ["Country"] = cbxCountry, ["ReleaseYear"] = numReleaseYear,
            ["PlaytimeHours"] = numPlaytimeHours, ["Genre"] = cbxGenre, ["Pegi"] = cbxPegi, ["URL"] = txtURL,
            ["Description"] = txtDescription, ["Comments"] = txtComments, ["Checked"] = chkChecked,
            ["InstallPath"] = txtInstallPath, ["Engine"] = cbxEngine, ["Version"] = txtVersion,
            ["GameMode"] = cbxGameMode, ["Platform"] = cbxPlatform, ["Resolution"] = txtResolution,
            ["Framerate"] = txtFramerate, ["AudioLanguages"] = txtAudioLanguages, ["Subtitles"] = txtSubtitles,
            ["SizeGb"] = txtSizeGb, ["DiskCount"] = numDiskCount, ["LastPlayed"] = dtpLastPlayed,
            ["CompletionStatus"] = cbxCompletionStatus,
        };
        _presets = new Dictionary<string, string[]>
        {
            ["Store"] = Stores.All, ["MediaType"] = MediaTypes.All, ["Platform"] = Platforms.All,
            ["GameMode"] = GameModes.All, ["Pegi"] = new[] { "", "PEGI 3", "PEGI 7", "PEGI 12", "PEGI 16", "PEGI 18" },
        };
        foreach (var tag in _presets.Keys) SetSuggestions(tag, Array.Empty<string>());

        foreach (var (tag, editor) in _editors)
        {
            string fieldTag = tag.EndsWith("None", StringComparison.Ordinal) ? tag[..^4] : tag;
            switch (editor)
            {
                case CheckBox box:
                    box.CheckedChanged += (_, _) => Commit(fieldTag);
                    break;
                case ComboBox { DropDownStyle: ComboBoxStyle.DropDownList } list:
                    list.SelectionChangeCommitted += (_, _) => Commit(fieldTag);
                    break;
                default:
                    editor.Leave += (_, _) => Commit(fieldTag);
                    break;
            }
        }
        chkUserRatingNone.CheckedChanged += (_, _) => numUserRating.Enabled = !chkUserRatingNone.Checked;
        chkCriticRatingNone.CheckedChanged += (_, _) => numCriticRating.Enabled = !chkCriticRatingNone.Checked;
        btnUrl.Click += (_, _) => OpenUrlRequested?.Invoke(this, EventArgs.Empty);
        RefreshCaptions();
        Enabled = false;
    }

    public event EventHandler<FrameModifiedEventArgs>? Modified;
    public event EventHandler? OpenUrlRequested;

    /// <summary>Editor control of a field tag (also "UserRatingNone"/"CriticRatingNone").</summary>
    public Control? EditorFor(string tag) => _editors.GetValueOrDefault(tag);

    public void FocusTitle()
    {
        txtTitle.Focus();
        txtTitle.SelectAll();
    }

    /// <summary>Re-fills captions that come from the core labels (status list) after a language change.</summary>
    public void RefreshCaptions()
    {
        int index = cbxCompletionStatus.SelectedIndex;
        cbxCompletionStatus.Items.Clear();
        foreach (var s in Enum.GetValues<CompletionStatus>())
            cbxCompletionStatus.Items.Add(CatalogLabels.Current.Status(s));
        cbxCompletionStatus.SelectedIndex = index;
    }

    /// <summary>Drop-down values: presets for the field, then catalog values (AMC lists).</summary>
    public void SetSuggestions(string tag, IEnumerable<string> values)
    {
        if (_editors.GetValueOrDefault(tag) is not ComboBox { DropDownStyle: ComboBoxStyle.DropDown } box) return;
        var text = box.Text;
        var all = _presets.GetValueOrDefault(tag, Array.Empty<string>())
            .Concat(values.Where(v => !string.IsNullOrWhiteSpace(v)).OrderBy(v => v, StringComparer.CurrentCulture))
            .Distinct()
            .ToArray();
        box.BeginUpdate();
        box.Items.Clear();
        box.Items.AddRange(all);
        box.EndUpdate();
        box.Text = text;
    }

    public void Bind(Game? game)
    {
        _loading = true;
        try
        {
            _game = game;
            Enabled = game is not null;
            if (game is null) return;
            foreach (var tag in _editors.Keys)
            {
                if (tag.EndsWith("None", StringComparison.Ordinal)) continue;
                LoadField(game, tag);
            }
        }
        finally { _loading = false; }
    }

    public void Commit()
    {
        foreach (var tag in _editors.Keys.Where(t => !t.EndsWith("None", StringComparison.Ordinal)))
            Commit(tag);
    }

    public void Commit(string tag)
    {
        if (_loading || _game is null) return;
        if (Store(_game, tag)) Modified?.Invoke(this, new FrameModifiedEventArgs(tag));
    }

    private void LoadField(Game g, string tag)
    {
        switch (tag)
        {
            case "UserRating": SetRating(numUserRating, chkUserRatingNone, g.UserRating); break;
            case "CriticRating": SetRating(numCriticRating, chkCriticRatingNone, g.CriticRating); break;
            case "ReleaseYear": SetNumber(numReleaseYear, g.ReleaseYear); break;
            case "PlaytimeHours": SetNumber(numPlaytimeHours, (decimal)g.PlaytimeHours); break;
            case "DiskCount": SetNumber(numDiskCount, g.DiskCount); break;
            case "DateAdded": SetDate(dtpDateAdded, g.DateAdded); break;
            case "DateFinished": SetDate(dtpDateFinished, g.DateFinished); break;
            case "LastPlayed": SetDate(dtpLastPlayed, g.LastPlayed); break;
            case "Checked": chkChecked.Checked = g.Checked; break;
            case "CompletionStatus": cbxCompletionStatus.SelectedIndex = (int)g.CompletionStatus; break;
            default: _editors[tag].Text = GameFieldValues.Get(g, tag); break;
        }
    }

    /// <returns>True when the game changed.</returns>
    private bool Store(Game g, string tag)
    {
        switch (tag)
        {
            case "UserRating": return Assign(g.UserRating, GetRating(numUserRating, chkUserRatingNone), v => g.UserRating = v);
            case "CriticRating": return Assign(g.CriticRating, GetRating(numCriticRating, chkCriticRatingNone), v => g.CriticRating = v);
            case "ReleaseYear": return Assign(g.ReleaseYear, (int)numReleaseYear.Value, v => g.ReleaseYear = v);
            case "PlaytimeHours": return Assign(g.PlaytimeHours, (double)numPlaytimeHours.Value, v => g.PlaytimeHours = v);
            case "DiskCount": return Assign(g.DiskCount, (int)numDiskCount.Value, v => g.DiskCount = v);
            case "DateAdded": return Assign(g.DateAdded, GetDate(dtpDateAdded), v => g.DateAdded = v);
            case "DateFinished": return Assign(g.DateFinished, GetDate(dtpDateFinished), v => g.DateFinished = v);
            case "LastPlayed": return Assign(g.LastPlayed, GetDate(dtpLastPlayed), v => g.LastPlayed = v);
            case "Checked": return Assign(g.Checked, chkChecked.Checked, v => g.Checked = v);
            case "CompletionStatus":
                return cbxCompletionStatus.SelectedIndex >= 0 &&
                       Assign(g.CompletionStatus, (CompletionStatus)cbxCompletionStatus.SelectedIndex, v => g.CompletionStatus = v);
            default:
                var text = _editors[tag].Text;
                if (GameFieldValues.Get(g, tag) == text) return false;
                return GameFieldWriter.Set(g, tag, text);
        }
    }

    private static bool Assign<T>(T old, T value, Action<T> apply)
    {
        if (EqualityComparer<T>.Default.Equals(old, value)) return false;
        apply(value);
        return true;
    }

    private static void SetNumber(NumericUpDown n, decimal v) => n.Value = Math.Clamp(v, n.Minimum, n.Maximum);

    private static void SetDate(DateTimePicker p, DateTime? d)
    {
        p.Value = d ?? DateTime.Today;
        p.Checked = d.HasValue;
    }

    private static DateTime? GetDate(DateTimePicker p) => p.Checked ? p.Value.Date : null;

    private static void SetRating(NumericUpDown n, CheckBox none, int rating)
    {
        none.Checked = rating < 0;
        n.Value = rating < 0 ? 0 : Math.Clamp(rating / 10m, 0, 10);
    }

    private static int GetRating(NumericUpDown n, CheckBox none) =>
        none.Checked ? -1 : (int)Math.Round(n.Value * 10, MidpointRounding.AwayFromZero);
}
