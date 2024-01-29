using SharedDomain.Shared;

namespace SharedDomain.Features.WeatherForecasts.Delete;
public record DeleteWeatherForecastEvent : IEvent
{
    public Guid Id { get; init; }
}