using MongoDB.Driver;
using TimeTracking.Domain;

namespace TimeTracking.Application.Common;

/// <summary>
/// Точка доступа к хранилищу для обработчиков.
/// </summary>
public interface ITimeTrackingDb
{
    IMongoClient Client { get; }

    IMongoCollection<Employee> Employees { get; }

    IMongoCollection<Project> Projects { get; }

    IMongoCollection<TimeEntry> TimeEntries { get; }

    IMongoCollection<ClosedPeriod> ClosedPeriods { get; }
}
