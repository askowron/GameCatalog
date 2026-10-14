namespace GameCatalog.Core.Catalog;

/// <summary>Natural comparison like AMC: digit runs compare by numeric value, not lexically.</summary>
public sealed class NaturalStringComparer : IComparer<string?>
{
    public static readonly NaturalStringComparer Instance = new();

    public int Compare(string? x, string? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x is null) return -1;
        if (y is null) return 1;
        int i = 0, j = 0;
        while (i < x.Length && j < y.Length)
        {
            bool dx = char.IsDigit(x[i]), dy = char.IsDigit(y[j]);
            if (dx && dy)
            {
                int sx = i, sy = j;
                while (i < x.Length && char.IsDigit(x[i])) i++;
                while (j < y.Length && char.IsDigit(y[j])) j++;
                string nx = x.Substring(sx, i - sx).TrimStart('0');
                string ny = y.Substring(sy, j - sy).TrimStart('0');
                int c = nx.Length.CompareTo(ny.Length);
                if (c != 0) return c;
                c = string.Compare(nx, ny, StringComparison.Ordinal);
                if (c != 0) return c;
                // Equal values — fewer leading zeros sorts first (stable).
                c = (i - sx).CompareTo(j - sy);
                if (c != 0) return c;
            }
            else
            {
                int c = char.ToUpperInvariant(x[i]).CompareTo(char.ToUpperInvariant(y[j]));
                if (c != 0) return c;
                i++; j++;
            }
        }
        return (x.Length - i).CompareTo(y.Length - j);
    }
}
