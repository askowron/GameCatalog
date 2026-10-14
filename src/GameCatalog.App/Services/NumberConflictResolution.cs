namespace GameCatalog.App.Services;

/// <summary>How to resolve a game number that is already taken — options of the AMC "Movie number" dialog.</summary>
public enum NumberConflictResolution
{
    /// <summary>Accept a duplicate (two records with the same number).</summary>
    AcceptDuplicate,
    /// <summary>Give the existing record another number (the next free one, or swap on change).</summary>
    RenumberExisting,
    /// <summary>Increase the new number until it is free.</summary>
    ShiftNew,
}
