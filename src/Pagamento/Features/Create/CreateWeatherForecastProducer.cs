using Pagamento.Services;

namespace Pagamento.Features.Create;
public class CreateWeatherForecastProducer : EventProducer<CreateWeatherForecastEvent>
{
    //private readonly IWeatherForecastRepository _repository;
    //private readonly ILogger<CreateWeatherForecastConsumer> _log;

    //public CreateWeatherForecastConsumer(IWeatherForecastRepository repository, ILogger<CreateWeatherForecastConsumer> log)
    //{
    //    _repository = repository;
    //    _log = log;
    //}

    //public async Task Consume(CreateWeatherForecastEvent mensagem)
    //{
    //    _log.LogInformation("Consume");
    //    var mensageiro = ConsumeMessageAsync();
    //    WeatherForecast clima = new WeatherForecast(mensagem.Date, mensagem.TemperatureC, mensagem.Summary)
    //    {
    //        Id = mensagem.Id
    //    };
    //    await _repository.CreateAsync(clima);
    //}
}