using Confluent.Kafka;
using Pagamento.Services;

namespace SharedDomain.Shared;

public interface IEventConsumer<T>
    where T : IEvent
{
    string TopicName { get; }
    Task<T> Consume();
}

public class EventConsumer<TMessage> : IEventConsumer<TMessage> where TMessage : IEvent
{
    private readonly IConsumer<Null, TMessage> _consumer;
    public EventConsumer()
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "teste",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        _consumer = new ConsumerBuilder<Null, TMessage>(consumerConfig)
            .SetValueDeserializer(new SerializerMessage<TMessage>())
            .Build();
        TopicName = "weatherforecast-requested";
    }
    public string TopicName { get; }

    public async Task<TMessage> Consume()
    {
        _consumer.Subscribe("weatherforecast-requested");

        var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(10));

        return consumeResult.Message.Value;
    }
}
