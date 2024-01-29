using Confluent.Kafka;
using System.Text.Json;
using System.Text;

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

public interface ITesteMessageProducer<TMessage>
{
    Task Produce(TMessage message);
}

public class TesteMessageProducer<TMessage> : ITesteMessageProducer<TMessage>
{
    private readonly IProducer<Null, TMessage> _producer;
    private readonly CancellationToken _token = CancellationToken.None;
    public TesteMessageProducer()
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = "localhost:9092",
        };
        _producer = new ProducerBuilder<Null, TMessage>(producerConfig)
            .SetValueSerializer(new SerializerMessage<TMessage>())
            .Build();
    }
    public async Task Produce(TMessage message)
    {
        await _producer.ProduceAsync("weatherforecast-requested", new Message<Null, TMessage>
        {
            Value = message
        }, _token);
    }
}

public interface ITesteMessageConsumer<TMessage>
{
    Task<TMessage> ConsumeMessageAsync();
}
public class TesteMessageConsumer<TMessage> : ITesteMessageConsumer<TMessage>
{
    private readonly IConsumer<Null, TMessage> _consumer;
    public TesteMessageConsumer()
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
    }
    public async Task<TMessage> ConsumeMessageAsync()
    {
        _consumer.Subscribe("weatherforecast-requested");

        var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(10));

        return consumeResult.Message.Value;
    }
}
