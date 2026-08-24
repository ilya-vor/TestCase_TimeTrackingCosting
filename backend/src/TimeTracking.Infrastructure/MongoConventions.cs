using MongoDB.Bson.Serialization.Conventions;

namespace TimeTracking.Infrastructure;

public static class MongoConventions
{
    /// <summary>
    /// camelCase для полей в BSON/JSON: employeeId, projectId, projectCode и т.д.
    /// Регистрируется до первого обращения к сериализаторам.
    /// </summary>
    public static void Register()
    {
        var pack = new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new IgnoreExtraElementsConvention(true)
        };
        ConventionRegistry.Register("time-tracking", pack, _ => true);
    }
}
