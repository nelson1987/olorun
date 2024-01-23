using FluentResults;
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

//public record Result
//{
//    internal static Result Ok()
//    {
//        throw new NotImplementedException();
//    }
//}

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
    Task RemoveAsync(Guid id, CancellationToken cancellationToken = default);
}

public class Repository : IRepository
{
    private readonly IMongoCollection<WeatherForecast> _booksCollection;

    public Repository(
        IOptions<BookStoreDatabaseSettings> bookStoreDatabaseSettings)
    {
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
public class CreateWeatherForecastConsumer :
        IConsumer<CreateWeatherForecastEvent>
{
    private readonly IRepository _repository;
    private readonly ILogger<CreateWeatherForecastConsumer> _log;

    public CreateWeatherForecastConsumer(IRepository repository, ILogger<CreateWeatherForecastConsumer> log)
    {
        _repository = repository;
        _log = log;
    }

    public async Task Consume(ConsumeContext<CreateWeatherForecastEvent> context)
    {
        _log.LogInformation("Consume");
        CreateWeatherForecastEvent mensagem = context.Message;
        WeatherForecast clima = new WeatherForecast(mensagem.Date, mensagem.TemperatureC, mensagem.Summary)
        {
            Id = mensagem.Id
        };
        await _repository.CreateAsync(clima);
    }
}
public class DeleteteWeatherForecastConsumer :
        IConsumer<DeleteWeatherForecastEvent>
{
    private readonly IRepository _repository;
    private readonly ILogger<DeleteteWeatherForecastConsumer> _log;

    public DeleteteWeatherForecastConsumer(IRepository repository, ILogger<DeleteteWeatherForecastConsumer> log)
    {
        _repository = repository;
        _log = log;
    }

    public async Task Consume(ConsumeContext<DeleteWeatherForecastEvent> context)
    {
        _log.LogInformation("Delete");
        DeleteWeatherForecastEvent mensagem = context.Message;
        await _repository.RemoveAsync(mensagem.Id);
    }
}

public record CreateWeatherForecastEvent
{
    public Guid Id { get; init; }
    public DateOnly Date { get; init; }
    public int TemperatureC { get; init; }
    public string? Summary { get; init; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
public record DeleteWeatherForecastEvent
{
    public Guid Id { get; init; }
}

public interface IWeatherForecastHandler
{
    Task<IList<WeatherForecast>> GetAsync();
    Task<WeatherForecast> PutAsync();
    Task PostAsync();
    Task DeleteAsync();
}
public class WeatherForecastHandler : IWeatherForecastHandler
{
    private string[] summaries = new[]
    {
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};
    private readonly IRepository _repository;
    private readonly ITopicProducer<CreateWeatherForecastEvent> _createProducer;
    private readonly ITopicProducer<DeleteWeatherForecastEvent> _deleteProducer;

    public WeatherForecastHandler(IRepository repository,
        ITopicProducer<CreateWeatherForecastEvent> createProducer,
        ITopicProducer<DeleteWeatherForecastEvent> deleteProducer)
    {
        _repository = repository;
        _createProducer = createProducer;
        _deleteProducer = deleteProducer;
    }

    public async Task<IList<WeatherForecast>> GetAsync()
    {
        return await _repository.GetAsync();
    }

    public async Task PostAsync()
    {
        await _createProducer.Produce(new CreateWeatherForecastEvent()
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
            Id = Guid.NewGuid(),
            Summary = summaries[Random.Shared.Next(summaries.Length)],
            TemperatureC = Random.Shared.Next(-20, 55)
        });
    }

    public async Task<WeatherForecast> PutAsync()
    {
        var climaAsync = await _repository.GetAsync();
        var clima = climaAsync.First() with
        {
            Date =
                DateOnly.FromDateTime(DateTime.Now.AddDays(2))
        };
        await _repository.UpdateAsync(climaAsync.First().Id, clima);
        return clima;
    }

    public async Task DeleteAsync()
    {
        var climaAsync = await _repository.GetAsync();
        DeleteWeatherForecastEvent @event = new DeleteWeatherForecastEvent()
        {
            Id = climaAsync.First().Id
        };
        await _deleteProducer.Produce(@event);
    }

}