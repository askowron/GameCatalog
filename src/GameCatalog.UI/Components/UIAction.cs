using System.ComponentModel;
using GameCatalog.Localization;
using GameCatalog.UI.Icons;

namespace GameCatalog.UI.Components;

/// <summary>
/// A user command shared by menu items, toolbar buttons and context menus — counterpart of
/// Delphi TAction. Bind it to items with <see cref="ActionList"/>; bound items mirror its
/// text, icon, shortcut and state.
/// </summary>
[ToolboxItem(true), DefaultEvent(nameof(Execute)), DefaultProperty(nameof(Text))]
public sealed class UIAction : Component
{
    private string _text = "", _toolTipText = "", _key = "";
    private string? _designText, _designToolTip;
    private AmcIcon _icon = AmcIcon.None;
    private Keys _shortcutKeys;
    private bool _enabled = true, _checked, _visible = true;

    public UIAction() { }

    public UIAction(IContainer container) : this() => container.Add(this);

    /// <summary>Translation key: texts come from strings "Action.&lt;Key&gt;" and "Action.&lt;Key&gt;.Hint".</summary>
    [Category("Localization"), DefaultValue("")]
    public string Key { get => _key; set => _key = value ?? ""; }

    [Category("Appearance"), DefaultValue("")]
    public string Text { get => _text; set => Set(ref _text, value ?? ""); }

    /// <summary>Hint shown on toolbar buttons; when empty the text without the mnemonic is used.</summary>
    [Category("Appearance"), DefaultValue("")]
    public string ToolTipText { get => _toolTipText; set => Set(ref _toolTipText, value ?? ""); }

    [Category("Appearance"), DefaultValue(AmcIcon.None)]
    public AmcIcon Icon { get => _icon; set => Set(ref _icon, value); }

    [Category("Behavior"), DefaultValue(Keys.None)]
    public Keys ShortcutKeys { get => _shortcutKeys; set => Set(ref _shortcutKeys, value); }

    [Category("Behavior"), DefaultValue(true)]
    public bool Enabled { get => _enabled; set => Set(ref _enabled, value); }

    [Category("Behavior"), DefaultValue(false)]
    public bool Checked { get => _checked; set => Set(ref _checked, value); }

    [Category("Behavior"), DefaultValue(true)]
    public bool Visible { get => _visible; set => Set(ref _visible, value); }

    /// <summary>Raised when the user invokes the action and it is enabled.</summary>
    [Category("Action")]
    public event EventHandler? Execute;

    /// <summary>Raised on idle so the handler can refresh Enabled/Checked (Delphi OnUpdate).</summary>
    [Category("Action")]
    public event EventHandler? Update;

    /// <summary>Raised whenever a visual property changes.</summary>
    [Browsable(false)]
    public event EventHandler? Changed;

    public void PerformExecute()
    {
        if (_enabled) Execute?.Invoke(this, EventArgs.Empty);
    }

    public void PerformUpdate() => Update?.Invoke(this, EventArgs.Empty);

    /// <summary>Applies texts of the current language; designer texts are the fallback.</summary>
    public void ApplyTranslation(ITranslator translator)
    {
        if (_key.Length == 0) return;
        _designText ??= _text;
        _designToolTip ??= _toolTipText;
        Text = translator.TryGet("Action." + _key, out var text) ? text : _designText;
        ToolTipText = translator.TryGet("Action." + _key + ".Hint", out var hint) ? hint : _designToolTip;
    }

    private void Set<T>(ref T field, T value)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
