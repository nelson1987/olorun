using FluentResults;
using SharedDomain.Features.WeatherForecasts.Create;
using SharedDomain.Features.WeatherForecasts.Delete;
using SharedDomain.Features.WeatherForecasts.Entities;
using SharedDomain.Shared;

namespace SharedDomain.Features.WeatherForecasts;
public interface IWeatherForecastHandler
{
    Task<IList<WeatherForecast>> GetAsync(CancellationToken cancellationToken);

    Task<WeatherForecast> PutAsync(Guid id, CancellationToken cancellationToken);

    Task PostAsync(CancellationToken cancellationToken);

    Task DeleteAsync(Guid idClima, CancellationToken cancellationToken);
}

public class WeatherForecastHandler : IWeatherForecastHandler
{
    private string[] summaries = new[]
    {
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

    private readonly IWeatherForecastRepository _repository;
    private readonly IEventProducer<CreateWeatherForecastEvent> _createProducer;
    private readonly IEventProducer<DeleteWeatherForecastEvent> _deleteProducer;

    public WeatherForecastHandler(IWeatherForecastRepository repository, IEventProducer<CreateWeatherForecastEvent> createProducer, IEventProducer<DeleteWeatherForecastEvent> deleteProducer)
    {
        _repository = repository;
        _createProducer = createProducer;
        _deleteProducer = deleteProducer;
    }

    public async Task<IList<WeatherForecast>> GetAsync(CancellationToken cancellationToken)
    {
        return await _repository.GetAsync();
    }

    public async Task PostAsync(CancellationToken cancellationToken)
    {
        var command = new CreateWeatherForecastCommand()
        {
            Date = DateTime.Now.AddDays(1),
            Id = Guid.NewGuid(),
            Summary = summaries[Random.Shared.Next(summaries.Length)],
            TemperatureC = Random.Shared.Next(-20, 55)
        };
        await _createProducer.Send(command.MapTo<CreateWeatherForecastEvent>(), cancellationToken);
    }

    public async Task<WeatherForecast> PutAsync(Guid idClima, CancellationToken cancellationToken)
    {
        var climaAsync = await _repository.GetAsync(idClima, cancellationToken);
        var clima = climaAsync! with
        {
            Date = DateTime.Now.AddDays(2)
        };
        await _repository.UpdateAsync(climaAsync.Id, clima);
        return clima;
    }

    public async Task DeleteAsync(Guid idClima, CancellationToken cancellationToken)
    {
        var climaAsync = await _repository.GetAsync(idClima, cancellationToken);
        await _deleteProducer.Send(new DeleteWeatherForecastEvent()
        {
            Id = climaAsync!.Id
        }, cancellationToken);
    }
}