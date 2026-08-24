using TimeTracking.Application.Common;
using TimeTracking.Domain;

namespace TimeTracking.Application.Rules;

public static class EmployeeRates
{
    /// <summary>
    /// Ставка, действующая на дату: последняя по дате начала, не позже указанной даты.
    /// Правило 1 задания: «ставка, действовавшая на дату записи», смена ставок задним числом учитывается.
    /// </summary>
    public static Rate? EffectiveOn(IEnumerable<Rate> rates, DateTime date)
        => rates.Where(r => r.From.Date <= date.Date)
                .OrderByDescending(r => r.From)
                .FirstOrDefault();

    /// <summary>Бросает ошибку, если на дату у сотрудника нет ни одной ставки.</summary>
    public static decimal RequireOn(IEnumerable<Rate> rates, DateTime date, string employeeName)
    {
        var rate = EffectiveOn(rates, date);
        if (rate is null)
            throw new BusinessRuleException(ErrorCodes.RateNotFoundOnDate,
                $"У сотрудника «{employeeName}» на {date:dd.MM.yyyy} ещё нет ни одной ставки, запись создать нельзя.");
        return rate.Value;
    }
}
