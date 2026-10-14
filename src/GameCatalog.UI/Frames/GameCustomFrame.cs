using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;

namespace GameCatalog.UI.Frames;

/// <summary>Custom fields editor generated from the catalog's definitions (AMC framemoviecustom.pas).</summary>
public partial class GameCustomFrame : UserControl, IGameFrame
{
    private readonly Dictionary<string, Control> _editors = new(StringComparer.OrdinalIgnoreCase);
    private IReadOnlyList<CustomFieldDefinition> _definitions = Array.Empty<CustomFieldDefinition>();
    private Game? _game;
    private bool _loading;

    public GameCustomFrame()
    {
        InitializeComponent();
        Enabled = false;
    }

    public event EventHandler<FrameModifiedEventArgs>? Modified;

    public Control? EditorFor(string customTag) => _editors.GetValueOrDefault(customTag);

    public void SetDefinitions(IReadOnlyList<CustomFieldDefinition> definitions)
    {
        _definitions = definitions;
        tlpFields.SuspendLayout();
        foreach (Control c in tlpFields.Controls.Cast<Control>().ToList()) c.Dispose();
        tlpFields.Controls.Clear();
        tlpFields.RowStyles.Clear();
        tlpFields.RowCount = 0;
        _editors.Clear();
        foreach (var def in definitions)
        {
            var editor = CreateEditor(def);
            int row = tlpFields.RowCount++;
            tlpFields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpFields.Controls.Add(new Label { Text = def.Name + ":", AutoSize = true, Anchor = AnchorStyles.Left, Padding = new Padding(0, 5, 0, 0) }, 0, row);
            editor.Dock = DockStyle.Fill;
            editor.Margin = new Padding(0, 2, 0, 2);
            tlpFields.Controls.Add(editor, 1, row);
            _editors[def.Tag] = editor;
            string tag = def.Tag;
            if (editor is CheckBox box) box.CheckedChanged += (_, _) => Commit(tag);
            else editor.Leave += (_, _) => Commit(tag);
        }
        tlpFields.ResumeLayout();
        Bind(_game);
    }

    public void Bind(Game? game)
    {
        _loading = true;
        try
        {
            _game = game;
            Enabled = game is not null;
            if (game is null) return;
            foreach (var def in _definitions)
            {
                var value = game.CustomValues.TryGetValue(def.Tag, out var v) ? v : def.DefaultValue;
                switch (_editors[def.Tag])
                {
                    case CheckBox c: c.Checked = YesNo.Parse(value); break;
                    case NumericUpDown n:
                        // Stored invariant; the Polish-only version stored the current culture ("12,5").
                        bool parsed = decimal.TryParse(value, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var d)
                                      || decimal.TryParse(value, out d);
                        n.Value = Math.Clamp(parsed ? d : 0, n.Minimum, n.Maximum);
                        break;
                    case DateTimePicker p:
                        bool has = DateTime.TryParse(value, out var date);
                        p.Value = has ? date : DateTime.Today;
                        p.Checked = has;
                        break;
                    case var other: other.Text = value; break;
                }
            }
        }
        finally { _loading = false; }
    }

    public void Commit()
    {
        foreach (var def in _definitions) Commit(def.Tag);
    }

    private void Commit(string tag)
    {
        if (_loading || _game is null || !_editors.TryGetValue(tag, out var editor)) return;
        string value = editor switch
        {
            CheckBox c => c.Checked ? YesNo.StoredTrue : "",
            NumericUpDown n => n.Value.ToString(System.Globalization.CultureInfo.InvariantCulture),
            DateTimePicker p => p.Checked ? p.Value.ToString("yyyy-MM-dd") : "",
            _ => editor.Text,
        };
        string old = _game.CustomValues.TryGetValue(tag, out var o) ? o : "";
        bool same = editor is CheckBox ? YesNo.Parse(old) == (value.Length > 0) : old == value;
        if (same) return;
        GameFieldWriter.Set(_game, GameFieldWriter.CustomPrefix + tag, value);
        Modified?.Invoke(this, new FrameModifiedEventArgs(GameFieldWriter.CustomPrefix + tag));
    }

    private static Control CreateEditor(CustomFieldDefinition def) => def.FieldType switch
    {
        "Number" => new NumericUpDown { Minimum = -1_000_000, Maximum = 1_000_000, DecimalPlaces = 2 },
        "Date" => new DateTimePicker { ShowCheckBox = true, Format = DateTimePickerFormat.Short },
        "List" => ListEditor(def.ListValues),
        "YesNo" => new CheckBox { Text = def.Name, AutoSize = true },
        _ => new TextBox(),
    };

    private static ComboBox ListEditor(string listValues)
    {
        var box = new ComboBox { DropDownStyle = ComboBoxStyle.DropDown };
        box.Items.AddRange(listValues.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return box;
    }
}
