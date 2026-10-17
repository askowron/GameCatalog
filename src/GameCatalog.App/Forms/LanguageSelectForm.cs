using GameCatalog.Localization;

namespace GameCatalog.App.Forms;

/// <summary>Interface language selection (AMC languageselect.pas); shown at first start and from Tools → Language.</summary>
public partial class LanguageSelectForm : Form
{
    public LanguageSelectForm(IReadOnlyList<LanguageInfo> languages, string selectedCode)
    {
        InitializeComponent();
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

    public string? SelectedCode => lvLanguages.SelectedItems.Count > 0 ? (string)lvLanguages.SelectedItems[0].Tag! : null;
}
