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
public class EventsSettings
{
    public static EventClientSettings GetSettings(IConfiguration configuration, bool useConsumers = true)
    {
        var settingsSection = configuration.GetSection("EventsSettings");
        var bootstrapServers = settingsSection["BootstrapServers"];
        var verboseLoggingEnabled = settingsSection.GetValue("EnableVerboseMessageLogging", defaultValue: false);
        var producerCredential = GetCredential(settingsSection.GetSection("ProducerCredential"));
        var consumerCredential = GetCredential(settingsSection.GetSection("ConsumerCredential"));

        var eventSettings = new EventClientSettings(bootstrapServers)// EventClientSet tings(bootstrapServers, producerCredential, consumerCredential)
        {
            ProducerTopics = ProducerTopics(),
            EnableVerboseMessageLogging = verboseLoggingEnabled
        };

        if (useConsumers)
        {
            eventSettings.ConsumerGroup = settingsSection["ConsumerGroup"];
            eventSettings.ConsumerTopics = ConsumerTopics();
        }

        return eventSettings;
    }

    private static EventClientCredential GetCredential(IConfigurationSection section) => new(section["Username"], section["Password"]);

    private static IList<string> ConsumerTopics()
    {
        return new[]
        {
            EventsTopics.WeatherforecastResponded.Name,
        };
    }

    private static IList<string> ProducerTopics()
    {
        return new[]
        {
            EventsTopics.WeatherforecastResponded.Name,
        };
    }
}