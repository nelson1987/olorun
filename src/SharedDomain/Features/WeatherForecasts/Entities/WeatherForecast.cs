using MongoDB.Bson.Serialization.Attributes;
using SharedDomain.Shared;

namespace SharedDomain.Features.WeatherForecasts.Entities;
public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary) : IEntity
{

    [BsonId]
    public Guid Id { get; set; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}