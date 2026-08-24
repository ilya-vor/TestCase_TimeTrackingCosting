using MongoDB.Driver;
using TimeTracking.Application.Common;
using TimeTracking.Domain;

namespace TimeTracking.Infrastructure;

public class TimeTrackingDb : ITimeTrackingDb
{
    private readonly IMongoDatabase _database;

    public TimeTrackingDb(IMongoClient client, string databaseName)
    {
        Client = client;
        _database = client.GetDatabase(databaseName);
    }

    public IMongoClient Client { get; }

    public IMongoCollection<Employee> Employees => _database.GetCollection<Employee>("employees");

    public IMongoCollection<Project> Projects => _database.GetCollection<Project>("projects");

    public IMongoCollection<TimeEntry> TimeEntries => _database.GetCollection<TimeEntry>("time_entries");

    public IMongoCollection<ClosedPeriod> ClosedPeriods => _database.GetCollection<ClosedPeriod>("closed_periods");
}
