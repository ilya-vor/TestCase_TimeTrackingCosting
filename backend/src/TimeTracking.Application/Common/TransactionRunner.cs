using MongoDB.Driver;

namespace TimeTracking.Application.Common;

public static class TransactionRunner
{
    /// <summary>
    /// Выполняет действие в транзакции MongoDB (требует replica set)
    /// </summary>
    public static async Task RunAsync(
        ITimeTrackingDb db, Func<IClientSessionHandle, CancellationToken, Task> action, CancellationToken ct)
    {
        using var session = await db.Client.StartSessionAsync(cancellationToken: ct);
        await session.WithTransactionAsync(
            async (s, t) =>
            {
                await action(s, t);
                return 1;
            },
            cancellationToken: ct);
    }
}
