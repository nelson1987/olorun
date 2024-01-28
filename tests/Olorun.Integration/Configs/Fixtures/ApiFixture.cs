using FluentResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Olorun.Integration.Configs.Environments;

namespace Olorun.Integration.Configs.Fixtures;

public sealed class ApiFixture : IAsyncDisposable
{
    public Api Server { get; } = new();
    public HttpClient Client { get; }

    public ApiFixture()
    {
        Client = Server.CreateDefaultClient();
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}
public sealed class KafkaFixture
{
    internal static readonly string[] Topics = GetKafkaTopicNames();
    private readonly IEventClient _eventClient;
    //private readonly IEventClientConsumers _eventClientConsumers;

    public KafkaFixture(Api server)
    {
        _eventClient = server.Services.GetRequiredService<IEventClient>();
        // _eventClientConsumers = server.Services.GetRequiredService<IEventClientConsumers>();
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

    public Task<bool> Produce<T>(string topic, T data)
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
        throw new NotImplementedException();
    }
}
public sealed class MongoFixture
{
    public IMongoDatabase MongoDatabase { get; }
    public MongoFixture(Api server)
    {
        var configuration = server.Services.GetRequiredService<IConfiguration>();
        var mongoUrl = new MongoUrl(configuration.GetConnectionString("MongoDB"));
        var mongoClient = new MongoClient(mongoUrl);
        MongoDatabase = mongoClient.GetDatabase(mongoUrl.DatabaseName);
    }
}
public interface IEventClient
{
    Task<bool> Produce<TMessage>(string message, TMessage @event, CancellationToken cancellationToken);
    Task<bool> Consume<TMessage>(string message, TMessage @event, CancellationToken cancellationToken);
}
//public interface IEventClientConsumers { }