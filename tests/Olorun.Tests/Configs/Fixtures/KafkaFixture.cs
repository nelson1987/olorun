using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using Olorun.Tests.Configs.Environments;
using SharedDomain.Features.WeatherForecasts.Create;
using SharedDomain.Shared;

namespace Olorun.Tests.Configs.Fixtures;

public sealed class KafkaFixture
{
    internal static readonly string[] Topics = GetKafkaTopicNames();
    private readonly IEventProducer<CreateWeatherForecastEvent> _producer;
    private readonly IEventConsumer<CreateWeatherForecastEvent> _consumer;
    //private readonly IEventClient _eventClient;
    //private readonly IEventClientConsumers _eventClientConsumers;

    public KafkaFixture(Api server)
    {
        //_eventClient = server.Services.GetRequiredService<IEventClient>();
        // _eventClientConsumers = server.Services.GetRequiredService<IEventClientConsumers>();
        _producer = server.Services.GetRequiredService<IEventProducer<CreateWeatherForecastEvent>>();
        _consumer = server.Services.GetRequiredService<IEventConsumer<CreateWeatherForecastEvent>>();
    }

    public async Task WarmUp()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        foreach (var topic in Topics)
        {
            while (!cts.IsCancellationRequested)
            {
                await Produce(topic, new object());
                var response = Consume<object>(topic, msTimeout: 5);
                if (response.IsSuccess)
                    break;
            }
        }

        if (cts.IsCancellationRequested)
            throw new TimeoutException("Unable to warm up Kafka.");
    }
    public void Reset()
    {
        //ClearTopicsMessages();
    }

    private void ClearTopicsMessages()
    {
        foreach (var topic in Topics)
            ConsumeAll<object>(topic, msTimeout: 5);
    }
    public IReadOnlyList<T> ConsumeAll<T>(string topic, int msTimeout = 150)
    {
        var messages = new List<T>();
        while (true)
        {
            var result = Consume<T>(topic, msTimeout);
            if (result.IsSuccess)
            {
                messages.Add(result.Value);
                continue;
            }

            break;
        }

        return messages;
    }
    public async Task ProduceMessageAsync(CreateWeatherForecastEvent message)
    {
        using var cancellationToken = ExpiringCancellationToken();
        await _producer.Send(message, cancellationToken.Token);
    }
    public async Task<CreateWeatherForecastEvent> ConsumeMessageAsync()
    {
        using var cancellationToken = ExpiringCancellationToken();
        return _consumer.Consume(cancellationToken.Token).Result;
    }

    public async Task<bool> Produce<T>(string topic, T data)
    {
        //using var cancellationToken = ExpiringCancellationToken();

        //return _eventClient.Produce(
        //        new EventClientRequest<T>(data, topic, "subject"),
        //        obj => JsonConvert.SerializeObject(obj, new StringEnumConverter()),
        //        cancellationToken.Token);
        throw new NotImplementedException();
    }

    public Result<T> Consume<T>(string topic, int msTimeout = 150)
    {
        //try
        //{
        //    if (typeof(T) == typeof(object))
        //    {
        //        // Quando utilizamos o parâmetro de timeout do tipo int
        //        // no consumo do kafka temos um ganho significativo
        //        // de performance no método ClearTopicsMessages
        //        // comparado ao timeout através de cancellationToken
        //        var consumeResult = _eventClientConsumers
        //            .Get(topic)
        //            .Consume(msTimeout);

        //        var isFailed = consumeResult == null || consumeResult.IsPartitionEOF;
        //        return isFailed ? Result.Fail("A timeout has ocurred or end of partition found") : (T)new object();
        //    }

        //    using var cancellationToken = ExpiringCancellationToken(msTimeout);
        //    var message = _eventClient.Consume(topic, JsonConvert.DeserializeObject<T>, cancellationToken.Token);

        //    return message is not null
        //        ? Result.Ok(message)
        //        : Result.Fail<T>("no messages found");
        //}
        //catch (OperationCanceledException)
        //{
        //    return Result.Fail<T>("no messages found");
        //}
        throw new NotImplementedException();
    }


    private static CancellationTokenSource ExpiringCancellationToken(int msTimeout = 150)
    {
        var timeout = TimeSpan.FromMilliseconds(msTimeout);
        return new CancellationTokenSource(timeout);
    }

    private static string[] GetKafkaTopicNames()
    {
        //return typeof(EventsTopics)
        //    .GetTypeInfo()
        //    .DeclaredNestedTypes
        //    .Select(x => x.GetField("Name")!.GetValue(null))
        //    .Cast<string>()
        //    .ToArray();

        return new string[] { "weatherforecast-requested", "weatherforecast-responded", "weather-topic" };
    }
}
//public interface IEventClientConsumers { }