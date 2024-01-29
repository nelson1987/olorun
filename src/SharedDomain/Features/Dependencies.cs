using FluentResults;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SharedDomain.Features.Pagamentos;
using SharedDomain.Features.Pagamentos.Events.Incluido;
using SharedDomain.Features.Pagamentos.Inclusao;
using SharedDomain.Features.WeatherForecasts;
using SharedDomain.Features.WeatherForecasts.Create;
using SharedDomain.Features.WeatherForecasts.Delete;
using SharedDomain.Features.WeatherForecasts.Entities;
using SharedDomain.Shared;

namespace SharedDomain.Features;

public static class Dependencies
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services
            .AddConsumers()
            .AddProducers()
            .AddEvents()
            .AddHandlers()
            .AddCommands()
            .AddValidators()
            .AddEntities()
            .AddRepositories();
        return services;
    }
    private static IServiceCollection AddConsumers(this IServiceCollection services)
    {
        services.AddScoped<IEventConsumer<CreateWeatherForecastEvent>, CreateWeatherForecastConsumer>();
        services.AddScoped<IEventConsumer<DeleteWeatherForecastEvent>, DeleteWeatherForecastConsumer>();
        return services;
    }
    private static IServiceCollection AddProducers(this IServiceCollection services)
    {
        services.AddScoped<IEventProducer<CreateWeatherForecastEvent>, CreateWeatherForecastProducer>();
        services.AddScoped<IEventProducer<DeleteWeatherForecastEvent>, DeleteWeatherForecastProducer>();
        services.AddScoped<IEventProducer<PagamentoIncluidoEvent>, PagamentoIncluidoProducer>();
        return services;
    }
    private static IServiceCollection AddEvents(this IServiceCollection services)
    {
        services.AddScoped<IEvent, PagamentoIncluidoEvent>();
        services.AddScoped<IEvent, CreateWeatherForecastEvent>();
        services.AddScoped<IEvent, DeleteWeatherForecastEvent>();
        return services;
    }
    private static IServiceCollection AddHandlers(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<InclusaoPagamentoCommand, Result<Pagamento?>>, InclusaoPagamentoCommandHandler>();
        services.AddScoped<ICommandHandler<CreateWeatherForecastCommand, Result<WeatherForecast?>>, CreateWeatherForecastCommandHandler>();
        return services;
    }
    private static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<InclusaoPagamentoCommand>, PagamentoCommandValidator>();
        services.AddScoped<IValidator<CreateWeatherForecastCommand>, CreateWeatherForecastCommandValidator>();
        return services;
    }
    private static IServiceCollection AddCommands(this IServiceCollection services)
    {
        services.AddScoped<ICommand, InclusaoPagamentoCommand>();
        services.AddScoped<ICommand, CreateWeatherForecastCommand>();
        return services;
    }
    private static IServiceCollection AddEntities(this IServiceCollection services)
    {
        services.AddScoped<IEntity, Pagamento>();
        services.AddScoped<IEntity, WeatherForecast>();
        return services;
    }
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IPagamentoReadRepository, PagamentoReadRepository>();
        services.AddScoped<IWeatherForecastRepository, WeatherForecastRepository>();
        return services;
    }
}