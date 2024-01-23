using MongoDB.Bson.Serialization.Attributes;

namespace Pagamento.Features.Entities;
public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    [BsonId]
    public Guid Id { get; init; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}