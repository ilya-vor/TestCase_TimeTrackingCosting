using MongoDB.Driver;
using TimeTracking.Application.Common;
using TimeTracking.Domain;

namespace TimeTracking.Infrastructure;

/// <summary>
/// Идемпотентный сид: заполняет базу данными из раздела «Приёмочные проверки» задания
/// (только если база ещё пуста). Чтобы пересоздать данные — очистите БД и перезапустите.
/// </summary>
public static class DatabaseSeeder
{
    private static DateTime Utc(int year, int month, int day) =>
        new(year, month, day, 0, 0, 0, DateTimeKind.Utc);

    public static async Task SeedIfEmptyAsync(ITimeTrackingDb db, CancellationToken ct)
    {
        var existing = await db.Employees.CountDocumentsAsync(FilterDefinition<Employee>.Empty, cancellationToken: ct);
        if (existing > 0)
            return;

        const string ivanov = "emp-ivanov";
        const string petrova = "emp-petrova";
        const string p001 = "proj-p001";
        const string p002 = "proj-p002";

        var employees = new List<Employee>
        {
            new()
            {
                Id = ivanov,
                Name = "Иванов И. И.",
                Department = "Проектный",
                Rates = new List<Rate>
                {
                    new() { From = Utc(2026, 1, 1), Value = 500m },
                    new() { From = Utc(2026, 3, 1), Value = 600m }
                }
            },
            new()
            {
                Id = petrova,
                Name = "Петрова А. С.",
                Department = "Проектный",
                Rates = new List<Rate>
                {
                    new() { From = Utc(2026, 2, 1), Value = 700m }
                }
            }
        };

        var projects = new List<Project>
        {
            new()
            {
                Id = p001,
                Code = "П-001",
                Name = "Реконструкция цеха",
                Budget = 20_000m,
                Start = Utc(2026, 1, 1),
                End = Utc(2026, 3, 31)
            },
            new()
            {
                Id = p002,
                Code = "П-002",
                Name = "Инженерные сети",
                Budget = 5_000m,
                Start = Utc(2026, 3, 1),
                End = null
            }
        };

        TimeEntry Entry(string id, string employeeId, string projectId, DateTime date, double hours) => new()
        {
            Id = id,
            EmployeeId = employeeId,
            ProjectId = projectId,
            Date = date,
            Hours = hours,
            Comment = string.Empty,
            Version = 1,
            CreatedBy = "seed",
            CreatedAt = Utc(2026, 1, 1),
            UpdatedBy = "seed",
            UpdatedAt = Utc(2026, 1, 1)
        };

        var entries = new List<TimeEntry>
        {
            Entry("te-20-02-ivanov", ivanov, p001, Utc(2026, 2, 20), 8),
            Entry("te-05-03-ivanov", ivanov, p001, Utc(2026, 3, 5), 8),
            Entry("te-05-03-petrova", petrova, p001, Utc(2026, 3, 5), 4),
            Entry("te-06-03-petrova", petrova, p002, Utc(2026, 3, 6), 10)
        };

        await db.Employees.InsertManyAsync(employees, cancellationToken: ct);
        await db.Projects.InsertManyAsync(projects, cancellationToken: ct);
        await db.TimeEntries.InsertManyAsync(entries, cancellationToken: ct);
    }
}
