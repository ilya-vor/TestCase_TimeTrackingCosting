using MongoDB.Driver;
using TimeTracking.Application.Common;
using TimeTracking.Application.Rules;
using TimeTracking.Domain;

namespace TimeTracking.Application.Entries;

/// <summary>
/// Проекция записи табеля в строку ответа: резолв ставки на дату, стоимость, флаг переработки.
/// </summary>
internal static class TimeEntryRowProjector
{
    /// <summary>
    /// Сборка строки по уже загруженным данным (без N+1). Если ставка не передана, резолвится по сотруднику.
    /// </summary>
    public static TimeEntryRow Build(TimeEntry entry, Employee? employee, Project? project, double dayTotal, decimal? rate = null)
    {
        var effectiveRate = rate ?? (employee is null ? 0m : EmployeeRates.EffectiveOn(employee.Rates, entry.Date)?.Value ?? 0m);
        var amount = Money.Round((decimal)entry.Hours * effectiveRate);

        return new TimeEntryRow
        {
            Id = entry.Id,
            EmployeeId = entry.EmployeeId,
            EmployeeName = employee?.Name ?? "—",
            ProjectId = entry.ProjectId,
            ProjectCode = project?.Code ?? "—",
            Date = entry.Date,
            Hours = entry.Hours,
            Rate = effectiveRate,
            Amount = amount,
            Comment = entry.Comment,
            Overtime = DayHoursLimitRule.IsOvertime(dayTotal),
            Version = entry.Version
        };
    }

    /// <summary>
    /// Сборка строки с подгрузкой сотрудника, проекта и дневного тотала (create/update).
    /// </summary>
    public static async Task<TimeEntryRow> BuildWithLookupsAsync(
        ITimeTrackingDb db, IClientSessionHandle? session, TimeEntry entry, CancellationToken ct, decimal? rate = null)
    {
        var employee = session is null
            ? await db.Employees.Find(e => e.Id == entry.EmployeeId).FirstOrDefaultAsync(ct)
            : await db.Employees.Find(session, e => e.Id == entry.EmployeeId).FirstOrDefaultAsync(ct);
        var project = session is null
            ? await db.Projects.Find(p => p.Id == entry.ProjectId).FirstOrDefaultAsync(ct)
            : await db.Projects.Find(session, p => p.Id == entry.ProjectId).FirstOrDefaultAsync(ct);
        var dayTotal = await DayHoursAggregator.SumForDayAsync(db, session, entry.EmployeeId, entry.Date, ct);
        return Build(entry, employee, project, dayTotal, rate);
    }
}
