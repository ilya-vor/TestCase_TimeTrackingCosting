using TimeTracking.Application.Common;

namespace TimeTracking.Application.Rules;

/// <summary>
/// Правило 5: дата записи попадает в период проекта (не раньше начала, не позже окончания, если задано).
/// </summary>
public static class ProjectPeriodRule
{
    public static void ThrowIfOutside(DateTime date, DateTime start, DateTime? end)
    {
        var day = date.Date;
        var range = end.HasValue
            ? $"{start.Date:dd.MM.yyyy} – {end.Value.Date:dd.MM.yyyy}"
            : $"с {start.Date:dd.MM.yyyy} (без даты окончания)";

        if (day < start.Date || (end.HasValue && day > end.Value.Date))
            throw new BusinessRuleException(ErrorCodes.ProjectPeriodViolation,
                $"Дата записи вне периода проекта ({range}).");
    }
}
