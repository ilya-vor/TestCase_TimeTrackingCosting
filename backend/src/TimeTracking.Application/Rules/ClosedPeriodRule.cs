using TimeTracking.Application.Common;
using TimeTracking.Domain;

namespace TimeTracking.Application.Rules;

/// <summary>Правило 4: в закрытом периоде записи нельзя создавать, изменять и удалять.</summary>
public static class ClosedPeriodRule
{
    public static bool IsClosed(IEnumerable<ClosedPeriod> periods, DateTime date)
        => periods.Any(p => p.Year == date.Year && p.Month == date.Month);

    public static void ThrowIfClosed(IEnumerable<ClosedPeriod> periods, DateTime date)
    {
        if (IsClosed(periods, date))
            throw new BusinessRuleException(ErrorCodes.PeriodClosed,
                $"Период {date:MM.yyyy} закрыт для редактирования.", HttpStatus.Conflict);
    }
}
