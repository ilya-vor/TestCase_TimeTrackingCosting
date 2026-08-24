using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TimeTracking.Domain;

[BsonIgnoreExtraElements]
public class TimeEntry
{
    [BsonId]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string EmployeeId { get; set; } = string.Empty;

    public string ProjectId { get; set; } = string.Empty;

    /// <summary>Календарная дата записи (UTC, полночь).</summary>
    public DateTime Date { get; set; }

    /// <summary>Часы. Деньги тут нет, поэтому double допустим.</summary>
    public double Hours { get; set; }

    public string Comment { get; set; } = string.Empty;

    /// <summary>Оптимистичная блокировка.</summary>
    public int Version { get; set; } = 1;

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string UpdatedBy { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; }
}
