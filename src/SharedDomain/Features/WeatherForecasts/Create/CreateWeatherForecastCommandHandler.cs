using FluentResults;
using SharedDomain.Features.WeatherForecasts.Entities;
using SharedDomain.Shared;

namespace SharedDomain.Features.WeatherForecasts.Create
{
    public class CreateWeatherForecastCommandHandler : CommandHandler<CreateWeatherForecastCommand, Result<WeatherForecast?>>
    {
        public override Task<Result<WeatherForecast?>> Handle(CreateWeatherForecastCommand command, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
