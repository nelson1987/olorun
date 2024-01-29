using Microsoft.Extensions.Logging;
using SharedDomain.Features.WeatherForecasts.Entities;
using SharedDomain.Shared;

namespace SharedDomain.Features.WeatherForecasts.Create;
public class CreateWeatherForecastConsumer : EventConsumer<CreateWeatherForecastEvent>
{
    private readonly IWeatherForecastRepository _repository;
    private readonly ILogger<CreateWeatherForecastConsumer> _log;

    public CreateWeatherForecastConsumer(IWeatherForecastRepository repository, ILogger<CreateWeatherForecastConsumer> log)
    {
        _repository = repository;
        _log = log;
    }

    public async Task Consume(CreateWeatherForecastEvent mensagem)
    {
        _log.LogInformation("Consume");
        var mensageiro = Consume(CancellationToken.None);
        WeatherForecast clima = new WeatherForecast
        {
            Date = mensagem.Date,
            TemperatureC = mensagem.TemperatureC,
            Summary = mensagem.Summary,
            Id = mensagem.Id
        };
        await _repository.CreateAsync(clima);
    }
}