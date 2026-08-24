using MongoDB.Driver;
using TimeTracking.Domain;

namespace TimeTracking.Application.Common;

/// <summary>
/// Единственная точка доступа к хранилищу для обработчиков.
/// Реализация — в Infrastructure (официальный драйвер MongoDB, без ORM).
/// </summary>
public interface ITimeTrackingDb
{
    IMongoClient Client { get; }

    IMongoCollection<Employee> Employees { get; }

    IMongoCollection<Project> Projects { get; }

    IMongoCollection<TimeEntry> TimeEntries { get; }

    IMongoCollection<ClosedPeriod> ClosedPeriods { get; }
}
