using SharedDomain.Shared;

namespace SharedDomain.Features.WeatherForecasts.Create;
public record CreateWeatherForecastEvent : IEvent
{
    public Guid Id { get; init; }
    public DateTime Date { get; init; }
    public int TemperatureC { get; init; }
    public string? Summary { get; init; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}