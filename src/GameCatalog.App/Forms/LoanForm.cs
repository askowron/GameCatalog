using System.ComponentModel;
using GameCatalog.App.Services;
using GameCatalog.Core.Catalog;
using GameCatalog.Core.Models;

namespace GameCatalog.App.Forms;

/// <summary>Loans with history (AMC loan.pas + loanhistory.pas).</summary>
public partial class LoanForm : Form
{
    private readonly GameList _model;
    private readonly List<LoanRecord> _loans;
    // Loans in grid row order.
    private List<LoanRecord> _orderedRows = new();

    public LoanForm(GameList model, List<LoanRecord> loans)
    {
        InitializeComponent();
        _model = model;
        _loans = loans;
        foreach (var g in _model.Games.OrderBy(g => g.Title))
            cbxGame.Items.Add(new GameItem(g.Number, g.DisplayTitle));
        if (cbxGame.Items.Count > 0) cbxGame.SelectedIndex = 0;
        btnBorrow.Click += (_, _) => Borrow();
        btnReturn.Click += (_, _) => ReturnSelected();
        RefreshGrid();
    }

    private sealed record GameItem(int Number, string Title)
    {
        public override string ToString() => $"#{Number} {Title}";
    }

    private void RefreshGrid()
    {
        var l = AppServices.Languages;
        string lent = l.Get("Loans.StatusLent"), returned = l.Get("Loans.StatusReturned");
        _orderedRows = _loans.OrderByDescending(r => r.LoanDate).ToList();
        grdLoans.DataSource = new BindingList<object>(_orderedRows
            .Select(r => new
            {
                Id = r.Id,
                GameTitle = string.IsNullOrWhiteSpace(r.GameTitle) ? $"#{r.GameNumber}" : r.GameTitle,
                Borrower = r.Borrower,
                LoanDate = r.LoanDate.ToString("yyyy-MM-dd"),
                ReturnDate = r.ReturnDate?.ToString("yyyy-MM-dd") ?? "–",
                State = r.IsReturned ? returned : lent,
            }).Cast<object>().ToList());
    }

    private void Borrow()
    {
        if (cbxGame.SelectedItem is not GameItem gi) return;
        if (string.IsNullOrWhiteSpace(txtBorrower.Text))
        {
            var l = AppServices.Languages;
            MessageBox.Show(this, l.Get("Loans.BorrowerRequired"), l.Get("Loans.BorrowTitle"));
            return;
        }
        var g = _model.Find(gi.Number);
        var rec = new LoanRecord
        {
            GameNumber = gi.Number,
            GameTitle = g?.Title ?? gi.Title,
            Borrower = txtBorrower.Text.Trim(),
            LoanDate = DateTime.Today,
        };
        _loans.Add(rec);
        if (g is not null) g.Borrower = rec.Borrower;
        RefreshGrid();
    }

    private void ReturnSelected()
    {
        if (grdLoans.CurrentRow is null) return;
        int idx = grdLoans.CurrentRow.Index;
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
