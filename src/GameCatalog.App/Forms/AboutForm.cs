namespace GameCatalog.App;

public sealed class AboutForm : Form
{
    public AboutForm()
    {
        Text = "O programie";
        Width = 460; Height = 300;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; MinimizeBox = false;
        var label = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Text = "GameCatalog 1.0\n\nPort Ant Movie Catalog (Delphi, Antoine Potten)\nna C# .NET + WinForms + SQLite.\n\nPola filmowe przeportowano na growe:\nDirector→Developer, Producer→Wydawca,\nActors→Aktorzy głosowi, Length→Czas gry,\nExtras→DLC/dodatki, nowe: Platforma, Status.",
        };
        var b = new Button { Text = "OK", DialogResult = DialogResult.OK, Dock = DockStyle.Bottom, Height = 36 };
        Controls.Add(label);
        Controls.Add(b);
    }
}
