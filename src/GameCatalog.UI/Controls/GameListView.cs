namespace GameCatalog.UI.Controls;

public sealed record GameListItem(int Number, bool Checked, int ColorTag, IReadOnlyList<string> Cells, string GroupKey);
public sealed record GameListGroup(string Key, string Caption);
public sealed class GameCheckedEventArgs(int number, bool @checked) : EventArgs
{
    public int Number { get; } = number;
    public bool Checked { get; } = @checked;
}
public sealed class ColumnSortEventArgs(string tag) : EventArgs
{
    public string Tag { get; } = tag;
}

/// <summary>
/// The catalog list (AMC ListView1, a TElTree): details view with check boxes, color tags and
/// groups. It only displays rows computed elsewhere and reports user actions through events.
/// </summary>
public sealed class GameListView : ListView
{
    private bool _filling;

    public GameListView()
    {
        View = View.Details;
        FullRowSelect = true;
        MultiSelect = false;
        CheckBoxes = true;
        HideSelection = false;
        ShowGroups = true;
        DoubleBuffered = true;
    }

    public event EventHandler? SelectedGameChanged;
    public event EventHandler<GameCheckedEventArgs>? GameCheckedChanged;
    public event EventHandler<ColumnSortEventArgs>? SortRequested;

    public int? SelectedNumber => SelectedItems.Count > 0 && SelectedItems[0].Tag is int n ? n : null;

    public void SetColumns(IEnumerable<(string Tag, int Width)> columns, Func<string, string> caption)
    {
        BeginUpdate();
        Columns.Clear();
        foreach (var (tag, width) in columns)
            Columns.Add(new ColumnHeader { Text = caption(tag), Width = width, Tag = tag });
        EndUpdate();
    }

    public IReadOnlyList<(string Tag, int Width)> ColumnWidths() =>
        Columns.Cast<ColumnHeader>().Select(c => ((string)c.Tag!, c.Width)).ToList();

    /// <summary>
    /// Replaces all rows and keeps the selected game when it is still present. SelectedGameChanged is
    /// raised once, and only when the selected game after the call differs from the one before.
    /// </summary>
    public void Display(IReadOnlyList<GameListItem> items, IReadOnlyList<GameListGroup> groups)
    {
        int? before = SelectedNumber;
        _filling = true;
        BeginUpdate();
        try
        {
            Groups.Clear();
            Items.Clear();
            var byKey = new Dictionary<string, ListViewGroup>();
            foreach (var g in groups)
            {
                var lg = new ListViewGroup(g.Caption, HorizontalAlignment.Left) { Tag = g.Key };
                Groups.Add(lg);
                byKey[g.Key] = lg;
            }
            foreach (var item in items)
            {
                var lvi = CreateItem(item);
                if (byKey.TryGetValue(item.GroupKey, out var lg)) lvi.Group = lg;
                Items.Add(lvi);
            }
            if (before is int n) SelectNumber(n);
        }
        finally
        {
            EndUpdate();
            _filling = false;
        }
        if (SelectedNumber != before) SelectedGameChanged?.Invoke(this, EventArgs.Empty);
    }

    public void UpdateItem(GameListItem item)
    {
        _filling = true;
        try
        {
            foreach (ListViewItem lvi in Items)
            {
                if (lvi.Tag is not int n || n != item.Number) continue;
                for (int i = 0; i < item.Cells.Count && i < lvi.SubItems.Count; i++)
                    lvi.SubItems[i].Text = item.Cells[i];
                lvi.Checked = item.Checked;
                var color = ColorTags.ToColor(item.ColorTag);
                lvi.BackColor = color.IsEmpty ? BackColor : color;
                break;
            }
        }
        finally { _filling = false; }
    }

    public bool SelectNumber(int number)
    {
        foreach (ListViewItem lvi in Items)
        {
            if (lvi.Tag is int n && n == number)
            {
                lvi.Selected = true;
                lvi.Focused = true;
                lvi.EnsureVisible();
                return true;
            }
        }
        return false;
    }

    /// <summary>Widens the first column to fill the remaining width (AMC "Stretch list").</summary>
    public void StretchFirstColumn()
    {
        if (Columns.Count == 0) return;
        int others = Columns.Cast<ColumnHeader>().Skip(1).Sum(c => c.Width);
        Columns[0].Width = Math.Max(40, ClientSize.Width - others - 4);
    }

    private static ListViewItem CreateItem(GameListItem item)
    {
        var lvi = new ListViewItem(item.Cells.Count > 0 ? item.Cells[0] : "") { Tag = item.Number, Checked = item.Checked };
        for (int i = 1; i < item.Cells.Count; i++) lvi.SubItems.Add(item.Cells[i]);
        var color = ColorTags.ToColor(item.ColorTag);
        if (!color.IsEmpty) lvi.BackColor = color;
        return lvi;
    }

    protected override void OnItemChecked(ItemCheckedEventArgs e)
    {
        base.OnItemChecked(e);
        if (!_filling && e.Item.Tag is int n) GameCheckedChanged?.Invoke(this, new GameCheckedEventArgs(n, e.Item.Checked));
    }

    protected override void OnSelectedIndexChanged(EventArgs e)
    {
        base.OnSelectedIndexChanged(e);
        if (!_filling) SelectedGameChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnColumnClick(ColumnClickEventArgs e)
    {
        base.OnColumnClick(e);
        if (Columns[e.Column].Tag is string tag) SortRequested?.Invoke(this, new ColumnSortEventArgs(tag));
    }
}
