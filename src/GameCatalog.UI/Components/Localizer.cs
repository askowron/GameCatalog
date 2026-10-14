using System.ComponentModel;
using System.ComponentModel.Design;
using GameCatalog.Localization;

namespace GameCatalog.UI.Components;

/// <summary>
/// Translates a form or user control from the language files — counterpart of AMC
/// TAntTranslator.Translate(Form). Keys are "&lt;ControlName&gt;.&lt;Property&gt;" inside the section
/// named after the container type ("$this.Text" is the form caption). Designer texts are the
/// fallback. Nested user controls translate themselves with their own Localizer; tool strip
/// items bound to a <see cref="UIAction"/> take their texts from the action.
/// </summary>
[ToolboxItem(true)]
public sealed class Localizer : Component
{
    private readonly IContainer? _components;
    private readonly Dictionary<string, string> _designTexts = new(StringComparer.Ordinal);
    private ContainerControl? _container;
    private bool _subscribed;

    /// <summary>Active translator; set once by the application before the first form is created.</summary>
    public static ITranslator? Translator { get; set; }

    public Localizer() { }

    public Localizer(IContainer container) : this()
    {
        container.Add(this);
        _components = container;
    }

    /// <summary>The translated form or user control (filled in automatically by the Designer).</summary>
    [DefaultValue(null)]
    public ContainerControl? ContainerControl
    {
        get => _container;
        set
        {
            if (_container is not null) _container.HandleCreated -= OnHandleCreated;
            _container = value;
            if (_container is null || (Site?.DesignMode ?? false)) return;
            _container.HandleCreated += OnHandleCreated;
            Subscribe();
        }
    }

    /// <summary>Section name in the language file; empty = container type name.</summary>
    [DefaultValue("")]
    public string FormName { get; set; } = "";

    public override ISite? Site
    {
        get => base.Site;
        set
        {
            base.Site = value;
            // Like ErrorProvider: default the container to the root component being designed.
            if (value?.GetService(typeof(IDesignerHost)) is IDesignerHost { RootComponent: ContainerControl root })
                _container = root;
        }
    }

    private string Section => FormName.Length > 0 ? FormName : _container?.GetType().Name ?? "";

    /// <summary>Applies the current language to the container now.</summary>
    public void Apply()
    {
        if (_container is null) return;
        var t = Translator;
        foreach (var (key, get, set) in Targets())
        {
            if (!_designTexts.ContainsKey(key)) _designTexts[key] = get();
            set(t?.GetFormValue(Section, key) ?? _designTexts[key]);
        }
        if (t is not null && _components is not null)
            foreach (var action in _components.Components.OfType<UIAction>())
                action.ApplyTranslation(t);
    }

    /// <summary>Every translatable property with its designer text (for the translation template).</summary>
    public IReadOnlyList<FormKey> CollectKeys() =>
        Targets().Select(t => new FormKey(Section, t.Key, _designTexts.GetValueOrDefault(t.Key) ?? t.Get())).ToList();

    private IEnumerable<(string Key, Func<string> Get, Action<string> Set)> Targets()
    {
        if (_container is null) yield break;
        var root = _container;
        if (root is Form) yield return ("$this.Text", () => root.Text, v => root.Text = v);
        foreach (var target in ControlTargets(root)) yield return target;
        if (_components is null) yield break;
        foreach (var strip in _components.Components.OfType<ToolStrip>())   // context menus live in the tray
            foreach (var t in ItemTargets(strip.Items)) yield return t;
        foreach (var tip in _components.Components.OfType<ToolTip>())
            foreach (var c in Descendants(root))
            {
                var control = c;
                if (control.Name.Length > 0 && !string.IsNullOrEmpty(tip.GetToolTip(control)))
                    yield return (control.Name + ".ToolTip", () => tip.GetToolTip(control) ?? "", v => tip.SetToolTip(control, v));
            }
    }

    private static IEnumerable<Control> Descendants(Control parent)
    {
        foreach (Control c in parent.Controls)
        {
            yield return c;
            if (c is UserControl) continue;
            foreach (var d in Descendants(c)) yield return d;
        }
    }

    private static IEnumerable<(string, Func<string>, Action<string>)> ControlTargets(Control parent)
    {
        foreach (Control c in parent.Controls)
        {
            var control = c;
            if (control.Name.Length > 0 && control is ButtonBase or Label or GroupBox or TabPage)
                yield return (control.Name + ".Text", () => control.Text, v => control.Text = v);
            switch (control)
            {
                case ToolStrip strip:
                    foreach (var t in ItemTargets(strip.Items)) yield return t;
                    break;
                case ListView lv:
                    foreach (ColumnHeader col in lv.Columns)
                    {
                        var column = col;
                        if (!string.IsNullOrEmpty(column.Name))
                            yield return (column.Name + ".Text", () => column.Text, v => column.Text = v);
                    }
                    break;
                case DataGridView grid:
                    foreach (DataGridViewColumn col in grid.Columns)
                    {
                        var column = col;
                        if (column.Name.Length > 0)
                            yield return (column.Name + ".HeaderText", () => column.HeaderText, v => column.HeaderText = v);
                    }
                    break;
            }
            if (control is UserControl) continue;   // frames localize themselves
            foreach (var t in ControlTargets(control)) yield return t;
        }
    }

    private static IEnumerable<(string, Func<string>, Action<string>)> ItemTargets(ToolStripItemCollection items)
    {
        foreach (ToolStripItem i in items)
        {
            var item = i;
            bool translatable = !string.IsNullOrEmpty(item.Name) && ActionList.FindAction(item) is null &&
                                item is not (ToolStripTextBox or ToolStripComboBox or ToolStripSeparator);
            if (translatable)
            {
                yield return (item.Name + ".Text", () => item.Text ?? "", v => item.Text = v);
                if (!string.IsNullOrEmpty(item.ToolTipText) && item.ToolTipText != item.Text)
                    yield return (item.Name + ".ToolTipText", () => item.ToolTipText ?? "", v => item.ToolTipText = v);
            }
            if (item is ToolStripDropDownItem dd)
                foreach (var t in ItemTargets(dd.DropDownItems)) yield return t;
        }
    }

    private void OnHandleCreated(object? sender, EventArgs e)
    {
        if (_container is not null) _container.HandleCreated -= OnHandleCreated;
        Apply();
    }

    private void Subscribe()
    {
        if (_subscribed || Translator is null) return;
        Translator.LanguageChanged += OnLanguageChanged;
        _subscribed = true;
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        if (_container is { IsHandleCreated: true, InvokeRequired: true })
            _container.BeginInvoke(Apply);
        else
            Apply();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _subscribed && Translator is not null)
        {
            Translator.LanguageChanged -= OnLanguageChanged;
            _subscribed = false;
        }
        base.Dispose(disposing);
    }
}
