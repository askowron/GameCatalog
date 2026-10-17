using GameCatalog.App.Services;
using GameCatalog.Core.Catalog;
using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;

namespace GameCatalog.App.Forms;

/// <summary>Cover management: games with/without a cover, load, delete, export (AMC pictures manager).</summary>
public partial class PicturesForm : Form
{
    private readonly GameList _model;

    public PicturesForm(GameList model)
    {
        InitializeComponent();
        _model = model;
        RefreshGrid();
        btnLoad.Click += (_, _) => LoadForSelected();
        btnDelete.Click += (_, _) => DeleteForSelected();
        btnExportAll.Click += (_, _) => ExportAll();
    }

    private Game? Selected() =>
        grdCovers.CurrentRow?.DataBoundItem is DataBoundItem<Game> w ? w.Value : null;

    private void RefreshGrid()
    {
        var labels = CatalogLabels.Current;
        grdCovers.DataSource = new System.ComponentModel.BindingList<object>(_model.Games
            .OrderBy(g => g.Number)
            .Select(g => (object)new DataBoundItem<Game>(g.Number, g.Title, g.HasCover ? labels.Yes : labels.No, g))
            .ToList());
    }

    private void LoadForSelected()
    {
        var g = Selected();
        if (g is null) return;
        using var o = new OpenFileDialog { Filter = AppServices.Languages.Get("Filter.Images") };
        if (o.ShowDialog(this) != DialogResult.OK) return;
        g.CoverImage = File.ReadAllBytes(o.FileName);
        RefreshGrid();
    }

    private void DeleteForSelected()
    {
        var g = Selected();
        if (g?.HasCover != true) return;
        g.CoverImage = null;
        RefreshGrid();
    }

    private void ExportAll()
    {
        var l = AppServices.Languages;
        using var f = new FolderBrowserDialog { Description = l.Get("Pictures.SelectFolder") };
        if (f.ShowDialog(this) != DialogResult.OK) return;
        int n = 0;
        foreach (var g in _model.Games.Where(g => g.HasCover))
        {
            try { File.WriteAllBytes(Path.Combine(f.SelectedPath, l.Get("File.CoverName", g.Number)), g.CoverImage!); n++; }
            catch { }
        }
        MessageBox.Show(this, l.Get("Pictures.Exported", n), Text);
        RefreshGrid();
    }

    private sealed record DataBoundItem<T>(int Number, string Title, string Has, T Value);
}
