using System.ComponentModel;

namespace GameCatalog.UI.Controls;

/// <summary>Find toolbar under the main window (AMC ToolbarFind).</summary>
public partial class FindBar : UserControl
{
    private List<string> _tags = new();
    private bool _filling;

    public FindBar()
    {
        InitializeComponent();
        cbxField.SelectedIndexChanged += (_, _) => OnCriteriaChanged();
        txtValue.TextChanged += (_, _) => OnCriteriaChanged();
        chkWhole.CheckedChanged += (_, _) => OnCriteriaChanged();
        chkShow.CheckedChanged += (_, _) => OnCriteriaChanged();
        chkReverse.CheckedChanged += (_, _) => OnCriteriaChanged();
        btnNext.Click += (_, _) => FindNextRequested?.Invoke(this, EventArgs.Empty);
        btnHide.Click += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);
        txtValue.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Escape) CloseRequested?.Invoke(this, EventArgs.Empty);
            else if (e.KeyCode == Keys.Enter) FindNextRequested?.Invoke(this, EventArgs.Empty);
        };
    }

    public event EventHandler? CriteriaChanged;
    public event EventHandler? FindNextRequested;
    public event EventHandler? CloseRequested;

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string FieldTag => cbxField.SelectedIndex >= 0 ? _tags[cbxField.SelectedIndex] : "Title";

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string SearchText { get => txtValue.Text.Trim(); set => txtValue.Text = value; }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool WholeField { get => chkWhole.Checked; set => chkWhole.Checked = value; }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool ShowResults { get => chkShow.Checked; set => chkShow.Checked = value; }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Reverse { get => chkReverse.Checked; set => chkReverse.Checked = value; }

    /// <summary>
    /// Fills the field list (captions in the current language) and keeps/sets the selection.
    /// CriteriaChanged is raised at most once, and only when the selected field tag changed.
    /// </summary>
    public void SetFields(IEnumerable<(string Tag, string Caption)> fields, string selectedTag)
    {
        var list = fields.ToList();
        string before = FieldTag;
        _filling = true;
        try
        {
            _tags = list.Select(f => f.Tag).ToList();
            cbxField.BeginUpdate();
            cbxField.Items.Clear();
            foreach (var f in list) cbxField.Items.Add(f.Caption);
            cbxField.EndUpdate();
            cbxField.SelectedIndex = list.Count == 0 ? -1 : Math.Max(0, _tags.IndexOf(selectedTag));
        }
        finally { _filling = false; }
        if (FieldTag != before) OnCriteriaChanged();
    }

    public void FocusText()
    {
        txtValue.Focus();
        txtValue.SelectAll();
    }

    private void OnCriteriaChanged()
    {
        if (!_filling) CriteriaChanged?.Invoke(this, EventArgs.Empty);
    }
}
