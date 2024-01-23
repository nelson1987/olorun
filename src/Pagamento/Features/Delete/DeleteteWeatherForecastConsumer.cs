public class DeleteteWeatherForecastConsumer :
        IConsumer<DeleteWeatherForecastEvent>
{
    private readonly IRepository _repository;
    private readonly ILogger<DeleteteWeatherForecastConsumer> _log;

    public DeleteteWeatherForecastConsumer(IRepository repository, ILogger<DeleteteWeatherForecastConsumer> log)
    {
        _repository = repository;
        _log = log;
    }

    public async Task Consume(ConsumeContext<DeleteWeatherForecastEvent> context)
    {
        _log.LogInformation("Delete");
        DeleteWeatherForecastEvent mensagem = context.Message;
        await _repository.RemoveAsync(mensagem.Id);
    }
}