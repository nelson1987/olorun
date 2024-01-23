public class CreateWeatherForecastConsumer :
        IConsumer<CreateWeatherForecastEvent>
{
    private readonly IRepository _repository;
    private readonly ILogger<CreateWeatherForecastConsumer> _log;

    public CreateWeatherForecastConsumer(IRepository repository, ILogger<CreateWeatherForecastConsumer> log)
    {
        _repository = repository;
        _log = log;
    }

    public async Task Consume(ConsumeContext<CreateWeatherForecastEvent> context)
    {
        _log.LogInformation("Consume");
        CreateWeatherForecastEvent mensagem = context.Message;
        WeatherForecast clima = new WeatherForecast(mensagem.Date, mensagem.TemperatureC, mensagem.Summary)
        {
            Id = mensagem.Id
        };
        await _repository.CreateAsync(clima);
    }
}
