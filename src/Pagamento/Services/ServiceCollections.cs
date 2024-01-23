public static class Service
{
    public static IServiceCollection AddDependencies(this IServiceCollection services)
    {
        services.AddScoped<IRepository, Repository>()
                .AddScoped<IWeatherForecastHandler, WeatherForecastHandler>();
        return services;
    }

    public static IServiceCollection AddMongoDb(this IServiceCollection services)
    {
        services.Configure<BookStoreDatabaseSettings>(builder.Configuration.GetSection("BookStoreDatabase"));
        return services;
    }

    public static IServiceCollection AddKafka(this IServiceCollection services)
    {
        services.AddMassTransit(x =>
        {
            x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
            x.AddRider(rider =>
            {
                rider.AddProducer<CreateWeatherForecastEvent>("weatherforecast-requested");
                rider.AddConsumer<CreateWeatherForecastConsumer>();
                rider.AddProducer<DeleteWeatherForecastEvent>("weatherforecast-deleted");
                rider.AddConsumer<DeleteteWeatherForecastConsumer>();
                rider.UsingKafka((context, k) =>
                {
                    k.Host("kafka:9092");
                    k.TopicEndpoint<CreateWeatherForecastEvent>("weatherforecast-requested", "consumer-group-name", e =>
                    {
                        e.ConfigureConsumer<CreateWeatherForecastConsumer>(context);
                    });
                    k.TopicEndpoint<DeleteWeatherForecastEvent>("weatherforecast-deleted", "consumer-group-name", e =>
                    {
                        e.ConfigureConsumer<DeleteteWeatherForecastConsumer>(context);
                    });
                });
            });
        });
        return services;
    }
}