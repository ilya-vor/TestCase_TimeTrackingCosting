using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TimeTracking.Domain;

[BsonIgnoreExtraElements]
public class Project
{
    [BsonId]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    [BsonRepresentation(MongoDB.Bson.BsonType.Decimal128)]
    public decimal Budget { get; set; }

    public DateTime Start { get; set; }

    public DateTime? End { get; set; }
}
