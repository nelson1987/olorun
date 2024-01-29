using Confluent.Kafka;

namespace Olorun.Integration;
public class KafkaIntegrationTest : IAsyncLifetime
{
    private readonly IFixture _fixture;
    private readonly string _topic;
    private readonly string _bootstrapServers;
    private readonly IProducer<Null, string> _producer;
    private readonly IConsumer<Null, string> _consumer;

    public KafkaIntegrationTest()
    {
        _fixture = new Fixture();
        _topic = "weatherforecast-requested";
        _bootstrapServers = "kafka:9092"; // e.g., "localhost:9092"

        var producerConfig = new ProducerConfig { BootstrapServers = _bootstrapServers };
        _producer = new ProducerBuilder<Null, string>(producerConfig).Build();

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _bootstrapServers,
            GroupId = "test-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        _consumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();
    }

    [Fact]
    public async Task GivenEventProduced_WhenConsumed_ThenShouldReceiveSameEvent()
    {
        // Arrange
        var message = _fixture.Create<string>();

        // Act
        await ProduceMessageAsync(message);
        var receivedMessage = await ConsumeMessageAsync();

        // Assert
        Assert.Equal(message, receivedMessage);
    }

    private async Task ProduceMessageAsync(string message)
    {
        await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = message });
    }

    private async Task<string?> ConsumeMessageAsync()
    {
        _consumer.Subscribe(_topic);

        var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(10));

        return consumeResult?.Message?.Value;
    }

    public async Task InitializeAsync()
    {
        // Additional setup before the tests run
    }

    public async Task DisposeAsync()
    {
        // Cleanup after the tests run
        _producer.Dispose();
        _consumer.Dispose();
    }
}