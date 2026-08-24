using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TimeTracking.Domain;

[BsonIgnoreExtraElements]
public class ClosedPeriod
{
    [BsonId]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public int Year { get; set; }

    public int Month { get; set; }
}
