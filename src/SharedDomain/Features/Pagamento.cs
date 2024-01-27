using AutoMapper;
using FluentResults;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SharedDomain.Shared;

namespace SharedDomain.Features;
public class Pagamento : IEntity
{
    public Guid Id { get; set; }
    public required string ContaDebitada { get; set; }
    public required string ContaCreditada { get; set; }
    public decimal Valor { get; set; }
    public PagamentoIncluidoType Type { get; set; }
}
public enum PagamentoIncluidoType { Open = 1, Submitted = 2, Rejected = 3, Closed = 4 }
public record InclusaoPagamentoCommand : ICommand
{
    public Guid Id { get; set; }
    public required string ContaDebitada { get; set; }
    public required string ContaCreditada { get; set; }
    public decimal Valor { get; set; }
    public PagamentoIncluidoType Type { get; set; }
}
public interface ICommandHandler<T, V>
    where T : ICommand
    where V : class
{
    Task<V> Handle(T command, CancellationToken cancellationToken);
}
public class PagamentoCommandHandler : CommandHandler<InclusaoPagamentoCommand, Result<Pagamento?>>
{
    private readonly IValidator<InclusaoPagamentoCommand> _validator;
    private readonly IEventProducer<PagamentoIncluidoEvent> _eventProducer;
    private readonly IPagamentoReadRepository _repository;

    public PagamentoCommandHandler(IValidator<InclusaoPagamentoCommand> validator,
        IEventProducer<PagamentoIncluidoEvent> pagamentoCriadoEvent,
        IPagamentoReadRepository repository)
    {
        _validator = validator;
        _eventProducer = pagamentoCriadoEvent;
        _repository = repository;
    }

    public override async Task<Result<Pagamento?>> Handle(InclusaoPagamentoCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);
        if (validation.IsInvalid())
            return validation.ToFailResult();

        await _repository.CreateAsync(command.MapTo<Pagamento>(), cancellationToken);
        await _eventProducer.Send(command.MapTo<PagamentoIncluidoEvent>(), cancellationToken);
        return Result.Ok();
    }
}
public class PagamentoIncluidoEvent : IEvent
{
    public Guid Id { get; set; }
    public PagamentoIncluidoType Type { get; set; }
}
public class PagamentoCriadoConsumer : IEventConsumer<PagamentoIncluidoEvent>
{
    public Task Consume(PagamentoIncluidoEvent @event, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
public class PagamentoIncluidoProducer : IEventProducer<PagamentoIncluidoEvent>
{
    public string TopicName => "pagamento-criado";

    public Task Send(PagamentoIncluidoEvent command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
public interface IProducer<T> where T : class
{
    Task Send(T @event);
}

public class PagamentoSubmetidoEvent
{
    public Guid Id { get; set; }
    public PagamentoIncluidoType Type { get; set; }
}


public abstract class EventConsumer<T> : IEventConsumer<T>
    where T : IEvent
{
    public abstract Task Consume(T @event, CancellationToken cancellationToken);
}
public class PagamentoIncluidoConsumer : EventConsumer<PagamentoIncluidoEvent>
{
    private readonly IPagamentoReadRepository? _repository;
    private readonly IProducer<PagamentoSubmetidoEvent> _producer;

    public PagamentoIncluidoConsumer(IPagamentoReadRepository? repository, IProducer<PagamentoSubmetidoEvent> producer)
    {
        _repository = repository;
        _producer = producer;
    }

    public override async Task Consume(PagamentoIncluidoEvent @event, CancellationToken cancellationToken)
    {
        var lista = await _repository.GetAsync(cancellationToken);
        await _producer.Send(@event.MapTo<PagamentoSubmetidoEvent>());
    }
}

public class BookStoreDatabaseSettings
{
    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public string BooksCollectionName { get; set; } = null!;
}
public interface IPagamentoReadRepository
{
    Task<List<Pagamento>> GetAsync(CancellationToken cancellationToken = default);

    Task<Pagamento?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task CreateAsync(Pagamento newBook, CancellationToken cancellationToken = default);

    Task UpdateAsync(Guid id, Pagamento updatedBook, CancellationToken cancellationToken = default);

    Task RemoveAsync(Guid id, CancellationToken cancellationToken = default);
}

public class WeatherForecastRepository : IPagamentoReadRepository
{
    private readonly IMongoCollection<Pagamento> _booksCollection;

    public WeatherForecastRepository(
        IOptions<BookStoreDatabaseSettings> bookStoreDatabaseSettings)
    {
        var mongoClient = new MongoClient(
            bookStoreDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            bookStoreDatabaseSettings.Value.DatabaseName);

        _booksCollection = mongoDatabase.GetCollection<Pagamento>(
            bookStoreDatabaseSettings.Value.BooksCollectionName);
    }

    public async Task<List<Pagamento>> GetAsync(CancellationToken cancellationToken = default) =>
        await _booksCollection.Find(_ => true).ToListAsync(cancellationToken);

    public async Task<Pagamento?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _booksCollection.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);

    [Obsolete]
    public async Task CreateAsync(Pagamento newBook, CancellationToken cancellationToken = default) =>
        await _booksCollection.InsertOneAsync(newBook, cancellationToken);

    public async Task UpdateAsync(Guid id, Pagamento updatedBook, CancellationToken cancellationToken = default) =>
        await _booksCollection.ReplaceOneAsync(x => x.Id == id, updatedBook);

    public async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _booksCollection.DeleteOneAsync(x => x.Id == id, cancellationToken);

    public void CreateAsync(Pagamento pagamento)
    {
        throw new NotImplementedException();
    }
}
public class PagamentoCommandValidator : AbstractValidator<InclusaoPagamentoCommand>
{
    public PagamentoCommandValidator()
    {
        RuleFor(x => x.Valor).GreaterThan(0);
    }
}
public class PagamentoMapping : Profile
{
    public PagamentoMapping()
    {
        CreateMap<InclusaoPagamentoCommand, Pagamento>();
        CreateMap<InclusaoPagamentoCommand, PagamentoIncluidoEvent>();
    }
}
public class KafkaProducer<T> where T : IEvent
{
}
public class KafkaConsume<T> where T : IEvent
{
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
            .AddValidators()
            .AddEntities();
        return services;
    }
    private static IServiceCollection AddConsumers(this IServiceCollection services)
    {
        services.AddScoped<IEventConsumer<PagamentoIncluidoEvent>, PagamentoCriadoConsumer>();
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
        services.AddScoped<ICommandHandler<InclusaoPagamentoCommand, Result<Pagamento?>>, PagamentoCommandHandler>();
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
}