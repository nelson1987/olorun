using AutoMapper;
using SharedDomain.Features.WeatherForecasts.Create;

namespace SharedDomain.Features.WeatherForecasts;

public class WeatherForecastMapping : Profile
{
    public WeatherForecastMapping()
    {
        CreateMap<CreateWeatherForecastCommand, CreateWeatherForecastEvent>();
    }
}
