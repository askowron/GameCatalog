namespace GameCatalog.Core.Models;

/// <summary>Game loan — counterpart of AMC Loan + LoanHistory.</summary>
public sealed class LoanRecord
{
    public int Id { get; set; }
    public int GameNumber { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public string Borrower { get; set; } = string.Empty;
    public DateTime LoanDate { get; set; } = DateTime.Today;
    public DateTime? ReturnDate { get; set; }
    public bool IsReturned => ReturnDate.HasValue;
}
