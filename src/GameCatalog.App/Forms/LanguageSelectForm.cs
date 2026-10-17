using GameCatalog.App.Services;
using GameCatalog.Localization;

namespace GameCatalog.App.Forms;

/// <summary>Interface language selection (AMC languageselect.pas), opened from Tools → Language.</summary>
public partial class LanguageSelectForm : Form
{
    /// <param name="selectedCode">Stored preference; empty selects the "system language" entry.</param>
    public LanguageSelectForm(IReadOnlyList<LanguageInfo> languages, string selectedCode)
    {
        InitializeComponent();
        // First entry: follow the operating system language (stored as an empty code).
        var system = new ListViewItem(AppServices.Languages.Get("Language.System")) { Tag = "" };
        system.SubItems.Add("");
        lvLanguages.Items.Add(system);
        if (string.IsNullOrEmpty(selectedCode)) system.Selected = true;
        foreach (var language in languages)
        {
            var item = new ListViewItem(language.Name) { Tag = language.Code };
            item.SubItems.Add(language.Author);
            lvLanguages.Items.Add(item);
            if (language.Code.Equals(selectedCode, StringComparison.OrdinalIgnoreCase)) item.Selected = true;
        }
        lvLanguages.DoubleClick += (_, _) =>
        {
            DialogResult = DialogResult.OK;
            Close();
        };
    }

    /// <summary>Chosen language code; empty = system language; null when nothing is selected.</summary>
    public string? SelectedCode =>
        lvLanguages.Items.Cast<ListViewItem>().FirstOrDefault(i => i.Selected)?.Tag as string;
}
