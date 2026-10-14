using GameCatalog.Core.Catalog;

namespace GameCatalog.Core.Tests;

public class NaturalStringComparerTests
{
    [Theory]
    [InlineData("a2", "a10", -1)]
    [InlineData("a10", "a2", 1)]
    [InlineData("A1", "a1", 0)]
    [InlineData("a01", "a1", 1)]
    [InlineData(null, "a", -1)]
    public void Compare(string? x, string? y, int expectedSign)
        => Assert.Equal(expectedSign, Math.Sign(NaturalStringComparer.Instance.Compare(x, y)));
}
