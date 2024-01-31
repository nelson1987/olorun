using SharedDomain;
using SharedDomain.Features;
using SharedDomain.Features.WeatherForecasts;

namespace Pagamento.Services;
public static class Service
{
    public static IServiceCollection AddDependencies(this IServiceCollection services)
    {
        services
                .AddScoped<IWeatherForecastHandler, WeatherForecastHandler>()
                .AddScoped<IPessoaReadRepository, PessoaReadRepository>()
                .AddDomain();
        return services;
    }

    public static IServiceCollection AddMongoDb(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.Configure<BookStoreDatabaseSettings>(configuration.GetSection("BookStoreDatabase"));
        return services;
    }

    public static IServiceCollection AddEventClientSettings(this IServiceCollection services, EventClientSettings eventClientSettings)
    {
        return services
            .AddSingleton<IEventClient, EventClient>()
            .AddEventClient(eventClientSettings);
    }

    private static IServiceCollection AddEventClient(this IServiceCollection services, EventClientSettings eventClientSettings)
    {
        return services
            .AddSingleton<IEventClientConsumers>(x => new EventClientConsumers(eventClientSettings))
            .AddSingleton<IEventClientProducer>(x => new EventClientProducer(eventClientSettings));
    }

    public static IServiceCollection AddEvents(this IServiceCollection services, IConfiguration configuration, bool useConsumers = true)
    {
        var eventsSettings = EventsSettings.GetSettings(configuration, useConsumers);
        services.AddEventClientSettings(eventsSettings);
        services.AddHealthChecks();
        //.AddHealthChecksConfluentKafka(eventsSettings, HealthStatus.Degraded);

        services.AddSingleton<IEventCommunicationService, EventCommunicationService>();

        return services;
    }
}