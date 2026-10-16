using GameCatalog.App.Services;

namespace GameCatalog.App.Forms;

/// <summary>Asks for a game number (go to number / change number).</summary>
public partial class NumberDialog : Form
{
    public int Value { get; private set; }

    public NumberDialog(int current, int max)
    {
        InitializeComponent();
        // The caption comes from strings, not from the form section, so callers can replace it.
        Text = AppServices.Languages.Get("Number.GoToTitle");
        numNumber.Maximum = Math.Max(1, max);
        numNumber.Value = Math.Clamp(current, 1, Math.Max(1, max));
        btnOk.Click += (_, _) => Value = (int)numNumber.Value;
    }
}
