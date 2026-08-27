using MongoDB.Bson;
using MongoDB.Driver;
using TimeTracking.Application.Common;
using TimeTracking.Application.Rules;
using TimeTracking.Domain;

namespace TimeTracking.Application.Entries;

/// <summary>
/// Атомарный счётчик часов сотрудника за календарный день (правило 2).
/// Unique-индекс {employeeId, date} делает $inc единственной точкой сериализации
/// параллельных записей на одного сотрудника в один день: вторая транзакция получает
/// WriteConflict и повторяется с актуальной суммой.
/// </summary>
internal static class DayCounterService
{
    public static async Task AddHoursAsync(
        ITimeTrackingDb db, IClientSessionHandle session, string employeeId, DateTime date, double hours, CancellationToken ct)
    {
        // _id задаётся явно через $setOnInsert: иначе сервер при upsert создаст ObjectId,
        // а сущность хранит Id строкой.
        var result = await db.DayCounters.FindOneAndUpdateAsync(
            session,
            Builders<DayCounter>.Filter.Eq(c => c.EmployeeId, employeeId)
                & Builders<DayCounter>.Filter.Eq(c => c.Date, date),
            Builders<DayCounter>.Update
                .SetOnInsert(c => c.Id, ObjectId.GenerateNewId().ToString())
                .Inc(c => c.Hours, hours),
            new FindOneAndUpdateOptions<DayCounter> { IsUpsert = true, ReturnDocument = ReturnDocument.After },
            ct);

        DayHoursLimitRule.ValidateDayTotal(result.Hours - hours, hours);
    }
}
