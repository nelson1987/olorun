using Confluent.Kafka;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace SharedDomain.Integrations;
public record CriacaoPessoaCommand();
public record PessoaCriadaEvent();
#region Handler
public interface ICriacaoPessoaHandler
{
    Task<Result> Handle(CriacaoPessoaCommand command, CancellationToken cancellationToken);
}
public class CriacaoPessoaHandler : ICriacaoPessoaHandler
{
    private readonly IClienteApiEventProducer _producer;

    public CriacaoPessoaHandler(IClienteApiEventProducer producer)
    {
        _producer = producer;
    }

    public async Task<Result> Handle(CriacaoPessoaCommand command, CancellationToken cancellationToken)
    {
        var produtor = await _producer.SendCriacaoPessoa(new PessoaCriadaEvent(), cancellationToken);
        return produtor.IsFailed ? Result.Fail(produtor.Errors) : Result.Ok();
    }
}
public interface IClienteApiEventProducer
{
    Task<Result> SendCriacaoPessoa(PessoaCriadaEvent @event, CancellationToken cancellationToken);
}
public class ClienteApiEventProducer : IClienteApiEventProducer
{

    public Task<Result> SendCriacaoPessoa(PessoaCriadaEvent @event, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
public interface IPessoaPersistence
{
}
public interface IPessoaRead
{
}
#endregion
#region EventService
#region EventEntities
public readonly struct EventClientRequest<TData>
{
    public TData Data { get; }
    public string Topic { get; }
    public string Subject { get; }
    public bool ShouldCompressData { get; }
    public IEnumerable<KeyValuePair<string, string>> Metadata { get; }

    public EventClientRequest(
        TData data,
        string topic,
        string subject,
        bool shouldCompressData = true,
        IEnumerable<KeyValuePair<string, string>> metadata = null)
    {
        Data = data;
        Topic = topic;
        Subject = subject;
        ShouldCompressData = shouldCompressData;
        Metadata = metadata;
    }
}

public class EventMessageBuilder : MessageBuilder<long, string>
{
    private readonly Func<object, string> _serializer;

    public EventMessageBuilder(Func<object, string> serializer) =>
        _serializer = serializer;

    public EventMessageBuilder WithValue<TData>(EventClientRequest<TData> value)
    {
        var data = EventClientData.Create(value, _serializer);
        var serializedData = _serializer(data);
        Message.Value = serializedData;

        return this;
    }
}

public class MessageBuilder<TKey, TValue>
{
    protected readonly Message<TKey, TValue> Message;

    public MessageBuilder()
    {
        Message = new Message<TKey, TValue>()
        {
            Headers = new Headers()
        };
    }

    public MessageBuilder<TKey, TValue> WithKey(TKey key)
    {
        Message.Key = key;

        return this;
    }

    public MessageBuilder<TKey, TValue> WithValue(TValue value)
    {
        Message.Value = value;

        return this;
    }

    public Message<TKey, TValue> Create() => Message;
}

public sealed class EventClientData
{
    public Guid Id { get; set; }
    public string Topic { get; set; }
    public string Subject { get; set; }
    public string EventType { get; set; }
    public DateTime EventTime { get; set; }
    public string Data { get; set; }
    public bool? IsDataCompressed { get; set; }
    public string TraceKey { get; set; }

    public static EventClientData Create<TData>(EventClientRequest<TData> request) =>
        Create(request, (data) => JsonSerializer.Serialize(data));

    public static EventClientData Create<TData>(EventClientRequest<TData> request, Func<object, string> serializer)
    {
        if (request.Data == null)
            throw new ArgumentException("'Data' should not be null.");

        if (string.IsNullOrWhiteSpace(request.Topic))
            throw new ArgumentException("'TopicName' is invalid.");

        if (string.IsNullOrWhiteSpace(request.Subject))
            throw new ArgumentException("'Subject' is invalid.");

        return new EventClientData
        {
            Id = Guid.NewGuid(),
            Data = serializer(request.Data),
            EventTime = DateTime.UtcNow,
            EventType = request.Data.GetType().Name,
            Subject = request.Subject,
            Topic = request.Topic,
            IsDataCompressed = request.ShouldCompressData
        };
    }
}
#endregion

public interface IEventCommunicationService
{
    Task<Result> ProduceEvent<TData>(TData data, string topic, string subject, CancellationToken cancellationToken = default);
}
public class EventCommunicationService : IEventCommunicationService
{
    private readonly IEventClient _eventClient;

    public EventCommunicationService(IEventClient eventClient)
    {
        _eventClient = eventClient;
    }

    public async Task<Result> ProduceEvent<TData>(TData data, string topic, string subject, CancellationToken cancellationToken = default)
    {
        var eventClientRequest = new EventClientRequest<TData>(data, topic, subject);
        var produceResult = await _eventClient.Produce(eventClientRequest, Newtonsoft.Json.JsonConvert.SerializeObject, cancellationToken);

        if (produceResult)
        {
            return Result.Ok();
        }

        return Result.Fail("Failed to produce event");
    }
}

public interface IEventClient
{
    Task<bool> Produce<TData>(EventClientRequest<TData> request, CancellationToken cancellationToken = default);
    Task<bool> Produce<TData>(EventClientRequest<TData> request, Func<object, string> serializer,
        CancellationToken cancellationToken = default);
}
public sealed class EventClient : IEventClient
{
    private readonly IEventClientProducer _producer;
    private readonly ILogger<EventClient> _log;

    public EventClient(IEventClientProducer producer, ILogger<EventClient> log)
    {
        _producer = producer;
        _log = log;
    }

    public async Task<bool> Produce<TData>(EventClientRequest<TData> request, CancellationToken cancellationToken = default)
        => await ProduceEvent(request, (data) => JsonSerializer.Serialize(data), cancellationToken);
    public async Task<bool> Produce<TData>(EventClientRequest<TData> request, Func<object, string> serializer, CancellationToken cancellationToken = default)
        => await ProduceEvent(request, serializer, cancellationToken);

    private async Task<bool> ProduceEvent<TData>(EventClientRequest<TData> request, Func<object, string> serializer, CancellationToken cancellationToken)
    {
        try
        {
            var eventMessage = new EventMessageBuilder(serializer)
                .WithValue(request)
                .WithKey(DateTime.UtcNow.Ticks)
                .Create();

            var deliveryResult = await _producer.Producer.ProduceAsync(request.Topic, eventMessage, cancellationToken);
            return deliveryResult.Status is PersistenceStatus.Persisted;
        }
        catch (Exception e)
        {
            _log.LogError($"Produce error: {e.Message}");
            return false;
        }
    }
}

public interface IEventClientProducer
{
    IProducer<long, string> Producer { get; }
}
public sealed class EventClientProducer : IEventClientProducer, IDisposable
{
    public IProducer<long, string> Producer { get; }

    public EventClientProducer()
    {
        var config = new ProducerConfig();

        Producer = new ProducerBuilder<long, string>(config)
            .SetKeySerializer(Serializers.Int64)
            .SetValueSerializer(Serializers.Utf8)
            .Build();
    }

    public void Dispose()
    {
        Producer.Flush();
        Producer.Dispose();
    }
}
#endregion
public static class Dependencies
{
    public static IServiceCollection AddIntegrationTests(this IServiceCollection services)
    {
        services.AddScoped<ICriacaoPessoaHandler, CriacaoPessoaHandler>()
                .AddScoped<IClienteApiEventProducer, ClienteApiEventProducer>()
                .AddSingleton<IEventClient, EventClient>()
                .AddSingleton<IEventCommunicationService, EventCommunicationService>()
                .AddSingleton<IEventClientProducer, EventClientProducer>();
        return services;
    }
}
