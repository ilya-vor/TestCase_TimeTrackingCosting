using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TimeTracking.Domain;

[BsonIgnoreExtraElements]
public class Employee
{
    [BsonId]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string Name { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public List<Rate> Rates { get; set; } = new();
}
