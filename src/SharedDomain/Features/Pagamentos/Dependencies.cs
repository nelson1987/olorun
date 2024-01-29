using FluentResults;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SharedDomain.Features.Pagamentos.Events.Incluido;
using SharedDomain.Features.Pagamentos.Inclusao;
using SharedDomain.Shared;

namespace SharedDomain.Features.Pagamentos;

public static class Dependencies
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services.AddConsumers()
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
        services.AddScoped<IEventConsumer<PagamentoIncluidoEvent>, PagamentoIncluidoConsumer>();
        services.AddScoped<IProducer<PagamentoSubmetidoEvent>, PagamentoSubmetidoEventProducer>();
        return services;
    }
    private static IServiceCollection AddProducers(this IServiceCollection services)
    {
        services.AddScoped<IEventProducer<PagamentoIncluidoEvent>, PagamentoIncluidoProducer>();
        return services;
    }
    private static IServiceCollection AddEvents(this IServiceCollection services)
    {
        services.AddScoped<IEvent, PagamentoIncluidoEvent>();
        return services;
    }
    private static IServiceCollection AddHandlers(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<InclusaoPagamentoCommand, Result<Pagamento?>>, InclusaoPagamentoCommandHandler>();
        return services;
    }
    private static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<InclusaoPagamentoCommand>, PagamentoCommandValidator>();
        return services;
    }
    private static IServiceCollection AddCommands(this IServiceCollection services)
    {
        services.AddScoped<ICommand, InclusaoPagamentoCommand>();
        return services;
    }
    private static IServiceCollection AddEntities(this IServiceCollection services)
    {
        services.AddScoped<IEntity, Pagamento>();
        return services;
    }
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IPagamentoReadRepository, PagamentoReadRepository>();
        return services;
    }
}