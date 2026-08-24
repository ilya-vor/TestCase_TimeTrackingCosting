using TimeTracking.Application.Common;

namespace TimeTracking.Application.Rules;

/// <summary>Правило 6: часы — положительные, кратные 0,5, не больше 24 за одну запись.</summary>
public static class EntryHoursRule
{
    public const double MaxHoursPerEntry = 24;

    public static void Validate(double hours)
    {
        if (double.IsNaN(hours) || double.IsInfinity(hours) || hours <= 0)
            throw new BusinessRuleException(ErrorCodes.InvalidHours,
                "Часы должны быть положительным числом.");

        if (hours > MaxHoursPerEntry)
            throw new BusinessRuleException(ErrorCodes.InvalidHours,
                $"Часы одной записи не могут превышать {MaxHoursPerEntry:0}.");

        if (Math.Abs(hours % 0.5) > 1e-9)
            throw new BusinessRuleException(ErrorCodes.InvalidHours,
                "Часы должны быть кратны 0,5.");
    }
}
