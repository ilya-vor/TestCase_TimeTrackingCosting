using FluentAssertions;
using TimeTracking.Application.Common;

namespace TimeTracking.UnitTests;

/// <summary>Правило 7: деньги — decimal, округление до копеек.</summary>
public class MoneyRoundingTests
{
    [Theory]
    [InlineData("12.345", "12.35")]
    [InlineData("12.344", "12.34")]
    [InlineData("1.005", "1.01")]
    [InlineData("1.004", "1.00")]
    [InlineData("0.999", "1.00")]
    [InlineData("0", "0.00")]
    public void Round_to_kopecks(string input, string expected)
    {
        Money.Round(decimal.Parse(input, System.Globalization.CultureInfo.InvariantCulture))
            .Should().Be(decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Round_uses_half_away_from_zero()
    {
        Money.Round(-1.005m).Should().Be(-1.01m);
    }
}
