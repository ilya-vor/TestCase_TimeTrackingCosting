using TimeTracking.Application.Common;

namespace TimeTracking.Application.Rules;

/// <summary>
/// Правила 2 и 3: за один календарный день по всем проектам у сотрудника не больше 24 часов,
/// день с суммой больше 12 часов помечается как переработка.
/// </summary>
public static class DayHoursLimitRule
{
    public const double MaxHoursPerDay = 24;
    public const double OvertimeThreshold = 12;

    public static void ValidateDayTotal(double currentTotal, double additionalHours)
    {
        var total = currentTotal + additionalHours;
        if (total > MaxHoursPerDay)
            throw new BusinessRuleException(ErrorCodes.DayHoursLimitExceeded,
                $"За один календарный день у сотрудника не может быть больше {MaxHoursPerDay:0} часов. " +
                $"За день уже учтено {currentTotal:0.#} ч, добавление {additionalHours:0.#} ч даст {total:0.#} ч.");
    }

    public static bool IsOvertime(double dayTotal) => dayTotal > OvertimeThreshold;
}
