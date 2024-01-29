using Confluent.Kafka;
using SharedDomain.Infrastructure.Kafka;

namespace SharedDomain.Shared;

public interface IEventProducer<T>
    where T : IEvent
{
    string TopicName { get; }
    Task Send(T @event, CancellationToken cancellationToken);
}

public class EventProducer<TMessage> : IEventProducer<TMessage> where TMessage : IEvent
{
    private readonly IProducer<Null, TMessage> _producer;
    public EventProducer()
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = "localhost:9092",
        };
        _producer = new ProducerBuilder<Null, TMessage>(producerConfig)
            .SetValueSerializer(new SerializerMessage<TMessage>())
            .Build();
        TopicName = "weatherforecast-requested";
    }

    public string TopicName { get; }

    public async Task Send(TMessage message, CancellationToken cancellationToken)
    {
        await _producer.ProduceAsync(TopicName, new Message<Null, TMessage>
        {
            Value = message
        }, cancellationToken);
    }
}
