using FluentAssertions;
using TimeTracking.Application.Common;
using TimeTracking.Application.Rules;

namespace TimeTracking.UnitTests;

/// <summary>Правило 5: дата записи попадает в период проекта.</summary>
public class ProjectPeriodRuleTests
{
    private static readonly DateTime Start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime End = new(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Date_inside_period_is_allowed()
    {
        ProjectPeriodRule.ThrowIfOutside(new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc), Start, End);
    }

    [Fact]
    public void Date_equal_to_start_is_allowed()
    {
        ProjectPeriodRule.ThrowIfOutside(Start, Start, End);
    }

    [Fact]
    public void Date_equal_to_end_is_allowed()
    {
        ProjectPeriodRule.ThrowIfOutside(End, Start, End);
    }

    [Fact]
    public void Date_before_start_throws()
    {
        var act = () => ProjectPeriodRule.ThrowIfOutside(new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc), Start, End);
        act.Should().Throw<BusinessRuleException>()
            .Where(e => e.Code == ErrorCodes.ProjectPeriodViolation);
    }

    [Fact]
    public void Date_after_end_throws()
    {
        var act = () => ProjectPeriodRule.ThrowIfOutside(new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc), Start, End);
        act.Should().Throw<BusinessRuleException>()
            .Where(e => e.Code == ErrorCodes.ProjectPeriodViolation);
    }

    [Fact]
    public void Open_ended_project_has_no_upper_bound()
    {
        DateTime? noEnd = null;
        ProjectPeriodRule.ThrowIfOutside(new DateTime(2030, 6, 1, 0, 0, 0, DateTimeKind.Utc), Start, noEnd);
    }

    [Fact]
    public void Open_ended_project_still_rejects_before_start()
    {
        DateTime? noEnd = null;
        var act = () => ProjectPeriodRule.ThrowIfOutside(new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc), Start, noEnd);
        act.Should().Throw<BusinessRuleException>()
            .Where(e => e.Code == ErrorCodes.ProjectPeriodViolation);
    }
}
