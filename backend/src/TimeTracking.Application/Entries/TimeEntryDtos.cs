namespace TimeTracking.Application.Entries;

public class TimeEntryRow
{
    public string Id { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string ProjectCode { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public double Hours { get; set; }
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// Применённая ставка (на дату записи, пересчитывается при изменении ставок задним числом).
    /// </summary>
    public decimal Rate { get; set; }

    /// <summary>
    /// Стоимость = часы × ставка, округлена до копеек.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Переработка от 12 часов за день
    /// </summary>
    public bool Overtime { get; set; }

    /// <summary>
    /// Версия записи для оптимистичной блокировки.
    /// </summary>
    public int Version { get; set; }
}

public class TimeEntryPageResult
{
    public List<TimeEntryRow> Items { get; set; } = new();
    public long TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }

    /// <summary>
    /// Итог по всей отфильтрованной выборке, а не по странице.
    /// </summary>
    public double TotalHours { get; set; }

    public decimal TotalAmount { get; set; }
}
