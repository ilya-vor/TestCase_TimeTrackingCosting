using MongoDB.Driver;

namespace TimeTracking.Application.Common;

/// <summary>
/// Материализация IAsyncCursor без зависимости от конкретных extension-методов
/// драйвера (перебор через MoveNextAsync — работает во всех версиях драйвера).
/// </summary>
public static class MongoCursorHelpers
{
    public static async Task<List<T>> ToListAsync<T>(IAsyncCursor<T> cursor, CancellationToken ct)
    {
        var list = new List<T>();
        while (await cursor.MoveNextAsync(ct))
            list.AddRange(cursor.Current);
        return list;
    }

    public static async Task<T?> FirstOrDefaultAsync<T>(IAsyncCursor<T> cursor, CancellationToken ct)
    {
        while (await cursor.MoveNextAsync(ct))
        {
            var item = cursor.Current.FirstOrDefault();
            if (item is not null)
                return item;
        }
        return default;
    }
}
