using System.Text.Json;

namespace Olorun.Integration;

public class Serializer
{
    public static string Serialize(object obj)
    {
        return JsonSerializer.Serialize(obj);
    }

    public static T Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json);
    }
}
