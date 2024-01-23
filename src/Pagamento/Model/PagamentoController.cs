using FluentResults;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Pagamento.Features;

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
    private readonly IWeatherForecastRepository _repository;
    private readonly IProducer<PagamentoIncluidoEvent> _producer;

    public InclusaoPagamentoHandler(IWeatherForecastRepository repository, IProducer<PagamentoIncluidoEvent> producer)
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
    private readonly IWeatherForecastRepository? _repository;
    private readonly IProducer<PagamentoSubmetidoEvent> _producer;

    public PagamentoIncluidoEventHandler(IWeatherForecastRepository? repository, IProducer<PagamentoSubmetidoEvent> producer)
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


public class BookStoreDatabaseSettings
{
    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public string BooksCollectionName { get; set; } = null!;
}