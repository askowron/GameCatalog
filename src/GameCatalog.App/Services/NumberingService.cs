using GameCatalog.Core.Models;

namespace GameCatalog.App.Services;

/// <summary>
/// Game number changes (AMC number.pas / renumber.pas): keeps loans and extras pointing at the
/// right game and records one undo step per operation.
/// </summary>
public sealed class NumberingService(CatalogSession session)
{
    public int ShiftUntilFree(int start)
    {
        int n = start;
        while (session.Games.Find(n) is not null) n++;
        return n;
    }

    /// <summary>Number to give a new game whose desired number may already be taken.</summary>
    public int ResolveNewNumber(int desired, NumberConflictResolution choice)
    {
        var existing = session.Games.Find(desired);
        if (existing is null) return desired;
        switch (choice)
        {
            case NumberConflictResolution.ShiftNew:
                return ShiftUntilFree(desired);
            case NumberConflictResolution.RenumberExisting:
                var snapshot = Snapshot();
                Reassign(existing, ShiftUntilFree(session.Games.NextNumber()));
                Commit(snapshot);
                return desired;
            default:
                return desired;
        }
    }

    /// <returns>The number actually assigned, or null when nothing changed.</returns>
    public int? ChangeNumber(Game game, int newNumber, NumberConflictResolution conflict)
    {
        if (newNumber == game.Number) return null;
        var other = session.Games.Find(newNumber);
        var snapshot = Snapshot();
        int oldNumber = game.Number;
        if (other is null || conflict == NumberConflictResolution.AcceptDuplicate)
        {
            Reassign(game, newNumber);
        }
        else if (conflict == NumberConflictResolution.ShiftNew)
        {
            newNumber = ShiftUntilFree(newNumber);
            Reassign(game, newNumber);
        }
        else
        {
            // Swap: loans are remapped in one pass so the two sets do not collide.
            foreach (var loan in session.Loans)
            {
                if (loan.GameNumber == oldNumber) loan.GameNumber = newNumber;
                else if (loan.GameNumber == newNumber) loan.GameNumber = oldNumber;
            }
            SetNumber(other, oldNumber);
            SetNumber(game, newNumber);
        }
        Commit(snapshot);
        return newNumber;
    }

    /// <summary>Numbers the given games 1..n in the given order (Tools → Renumber).</summary>
    public void RenumberSequential(IReadOnlyList<Game> view)
    {
        if (view.Count == 0) return;
        var snapshot = Snapshot();
        var newByOld = new Dictionary<int, int>();
        int n = 1;
        foreach (var g in view)
        {
            newByOld.TryAdd(g.Number, n);
            SetNumber(g, n++);
        }
        foreach (var loan in session.Loans)
            if (newByOld.TryGetValue(loan.GameNumber, out var nn)) loan.GameNumber = nn;
        Commit(snapshot);
    }

    private void Reassign(Game game, int number)
    {
        foreach (var loan in session.Loans)
            if (loan.GameNumber == game.Number) loan.GameNumber = number;
        SetNumber(game, number);
    }

    private static void SetNumber(Game game, int number)
    {
        game.Number = number;
        foreach (var e in game.Extras) e.GameNumber = number;
    }

    private sealed record NumberSnapshot(Dictionary<Game, int> Games, Dictionary<LoanRecord, int> Loans);

    private NumberSnapshot Snapshot()
    {
        // Reference equality: duplicates by number must stay distinct entries.
        var games = new Dictionary<Game, int>(ReferenceEqualityComparer.Instance);
        foreach (var g in session.Games.Games) games[g] = g.Number;
        var loans = new Dictionary<LoanRecord, int>(ReferenceEqualityComparer.Instance);
        foreach (var l in session.Loans) loans[l] = l.GameNumber;
        return new NumberSnapshot(games, loans);
    }

    private void Commit(NumberSnapshot snapshot)
    {
        session.Games.ReplaceAll(session.Games.Games.OrderBy(g => g.Number));
        session.Undo.Push(() =>
        {
            foreach (var (g, number) in snapshot.Games) SetNumber(g, number);
            foreach (var (l, number) in snapshot.Loans) l.GameNumber = number;
            session.Games.ReplaceAll(session.Games.Games.OrderBy(g => g.Number));
            session.MarkDirty();
        });
        session.MarkDirty();
    }
}
