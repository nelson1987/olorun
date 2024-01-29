using Microsoft.Extensions.Logging;
using SharedDomain.Shared;

namespace SharedDomain.Features.WeatherForecasts.Delete;
public class DeleteWeatherForecastConsumer :
        EventConsumer<DeleteWeatherForecastEvent>
{
    private readonly IWeatherForecastRepository _repository;
    private readonly ILogger<DeleteWeatherForecastConsumer> _log;

    public DeleteWeatherForecastConsumer(IWeatherForecastRepository repository, ILogger<DeleteWeatherForecastConsumer> log)
    {
        _repository = repository;
        _log = log;
    }

    public async Task Consume(DeleteWeatherForecastEvent mensagem)
    {
        _log.LogInformation("Delete");
        await _repository.RemoveAsync(mensagem.Id);
    }
}