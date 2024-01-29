using SharedDomain.Features;
using SharedDomain.Features.WeatherForecasts;

namespace Pagamento.Services;
public static class Service
{
    public static IServiceCollection AddDependencies(this IServiceCollection services)
    {
        services
                .AddScoped<IWeatherForecastHandler, WeatherForecastHandler>()
                .AddDomain();
        return services;
    }

    public static IServiceCollection AddMongoDb(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.Configure<BookStoreDatabaseSettings>(configuration.GetSection("BookStoreDatabase"));
        return services;
    }
}