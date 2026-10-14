using System.ComponentModel;
using System.Runtime.CompilerServices;
using GameCatalog.UI.Icons;

namespace GameCatalog.UI.Components;

/// <summary>
/// Binds <see cref="UIAction"/>s to tool strip items (adds the "Action" property to menu items
/// and toolbar buttons in the Designer) and refreshes action state on idle — Delphi TActionList.
/// </summary>
[ToolboxItem(true), ProvideProperty("Action", typeof(ToolStripItem))]
public sealed class ActionList : Component, IExtenderProvider
{
    private static readonly ConditionalWeakTable<ToolStripItem, UIAction> Bound = new();
    private readonly Dictionary<ToolStripItem, UIAction> _items = new();
    private bool _autoUpdate = true, _idleHooked;

    public ActionList() { }

    public ActionList(IContainer container) : this() => container.Add(this);

    /// <summary>When true, every action's Update event is raised on Application.Idle.</summary>
    [DefaultValue(true)]
    public bool AutoUpdate
    {
        get => _autoUpdate;
        set { _autoUpdate = value; HookIdle(); }
    }

    /// <summary>Distinct actions bound to at least one item.</summary>
    [Browsable(false)]
    public IEnumerable<UIAction> Actions => _items.Values.Distinct();

    public bool CanExtend(object extendee) =>
        extendee is ToolStripMenuItem or ToolStripButton or ToolStripSplitButton or ToolStripDropDownButton;

    [DefaultValue(null), Category("Behavior"), Description("Action that drives this item.")]
    public UIAction? GetAction(ToolStripItem item) => _items.GetValueOrDefault(item);

    public void SetAction(ToolStripItem item, UIAction? action)
    {
        if (_items.Remove(item, out var old))
        {
            old.Changed -= OnActionChanged;
            item.Click -= OnItemClick;
            Bound.Remove(item);
        }
        if (action is null)
        {
            HookIdle();
            return;
        }
        _items[item] = action;
        Bound.AddOrUpdate(item, action);
        action.Changed += OnActionChanged;
        item.Click += OnItemClick;
        Sync(item, action);
        HookIdle();
    }

    /// <summary>Action bound to an item by any ActionList, or null.</summary>
    public static UIAction? FindAction(ToolStripItem item) => Bound.TryGetValue(item, out var a) ? a : null;

    public void UpdateActions()
    {
        foreach (var action in Actions.ToList()) action.PerformUpdate();
    }

    private void OnItemClick(object? sender, EventArgs e)
    {
        if (sender is ToolStripItem item && _items.TryGetValue(item, out var action)) action.PerformExecute();
    }

    private void OnActionChanged(object? sender, EventArgs e)
    {
        foreach (var (item, action) in _items)
            if (ReferenceEquals(action, sender)) Sync(item, action);
    }

    private static string WithoutMnemonic(string text) =>
        text.Replace("&&", "\0").Replace("&", "").Replace("\0", "&");

    private static void Sync(ToolStripItem item, UIAction a)
    {
        item.Text = a.Text;
        item.ToolTipText = a.ToolTipText.Length > 0 ? a.ToolTipText : WithoutMnemonic(a.Text);
        if (a.Icon != AmcIcon.None) item.ImageIndex = (int)a.Icon;
        item.Enabled = a.Enabled;
        item.Visible = a.Visible;
        switch (item)
        {
            case ToolStripMenuItem m:
                m.ShortcutKeys = a.ShortcutKeys;
                m.Checked = a.Checked;
                break;
            case ToolStripButton b:
                b.Checked = a.Checked;
                break;
        }
    }

    private void HookIdle()
    {
        bool want = _autoUpdate && _items.Count > 0 && !(Site?.DesignMode ?? false);
        if (want == _idleHooked) return;
        if (want) Application.Idle += OnIdle; else Application.Idle -= OnIdle;
        _idleHooked = want;
    }

    private void OnIdle(object? sender, EventArgs e) => UpdateActions();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var item in _items.Keys.ToList()) SetAction(item, null);
            if (_idleHooked) Application.Idle -= OnIdle;
            _idleHooked = false;
        }
        base.Dispose(disposing);
    }
}
