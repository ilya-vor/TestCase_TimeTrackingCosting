using FluentAssertions;
using TimeTracking.Application.Common;
using TimeTracking.Application.Rules;

namespace TimeTracking.UnitTests;

/// <summary>
/// Правило 2 и 3: не больше 24 часов за календарный день по всем проектам,
/// день с суммой больше 12 часов — переработка
/// </summary>
public class DayHoursLimitRuleTests
{
    [Fact]
    public void Total_over_24_throws_with_readable_message()
    {
        var act = () => DayHoursLimitRule.ValidateDayTotal(20, 6);
        act.Should().Throw<BusinessRuleException>()
            .Where(e => e.Code == ErrorCodes.DayHoursLimitExceeded)
            .WithMessage("*24*")
            .WithMessage("*26*");
    }

    [Fact]
    public void Total_exactly_24_is_allowed()
    {
        DayHoursLimitRule.ValidateDayTotal(20, 4);
    }

    [Fact]
    public void Total_below_24_is_allowed()
    {
        DayHoursLimitRule.ValidateDayTotal(12, 8);
    }

    [Theory]
    [InlineData(12, false)]
    [InlineData(12.5, true)]
    [InlineData(24, true)]
    [InlineData(0, false)]
    public void IsOvertime_threshold_is_more_than_12(double dayTotal, bool expected)
    {
        DayHoursLimitRule.IsOvertime(dayTotal).Should().Be(expected);
    }
}
