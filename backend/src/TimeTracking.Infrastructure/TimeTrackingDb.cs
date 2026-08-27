using MongoDB.Driver;
using TimeTracking.Application.Common;
using TimeTracking.Domain;

namespace TimeTracking.Infrastructure;

public class TimeTrackingDb(IMongoClient client, string databaseName) : ITimeTrackingDb
{
    private readonly IMongoDatabase _database = client.GetDatabase(databaseName);

    public IMongoClient Client => client;

    public IMongoCollection<Employee> Employees => _database.GetCollection<Employee>("employees");

    public IMongoCollection<Project> Projects => _database.GetCollection<Project>("projects");

    public IMongoCollection<TimeEntry> TimeEntries => _database.GetCollection<TimeEntry>("time_entries");

    public IMongoCollection<DayCounter> DayCounters => _database.GetCollection<DayCounter>("day_counters");

    public IMongoCollection<ClosedPeriod> ClosedPeriods => _database.GetCollection<ClosedPeriod>("closed_periods");
}
