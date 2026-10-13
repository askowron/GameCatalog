using GameCatalog.Core.Catalog;
using GameCatalog.Core.Models;

namespace GameCatalog.App;

/// <summary>Odczyt wartości pól gry po tagu — używane przez szukanie, sortowanie, widok wszystkich pól.</summary>
public static class FieldValues
{
    public static string Get(Game g, string tag) => GameFieldValues.Get(g, tag);

    /// <summary>Pola do szukania/sortowania (jak cbxField w AMC).</summary>
    public static IReadOnlyList<(string Tag, string Name)> SearchableFields() =>
        GameFields.All.Where(f => f.Tag is not ("DisplayTitle" or "NbExtras"))
            .Select(f => (f.Tag, f.DisplayName)).ToList();
}
