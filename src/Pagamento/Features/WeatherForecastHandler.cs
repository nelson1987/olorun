using Pagamento.Features.Create;
using Pagamento.Features.Delete;
using Pagamento.Features.Entities;
using Pagamento.Services;

namespace Pagamento.Features;
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
    private readonly ITesteMessageProducer<CreateWeatherForecastEvent> _createProducer;
    private readonly ITesteMessageProducer<DeleteWeatherForecastEvent> _deleteProducer;

    public WeatherForecastHandler(IWeatherForecastRepository repository)
    {
        _repository = repository;
        _createProducer = new TesteMessageProducer<CreateWeatherForecastEvent>();
        _deleteProducer = new TesteMessageProducer<DeleteWeatherForecastEvent>();
    }

    public async Task<IList<WeatherForecast>> GetAsync(CancellationToken cancellationToken)
    {
        return await _repository.GetAsync();
    }

    public async Task PostAsync(CancellationToken cancellationToken)
    {
        await _createProducer.Produce(new CreateWeatherForecastEvent()
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
            Id = Guid.NewGuid(),
            Summary = summaries[Random.Shared.Next(summaries.Length)],
            TemperatureC = Random.Shared.Next(-20, 55)
        });
    }

    public async Task<WeatherForecast> PutAsync(Guid idClima, CancellationToken cancellationToken)
    {
        var climaAsync = await _repository.GetAsync(idClima, cancellationToken);
        var clima = climaAsync! with
        {
            Date =
                DateOnly.FromDateTime(DateTime.Now.AddDays(2))
        };
        await _repository.UpdateAsync(climaAsync.Id, clima);
        return clima;
    }

    public async Task DeleteAsync(Guid idClima, CancellationToken cancellationToken)
    {
        var climaAsync = await _repository.GetAsync(idClima, cancellationToken);
        await _deleteProducer.Produce(new DeleteWeatherForecastEvent()
        {
            Id = climaAsync!.Id
        });
    }
}