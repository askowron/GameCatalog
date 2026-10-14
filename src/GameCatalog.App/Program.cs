namespace GameCatalog.App;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        // Temporary until Task 6 wires the language files: keep the Polish UI captions.
        Core.Fields.CatalogLabels.Current = new PolishCatalogLabels();
        Application.Run(new MainForm());
    }
}
