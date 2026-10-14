using GameCatalog.Core.Fields;

namespace GameCatalog.Core.Tests;

public class YesNoTests
{
    [Theory]
    [InlineData("1", true)]
    [InlineData("Tak", true)]
    [InlineData("tak", true)]
    [InlineData("true", true)]
    [InlineData("Yes", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("0", false)]
    [InlineData("Nie", false)]
    public void Parse_AcceptsLegacyPolishAndNeutralValues(string? value, bool expected)
        => Assert.Equal(expected, YesNo.Parse(value));
}
