using MongoDB.Bson.Serialization.Attributes;

namespace TimeTracking.Domain;

public class Rate
{
    public DateTime From { get; set; }

    [BsonRepresentation(MongoDB.Bson.BsonType.Decimal128)]
    public decimal Value { get; set; }
}
