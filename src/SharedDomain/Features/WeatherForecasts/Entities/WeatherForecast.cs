using MongoDB.Bson.Serialization.Attributes;
using SharedDomain.Shared;

namespace SharedDomain.Features.WeatherForecasts.Entities;
public record WeatherForecast : IEntity
{

    [BsonId]
    public Guid Id { get; set; }
    public DateTime Date { get; init; }
    public int TemperatureC { get; init; }
    public string? Summary { get; init; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}