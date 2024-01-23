
public interface IWeatherForecastHandler
{
    Task<IList<WeatherForecast>> GetAsync();
    Task<WeatherForecast> PutAsync();
    Task PostAsync();
    Task DeleteAsync();
}

public class WeatherForecastHandler : IWeatherForecastHandler
{
    private string[] summaries = new[]
    {
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};
    private readonly IRepository _repository;
    private readonly ITopicProducer<CreateWeatherForecastEvent> _createProducer;
    private readonly ITopicProducer<DeleteWeatherForecastEvent> _deleteProducer;

    public WeatherForecastHandler(IRepository repository,
        ITopicProducer<CreateWeatherForecastEvent> createProducer,
        ITopicProducer<DeleteWeatherForecastEvent> deleteProducer)
    {
        _repository = repository;
        _createProducer = createProducer;
        _deleteProducer = deleteProducer;
    }

    public async Task<IList<WeatherForecast>> GetAsync()
    {
        return await _repository.GetAsync();
    }

    public async Task PostAsync()
    {
        await _createProducer.Produce(new CreateWeatherForecastEvent()
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
            Id = Guid.NewGuid(),
            Summary = summaries[Random.Shared.Next(summaries.Length)],
            TemperatureC = Random.Shared.Next(-20, 55)
        });
    }

    public async Task<WeatherForecast> PutAsync()
    {
        var climaAsync = await _repository.GetAsync();
        var clima = climaAsync.First() with
        {
            Date =
                DateOnly.FromDateTime(DateTime.Now.AddDays(2))
        };
        await _repository.UpdateAsync(climaAsync.First().Id, clima);
        return clima;
    }

    public async Task DeleteAsync()
    {
        var climaAsync = await _repository.GetAsync();
        DeleteWeatherForecastEvent @event = new DeleteWeatherForecastEvent()
        {
            Id = climaAsync.First().Id
        };
        await _deleteProducer.Produce(@event);
    }

}