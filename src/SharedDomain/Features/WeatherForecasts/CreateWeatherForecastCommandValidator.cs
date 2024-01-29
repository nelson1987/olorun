using FluentValidation;
using SharedDomain.Features.WeatherForecasts.Create;

namespace SharedDomain.Features.WeatherForecasts
{
    internal class CreateWeatherForecastCommandValidator : AbstractValidator<CreateWeatherForecastCommand>
    {
    }
}
