using System.ComponentModel;
using GameCatalog.Core.Catalog;
using GameCatalog.Core.Models;

namespace GameCatalog.App;

/// <summary>Wypożyczenia — odpowiednik AMC loan.pas + loanhistory.pas.</summary>
public sealed class LoanForm : Form
{
    private readonly GameCatalogModel _model;
    private readonly List<LoanRecord> _loans;
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill };
    private readonly ComboBox _cbGame = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };
    private readonly TextBox _txtBorrower = new() { Width = 160 };

    public LoanForm(GameCatalogModel model, List<LoanRecord> loans)
    {
        _model = model;
        _loans = loans;
        Text = "Wypożyczenia";
        Width = 720; Height = 460;
        StartPosition = FormStartPosition.CenterParent;

        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.AutoGenerateColumns = false;
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "GameTitle", HeaderText = "Gra", Width = 220 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Borrower", HeaderText = "Pożyczono komu", Width = 150 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "LoanDate", HeaderText = "Data wypoż.", Width = 100 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ReturnDate", HeaderText = "Data zwrotu", Width = 100 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "State", HeaderText = "Stan", Width = 100 });

        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(8) };
        foreach (var g in _model.Games.OrderBy(g => g.Title))
            _cbGame.Items.Add(new GameItem(g.Number, g.DisplayTitle));
        if (_cbGame.Items.Count > 0) _cbGame.SelectedIndex = 0;
        var bBorrow = new Button { Text = "Pożycz", AutoSize = true };
        var bReturn = new Button { Text = "Zwróć zaznaczone", AutoSize = true };
        bBorrow.Click += (_, _) => Borrow();
        bReturn.Click += (_, _) => ReturnSelected();
        top.Controls.Add(new Label { Text = "Gra:", AutoSize = true });
        top.Controls.Add(_cbGame);
        top.Controls.Add(new Label { Text = "Komu:", AutoSize = true });
        top.Controls.Add(_txtBorrower);
        top.Controls.Add(bBorrow);
        top.Controls.Add(bReturn);

        var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 44, FlowDirection = FlowDirection.RightToLeft };
        var bClose = new Button { Text = "Zamknij", DialogResult = DialogResult.OK, Width = 90 };
        bottom.Controls.Add(bClose);

        Controls.Add(_grid);
        Controls.Add(top);
        Controls.Add(bottom);
        RefreshGrid();
    }

    private sealed record GameItem(int Number, string Title)
    {
        public override string ToString() => $"#{Number} {Title}";
    }

    private void RefreshGrid()
    {
        var ordered = _loans.OrderByDescending(l => l.LoanDate).ToList();
        _orderedRows = ordered;
        _grid.DataSource = new BindingList<object>(ordered
            .Select(l => new
            {
                Id = l.Id,
                GameTitle = string.IsNullOrWhiteSpace(l.GameTitle) ? $"#{l.GameNumber}" : l.GameTitle,
                Borrower = l.Borrower,
                LoanDate = l.LoanDate.ToString("yyyy-MM-dd"),
                ReturnDate = l.ReturnDate?.ToString("yyyy-MM-dd") ?? "–",
                State = l.IsReturned ? "zwrócona" : "wypożyczona",
            }).Cast<object>().ToList());
    }

    private List<LoanRecord> _orderedRows = new();

    private void Borrow()
    {
        if (_cbGame.SelectedItem is not GameItem gi) return;
        if (string.IsNullOrWhiteSpace(_txtBorrower.Text))
        {
            MessageBox.Show(this, "Podaj komu pożyczasz.", "Wypożyczenie");
            return;
        }
        var g = _model.Find(gi.Number);
        var rec = new LoanRecord
        {
            GameNumber = gi.Number,
            GameTitle = g?.Title ?? gi.Title,
            Borrower = _txtBorrower.Text.Trim(),
            LoanDate = DateTime.Today,
        };
        _loans.Add(rec);
        if (g is not null) g.Borrower = rec.Borrower;
        RefreshGrid();
    }

    private void ReturnSelected()
    {
        if (_grid.CurrentRow is null) return;
        int idx = _grid.CurrentRow.Index;
        if (idx < 0 || idx >= _orderedRows.Count) return;
        var rec = _orderedRows[idx];
        if (rec.IsReturned) return;
        rec.ReturnDate = DateTime.Today;
        var g = _model.Find(rec.GameNumber);
        if (g is not null && !_loans.Any(l => l.GameNumber == g.Number && !l.IsReturned))
            g.Borrower = "";
        RefreshGrid();
    }
}
