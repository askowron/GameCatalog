using GameCatalog.App.Services;
using GameCatalog.Core.Models;

namespace GameCatalog.App.Tests;

public class NumberingServiceTests
{
    private static (CatalogSession Session, NumberingService Numbering) Create()
    {
        var s = new CatalogSession(new PrefsStore());
        s.Games.Add(new Game { Number = 1, Title = "A", Extras = { new GameExtra { Title = "A-DLC", GameNumber = 1 } } });
        s.Games.Add(new Game { Number = 2, Title = "B" });
        s.Games.Add(new Game { Number = 5, Title = "C" });
        s.Loans.Add(new LoanRecord { GameNumber = 1, Borrower = "Ola" });
        s.Loans.Add(new LoanRecord { GameNumber = 2, Borrower = "Jan" });
        return (s, new NumberingService(s));
    }

    [Fact]
    public void ShiftUntilFree_SkipsTakenNumbers()
        => Assert.Equal(3, Create().Numbering.ShiftUntilFree(1));

    [Fact]
    public void ResolveNewNumber_Variants()
    {
        var (s, n) = Create();
        Assert.Equal(4, n.ResolveNewNumber(4, NumberConflictResolution.ShiftNew));
        Assert.Equal(2, n.ResolveNewNumber(2, NumberConflictResolution.AcceptDuplicate));
        Assert.Equal(3, n.ResolveNewNumber(2, NumberConflictResolution.ShiftNew));
        Assert.Equal(2, n.ResolveNewNumber(2, NumberConflictResolution.RenumberExisting));
        Assert.Null(s.Games.Find(2));                         // "B" moved away
        Assert.Equal(6, s.Games.Games.Single(g => g.Title == "B").Number);
        Assert.Equal(6, s.Loans.Single(l => l.Borrower == "Jan").GameNumber);
    }

    [Fact]
    public void ChangeNumber_RenumberExisting_SwapsGamesLoansAndExtras()
    {
        var (s, n) = Create();
        var a = s.Games.Find(1)!;

        Assert.Equal(2, n.ChangeNumber(a, 2, NumberConflictResolution.RenumberExisting));

        Assert.Equal("A", s.Games.Find(2)!.Title);
        Assert.Equal("B", s.Games.Find(1)!.Title);
        Assert.Equal(2, a.Extras[0].GameNumber);
        Assert.Equal(2, s.Loans.Single(l => l.Borrower == "Ola").GameNumber);
        Assert.Equal(1, s.Loans.Single(l => l.Borrower == "Jan").GameNumber);
        Assert.True(s.IsDirty);
    }

    [Fact]
    public void ChangeNumber_ShiftNew_UsesNextFreeNumber()
    {
        var (s, n) = Create();
        Assert.Equal(3, n.ChangeNumber(s.Games.Find(1)!, 2, NumberConflictResolution.ShiftNew));
    }

    [Fact]
    public void ChangeNumber_AcceptDuplicate_LeavesOtherGameAlone()
    {
        var (s, n) = Create();
        n.ChangeNumber(s.Games.Find(1)!, 2, NumberConflictResolution.AcceptDuplicate);
        Assert.Equal(2, s.Games.Games.Count(g => g.Number == 2));
        Assert.Equal(2, s.Loans.Single(l => l.Borrower == "Jan").GameNumber);
    }

    [Fact]
    public void ChangeNumber_SameNumber_ReturnsNull()
    {
        var (s, n) = Create();
        Assert.Null(n.ChangeNumber(s.Games.Find(1)!, 1, NumberConflictResolution.RenumberExisting));
        Assert.Equal(0, s.Undo.Count);
    }

    [Fact]
    public void Undo_RestoresNumbersLoansAndExtras()
    {
        var (s, n) = Create();
        n.ChangeNumber(s.Games.Find(1)!, 2, NumberConflictResolution.RenumberExisting);
        Assert.True(s.Undo.TryUndo());
        Assert.Equal("A", s.Games.Find(1)!.Title);
        Assert.Equal(1, s.Games.Find(1)!.Extras[0].GameNumber);
        Assert.Equal(1, s.Loans.Single(l => l.Borrower == "Ola").GameNumber);
    }

    [Fact]
    public void RenumberSequential_FollowsViewOrder()
    {
        var (s, n) = Create();
        var view = s.Games.Games.OrderByDescending(g => g.Title).ToList();   // C, B, A
        n.RenumberSequential(view);
        Assert.Equal(new[] { "C", "B", "A" }, s.Games.Games.OrderBy(g => g.Number).Select(g => g.Title));
        Assert.Equal(3, s.Loans.Single(l => l.Borrower == "Ola").GameNumber);
        Assert.True(s.Undo.TryUndo());
        Assert.Equal("A", s.Games.Find(1)!.Title);
    }
}
