using MassTransit;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Pagamento.Model;
public interface IProducer<T> where T : class
{
    Task Send(T @event);
}

public record InclusaoPagamentoCommand
{
    public Guid Id { get; set; }
}
public enum PagamentoIncluidoType { Open = 1, Submitted = 2, Rejected = 3, Closed = 4 }
public class PagamentoIncluidoEvent
{
    public Guid Id { get; set; }
    public PagamentoIncluidoType Type { get; set; }
}

public class PagamentoSubmetidoEvent
{
    public Guid Id { get; set; }
    public PagamentoIncluidoType Type { get; set; }
}

public class InclusaoPagamentoHandler
{
    private readonly IRepository _repository;
    private readonly IProducer<PagamentoIncluidoEvent> _producer;

    public InclusaoPagamentoHandler(IRepository repository, IProducer<PagamentoIncluidoEvent> producer)
    {
        _repository = repository;
        _producer = producer;
    }

    public async Task<Result> Handle(InclusaoPagamentoCommand request, CancellationToken cancellationToken)
    {
        var lista = await _repository.GetAsync(request.Id, cancellationToken);
        await _producer.Send(request.ToMap<PagamentoIncluidoEvent>());
        return Result.Ok();
    }
}

public class PagamentoIncluidoEventHandler
{
    private readonly IRepository? _repository;
    private readonly IProducer<PagamentoSubmetidoEvent> _producer;

    public PagamentoIncluidoEventHandler(IRepository? repository, IProducer<PagamentoSubmetidoEvent> producer)
    {
        _repository = repository;
        _producer = producer;
    }

    public async Task<Result> Handle(PagamentoIncluidoEvent @event, CancellationToken cancellationToken)
    {
        var lista = await _repository.GetAsync(cancellationToken);
        await _producer.Send(@event.ToMap<PagamentoSubmetidoEvent>());
        return Result.Ok();
    }
}

public static class Mapper
{
    public static T ToMap<T>(this object obj)
    {
        throw new NotImplementedException();
    }
}

public record Result
{
    internal static Result Ok()
    {
        throw new NotImplementedException();
    }
}

public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{

    [BsonId]
    public Guid Id { get; init; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public interface IRepository
{
    Task<List<WeatherForecast>> GetAsync(CancellationToken cancellationToken = default);
    Task<WeatherForecast?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateAsync(WeatherForecast newBook, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, WeatherForecast updatedBook);
}

public class Repository : IRepository
{
    private readonly IMongoCollection<WeatherForecast> _booksCollection;

    public Repository(
        IOptions<BookStoreDatabaseSettings> bookStoreDatabaseSettings)
    {
        /*
        MongoClientSettings settings = new MongoClientSettings();
        settings.Server = new MongoServerAddress("mongodb", 27017);
        settings.UseTls = true;
        //settings.SslSettings = new SslSettings() { 
        //    EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12 
        //};

        MongoIdentity identity = new MongoInternalIdentity("sales", "sales");
        MongoIdentityEvidence evidence = new PasswordEvidence("sales");

        settings.Credential = new MongoCredential("SCRAM-SHA-256", identity, evidence);

        var mongoClient = new MongoClient(settings);
        */
        var mongoClient = new MongoClient(
            bookStoreDatabaseSettings.Value.ConnectionString);        

        var mongoDatabase = mongoClient.GetDatabase(
            bookStoreDatabaseSettings.Value.DatabaseName);

        _booksCollection = mongoDatabase.GetCollection<WeatherForecast>(
            bookStoreDatabaseSettings.Value.BooksCollectionName);
    }

    public async Task<List<WeatherForecast>> GetAsync(CancellationToken cancellationToken = default) =>
        await _booksCollection.Find(_ => true).ToListAsync(cancellationToken);

    public async Task<WeatherForecast?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _booksCollection.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);

    [Obsolete]
    public async Task CreateAsync(WeatherForecast newBook, CancellationToken cancellationToken = default) =>
        await _booksCollection.InsertOneAsync(newBook, cancellationToken);

    public async Task UpdateAsync(Guid id, WeatherForecast updatedBook) =>
        await _booksCollection.ReplaceOneAsync(x => x.Id == id, updatedBook);

    public async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _booksCollection.DeleteOneAsync(x => x.Id == id, cancellationToken);

}

public class BookStoreDatabaseSettings
{
    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public string BooksCollectionName { get; set; } = null!;
}
public class KafkaMessageConsumer :
        IConsumer<WeatherForecastEvent>
{
    private readonly IRepository _repository;
    private readonly ILogger<KafkaMessageConsumer> _log;

    public KafkaMessageConsumer(IRepository repository, ILogger<KafkaMessageConsumer> log)
    {
        _repository = repository;
        _log = log;
    }

    public async Task Consume(ConsumeContext<WeatherForecastEvent> context)
    {
        _log.LogInformation("Consume");
        WeatherForecastEvent mensagem = context.Message;
        WeatherForecast clima = new WeatherForecast(mensagem.Date, mensagem.TemperatureC, mensagem.Summary)
        {
            Id = mensagem.Id
        };
        await _repository.CreateAsync(clima);
    }
}

public record WeatherForecastEvent
{
    public Guid Id { get; init; }
    public DateOnly Date { get; init; }
    public int TemperatureC { get; init; }
    public string? Summary { get; init; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}