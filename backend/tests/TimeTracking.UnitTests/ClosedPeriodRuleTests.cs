using FluentAssertions;
using TimeTracking.Application.Common;
using TimeTracking.Application.Rules;
using TimeTracking.Domain;

namespace TimeTracking.UnitTests;

/// <summary>
/// Правило 4: в закрытом периоде записи нельзя создавать, изменять и удалять
/// </summary>
public class ClosedPeriodRuleTests
{
    private static readonly DateTime Feb2026 = new(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc);

    private static readonly List<ClosedPeriod> ClosedFeb2026 = new()
    {
        new ClosedPeriod { Year = 2026, Month = 2 }
    };

    [Fact]
    public void Closed_period_throws_conflict()
    {
        var act = () => ClosedPeriodRule.ThrowIfClosed(ClosedFeb2026, Feb2026);
        act.Should().Throw<BusinessRuleException>()
            .Where(e => e.Code == ErrorCodes.PeriodClosed)
            .Where(e => e.StatusCode == HttpStatus.Conflict)
            .WithMessage("*02.2026*");
    }

    [Fact]
    public void Open_period_is_allowed()
    {
        ClosedPeriodRule.ThrowIfClosed(ClosedFeb2026, new DateTime(2026, 3, 5, 0, 0, 0, DateTimeKind.Utc));
    }

    [Theory]
    [InlineData(2026, 1)]
    [InlineData(2026, 3)]
    [InlineData(2027, 2)]
    public void IsClosed_checks_year_and_month(int year, int month)
    {
        ClosedPeriodRule.IsClosed(ClosedFeb2026, new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc))
            .Should().BeFalse();
    }

    [Fact]
    public void IsClosed_matches_same_month_of_same_year()
    {
        ClosedPeriodRule.IsClosed(ClosedFeb2026, Feb2026).Should().BeTrue();
    }
}
