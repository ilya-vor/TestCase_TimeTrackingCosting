using MongoDB.Driver;
using TimeTracking.Application.Common;
using TimeTracking.Domain;

namespace TimeTracking.Infrastructure;

/// <summary>
/// Индексы под используемые запросы. Создаются явно на старте приложения.
/// Обоснование — в NOTES.md.
/// </summary>
public static class IndexSetup
{
    public static async Task CreateIndexesAsync(ITimeTrackingDb db, CancellationToken ct)
    {
        var entries = db.TimeEntries;

        // Список за месяц и отчёт: выборка по диапазону дат.
        await entries.Indexes.CreateOneAsync(new CreateIndexModel<TimeEntry>(
            Builders<TimeEntry>.IndexKeys.Ascending(e => e.Date)), cancellationToken: ct);

        // Дневные тоталы и фильтр по сотруднику + месяцу (правила 2, 3 и список).
        await entries.Indexes.CreateOneAsync(new CreateIndexModel<TimeEntry>(
            Builders<TimeEntry>.IndexKeys.Ascending(e => e.EmployeeId).Ascending(e => e.Date)), cancellationToken: ct);

        // Фильтр списка по проекту + месяцу.
        await entries.Indexes.CreateOneAsync(new CreateIndexModel<TimeEntry>(
            Builders<TimeEntry>.IndexKeys.Ascending(e => e.ProjectId).Ascending(e => e.Date)), cancellationToken: ct);

        // Уникальность шифра проекта.
        await db.Projects.Indexes.CreateOneAsync(new CreateIndexModel<Project>(
            Builders<Project>.IndexKeys.Ascending(p => p.Code),
            new CreateIndexOptions { Unique = true }), cancellationToken: ct);

        // Уникальность пары «год, месяц» у закрытых периодов.
        await db.ClosedPeriods.Indexes.CreateOneAsync(new CreateIndexModel<ClosedPeriod>(
            Builders<ClosedPeriod>.IndexKeys.Ascending(p => p.Year).Ascending(p => p.Month),
            new CreateIndexOptions { Unique = true }), cancellationToken: ct);
    }
}
