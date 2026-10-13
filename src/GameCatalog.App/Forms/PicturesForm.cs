using GameCatalog.Core.Catalog;
using GameCatalog.Core.Models;

namespace GameCatalog.App;

/// <summary>Zarządzanie okładkami — przegląd gier z/bez okładki, wczytywanie, usuwanie, eksport.</summary>
public sealed class PicturesForm : Form
{
    private readonly GameCatalogModel _model;
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill };

    public PicturesForm(GameCatalogModel model)
    {
        _model = model;
        Text = "Zarządzanie obrazkami";
        Width = 520; Height = 420;
        StartPosition = FormStartPosition.CenterParent;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.AutoGenerateColumns = false;
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Number", HeaderText = "#", Width = 45 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Title", HeaderText = "Tytuł", Width = 260 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Has", HeaderText = "Okładka", Width = 80 });
        RefreshGrid();
        var bar = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 44, Padding = new Padding(8) };
        var bLoad = new Button { Text = "Wczytaj…", AutoSize = true };
        var bDel = new Button { Text = "Usuń", AutoSize = true };
        var bExp = new Button { Text = "Eksportuj wszystkie…", AutoSize = true };
        var bClose = new Button { Text = "Zamknij", DialogResult = DialogResult.OK, AutoSize = true };
        bLoad.Click += (_, _) => LoadForSelected();
        bDel.Click += (_, _) => DeleteForSelected();
        bExp.Click += (_, _) => ExportAll();
        bar.Controls.AddRange(new Control[] { bLoad, bDel, bExp, bClose });
        Controls.Add(_grid);
        Controls.Add(bar);
    }

    private Game? Selected()
    {
        if (_grid.CurrentRow?.DataBoundItem is DataBoundItem<Game> w) return w.Value;
        return null;
    }

    private void RefreshGrid()
    {
        _grid.DataSource = new System.ComponentModel.BindingList<object>(_model.Games
            .OrderBy(g => g.Number)
            .Select(g => (object)new DataBoundItem<Game>(g.Number, g.Title, g.HasCover ? "Tak" : "Nie", g))
            .ToList());
    }

    private void LoadForSelected()
    {
        var g = Selected();
        if (g is null) return;
        using var o = new OpenFileDialog { Filter = "Obrazy (*.jpg;*.jpeg;*.png;*.gif;*.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp" };
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
        using var f = new FolderBrowserDialog { Description = "Folder docelowy okładek" };
        if (f.ShowDialog(this) != DialogResult.OK) return;
        int n = 0;
        foreach (var g in _model.Games.Where(g => g.HasCover))
        {
            try { File.WriteAllBytes(Path.Combine(f.SelectedPath, $"okladka_{g.Number}.png"), g.CoverImage!); n++; }
            catch { }
        }
        MessageBox.Show(this, $"Wyeksportowano {n} okładek.", "Zarządzanie obrazkami");
        RefreshGrid();
    }

    private sealed record DataBoundItem<T>(int Number, string Title, string Has, T Value);
}
