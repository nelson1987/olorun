using Confluent.Kafka;
using System.Text;
using System.Text.Json;

namespace SharedDomain.Infrastructure.Kafka;
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
