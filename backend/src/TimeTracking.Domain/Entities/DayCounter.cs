using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TimeTracking.Domain;

[BsonIgnoreExtraElements]
public class DayCounter
{
    [BsonId]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string EmployeeId { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public double Hours { get; set; }
}
