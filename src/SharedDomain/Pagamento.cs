using FluentResults;
using Microsoft.Extensions.DependencyInjection;

namespace SharedDomain;
public interface IEntity
{
    Guid Id { get; set; }
}
public class Pagamento : IEntity
{
    public Guid Id { get; set; }
    public string ContaDebitada { get; set; }
    public string ContaCreditada { get; set; }
    public decimal Valor { get; set; }
}
public interface ICommand
{
}
public class PagamentoCommand : ICommand
{
    public string ContaDebitada { get; set; }
    public string ContaCreditada { get; set; }
    public decimal Valor { get; set; }
}
public interface ICommandHandler<T, V>
    where T : ICommand
    where V : class
{
    Task<V> Handle(T command, CancellationToken cancellationToken);
}
public class PagamentoCommandHandler : ICommandHandler<PagamentoCommand, Result<Pagamento?>>
{
    public Task<Result<Pagamento?>> Handle(PagamentoCommand command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
public interface IEvent { }
public class PagamentoCriadoEvent : IEvent { }
public interface IEventConsumer<T, V>
    where T : IEvent
    where V : class
{
    Task<V> Consume(T command, CancellationToken cancellationToken);
}
public class PagamentoCriadoConsumer : IEventConsumer<PagamentoCriadoEvent, Result<PagamentoCriadoEvent>>
{
    public Task<Result<PagamentoCriadoEvent>> Consume(PagamentoCriadoEvent command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
public interface IEventProducer<T, V>
    where T : IEvent
    where V : class
{
    Task<V> Send(T command, CancellationToken cancellationToken);
}
public class PagamentoCriadoProducer : IEventProducer<PagamentoCriadoEvent, Result<PagamentoCriadoEvent>>
{
    public Task<Result<PagamentoCriadoEvent>> Send(PagamentoCriadoEvent command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

public static class Dependencies
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services.AddConsumers()
            .AddProducers()
            .AddEvents()
            .AddHandlers()
            .AddCommands()
            .AddEntities();
        return services;
    }
    private static IServiceCollection AddConsumers(this IServiceCollection services)
    {
        services.AddScoped<IEventConsumer<PagamentoCriadoEvent, Result<PagamentoCriadoEvent>>, PagamentoCriadoConsumer>();
        return services;
    }
    private static IServiceCollection AddProducers(this IServiceCollection services)
    {
        services.AddScoped<IEventProducer<PagamentoCriadoEvent, Result<PagamentoCriadoEvent>>, PagamentoCriadoProducer>();
        return services;
    }
    private static IServiceCollection AddEvents(this IServiceCollection services)
    {
        services.AddScoped<IEvent, PagamentoCriadoEvent>();
        return services;
    }
    private static IServiceCollection AddHandlers(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<PagamentoCommand, Result<Pagamento?>>, PagamentoCommandHandler>();
        return services;
    }
    private static IServiceCollection AddCommands(this IServiceCollection services)
    {
        services.AddScoped<ICommand, PagamentoCommand>();
        return services;
    }
    private static IServiceCollection AddEntities(this IServiceCollection services)
    {
        services.AddScoped<IEntity, Pagamento>();
        return services;
    }
}