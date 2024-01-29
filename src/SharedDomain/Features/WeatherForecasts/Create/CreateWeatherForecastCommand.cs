using SharedDomain.Shared;

namespace SharedDomain.Features.WeatherForecasts.Create
{
    public record CreateWeatherForecastCommand : ICommand
    {
        public Guid Id { get; init; }
        public DateOnly Date { get; init; }
        public int TemperatureC { get; init; }
        public string? Summary { get; init; }
    }
}
