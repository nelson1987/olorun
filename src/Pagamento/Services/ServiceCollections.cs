using SharedDomain.Features;
using SharedDomain.Features.Pagamentos;
using SharedDomain.Features.Pagamentos.Events.Incluido;
using SharedDomain.Features.WeatherForecasts;
using SharedDomain.Features.WeatherForecasts.Create;
using SharedDomain.Features.WeatherForecasts.Delete;
using SharedDomain.Shared;

namespace Pagamento.Services;
public static class Service
{
    public static IServiceCollection AddDependencies(this IServiceCollection services)
    {
        services.AddScoped<IWeatherForecastRepository, WeatherForecastRepository>()
                .AddScoped<IWeatherForecastHandler, WeatherForecastHandler>()
                .AddDomain();
        return services;
    }

    public static IServiceCollection AddMongoDb(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.Configure<BookStoreDatabaseSettings>(configuration.GetSection("BookStoreDatabase"));
        return services;
    }

    public static IServiceCollection AddKafka(this IServiceCollection services)
    {
        services.AddScoped<IEventProducer<CreateWeatherForecastEvent>, CreateWeatherForecastProducer>();
        services.AddScoped<IEventProducer<DeleteWeatherForecastEvent>, DeleteWeatherForecastProducer>();
        services.AddScoped<IEventConsumer<CreateWeatherForecastEvent>, CreateWeatherForecastConsumer>();
        services.AddScoped<IEventConsumer<DeleteWeatherForecastEvent>, DeleteWeatherForecastConsumer>();
        services.AddScoped<IEventProducer<PagamentoIncluidoEvent>, PagamentoIncluidoProducer>();
        return services;
    }
}