using FluentAssertions;
using TimeTracking.Application.Common;
using TimeTracking.Application.Rules;

namespace TimeTracking.UnitTests;

/// <summary>Правило 6: часы — положительные, кратные 0,5, не больше 24 за запись.</summary>
public class EntryHoursRuleTests
{
    [Fact]
    public void Zero_hours_is_rejected()
    {
        var act = () => EntryHoursRule.Validate(0);
        act.Should().Throw<BusinessRuleException>().Where(e => e.Code == ErrorCodes.InvalidHours);
    }

    [Fact]
    public void Negative_hours_is_rejected()
    {
        var act = () => EntryHoursRule.Validate(-2);
        act.Should().Throw<BusinessRuleException>().Where(e => e.Code == ErrorCodes.InvalidHours);
    }

    [Fact]
    public void Hours_not_multiple_of_half_are_rejected()
    {
        var act = () => EntryHoursRule.Validate(3.7);
        act.Should().Throw<BusinessRuleException>().Where(e => e.Code == ErrorCodes.InvalidHours);
    }

    [Fact]
    public void Hours_over_24_are_rejected()
    {
        var act = () => EntryHoursRule.Validate(24.5);
        act.Should().Throw<BusinessRuleException>().Where(e => e.Code == ErrorCodes.InvalidHours);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(8)]
    [InlineData(24)]
    public void Valid_hours_are_accepted(double hours)
    {
        EntryHoursRule.Validate(hours); // не бросает
    }
}
