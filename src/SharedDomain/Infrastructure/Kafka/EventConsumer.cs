using Confluent.Kafka;
using System.Text.Json;
using System.Text;
using SharedDomain.Shared;

namespace Pagamento.Services;
public class SerializerMessage<TMessage> : ISerializer<TMessage>, IDeserializer<TMessage>
{
    public TMessage Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        return JsonSerializer.Deserialize<TMessage>(Encoding.UTF8.GetString(data.ToArray()));
    }

    public byte[] Serialize(TMessage data, SerializationContext context)
    {
        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));
    }
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
