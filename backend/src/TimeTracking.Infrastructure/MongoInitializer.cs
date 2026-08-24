using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TimeTracking.Application.Common;

namespace TimeTracking.Infrastructure;

/// <summary>На старте создаёт индексы и наполняет базу тестовыми данными (если пуста).</summary>
public class MongoInitializer : IHostedService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<MongoInitializer> _logger;

    public MongoInitializer(IServiceProvider services, ILogger<MongoInitializer> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        var db = _services.GetRequiredService<ITimeTrackingDb>();
        await IndexSetup.CreateIndexesAsync(db, ct);
        await DatabaseSeeder.SeedIfEmptyAsync(db, ct);
        _logger.LogInformation("MongoDB ready: индексы созданы, тестовые данные на месте.");
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
