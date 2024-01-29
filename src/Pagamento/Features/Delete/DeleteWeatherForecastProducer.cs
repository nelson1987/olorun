using Pagamento.Services;

namespace Pagamento.Features.Delete;
public class DeleteWeatherForecastProducer :
        TesteMessageProducer<DeleteWeatherForecastEvent>
{
    //private readonly IWeatherForecastRepository _repository;
    //private readonly ILogger<DeleteteWeatherForecastConsumer> _log;

    //public DeleteteWeatherForecastConsumer(IWeatherForecastRepository repository, ILogger<DeleteteWeatherForecastConsumer> log)
    //{
    //    _repository = repository;
    //    _log = log;
    //}

    //public async Task Consume(DeleteWeatherForecastEvent mensagem)
    //{
    //    _log.LogInformation("Delete");
    //    await _repository.RemoveAsync(mensagem.Id);
    //}
}