namespace TimeTracking.Application.Common;

public static class Dates
{
    /// <summary>
    /// Клиент присылает календарную дату («2026-03-05»). Храним её как UTC 00:00:00
    /// того же календарного дня и не смещаем на часовой пояс сервера.
    /// </summary>
    public static DateTime CalendarDateToUtc(DateTime value) => DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);
}
