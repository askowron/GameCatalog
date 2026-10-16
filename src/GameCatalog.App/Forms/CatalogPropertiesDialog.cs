using GameCatalog.App.Services;

namespace GameCatalog.App.Forms;

/// <summary>Catalog properties (AMC FileProperties).</summary>
public partial class CatalogPropertiesDialog : Form
{
    public CatalogPropertiesDialog(string dbPath, int games, int extras, double hours, int loans)
    {
        InitializeComponent();
        var fi = new FileInfo(dbPath);
        string size = fi.Exists ? $"{fi.Length / 1024.0:0.#} KB" : "–";
        lblSummary.Text = AppServices.Languages.Get("Properties.Summary", dbPath, size, games, extras, hours, loans);
    }
}
