using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using TimeTracking.Application.Common;

namespace TimeTracking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string connectionString, string databaseName)
    {
        MongoConventions.Register();

        services.AddSingleton<IMongoClient>(_ => new MongoClient(MongoClientSettings.FromConnectionString(connectionString)));
        services.AddSingleton<ITimeTrackingDb>(sp =>
            new TimeTrackingDb(sp.GetRequiredService<IMongoClient>(), databaseName));

        services.AddHostedService<MongoInitializer>();

        return services;
    }
}
