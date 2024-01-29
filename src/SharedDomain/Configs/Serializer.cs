using System.Text.Json;

namespace SharedDomain.Configs;

public static class Serializer
{
    public static string Serialize(this object obj)
    {
        return JsonSerializer.Serialize(obj);
    }

    public static T Deserialize<T>(this string json)
    {
        return JsonSerializer.Deserialize<T>(json);
    }
}
