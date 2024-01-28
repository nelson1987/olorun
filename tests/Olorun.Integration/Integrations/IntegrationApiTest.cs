using Confluent.Kafka;
using FluentAssertions;
using MongoDB.Driver;
using Olorun.Integration.Configs;
using Olorun.Integration.Configs.Fixtures;
using Pagamento.Features.Entities;
using System.Text.Json;

namespace Olorun.Integration.Integrations;
public interface IKafkaProducer
{
    Task Produce<TMessage>(string topic, TMessage message);
}

public interface IKafkaConsumer
{
    void Consume(string topic);
}

public class KafkaConsumer
{
    private readonly IConsumer<Null, string>? _kafkaConsumer;

    public KafkaConsumer()
    {
        var config = new ConsumerConfig { BootstrapServers = "" };
        _kafkaConsumer = new ConsumerBuilder<Null, string>(config).Build();
    }

    public void Consume(string topic)
    {
        _kafkaConsumer.Subscribe(topic);
    }
}

public class KafkaProducer : IKafkaProducer
{
    //private readonly IProducer<WeatherForecast>? _kafkaProducer;
    private readonly IProducer<Null, string>? _kafkaProducer;
    private int disposed;

    public KafkaProducer()
    {
        var config = new ProducerConfig { BootstrapServers = "" };
        _kafkaProducer = new ProducerBuilder<Null, string>(config).Build();
    }
    public async Task Produce<TMessage>(string topic, TMessage message)
    {
        await _kafkaProducer.ProduceAsync(topic, new Message<Null, string>
        {
            Value = JsonSerializer.Serialize(message)
        });
    }
    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref disposed, 1, 0) == 1) return;
        _kafkaProducer?.Flush();
        _kafkaProducer?.Dispose();
    }

    public Task Subscribe<TMessage>(string topic, TMessage message)
    {
        throw new NotImplementedException();
    }
}
public class OrdersController
{
    private readonly IKafkaProducer _kafkaProducer;

    public OrdersController(IKafkaProducer kafkaProducer)
    {
        _kafkaProducer = kafkaProducer;
    }

    public async Task CreateOrder(WeatherForecast @event)
    {
        await _kafkaProducer.Produce("", @event);
    }
}
public class IntegrationApiTest : IntegrationTest
{
    private readonly IMongoCollection<WeatherForecast> _settlementOrderCollection;
    public IntegrationApiTest(IntegrationTestFixture integrationTestFixture) : base(integrationTestFixture)
    {
        _settlementOrderCollection = MongoFixture.MongoDatabase.
            GetCollection<WeatherForecast>
            ("invoices");
    }

    [Fact]
    public async Task Get()
    {
        // Arrange
        var creditNote = new WeatherForecast(DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
             Random.Shared.Next(-20, 55),
             "frio"
        );

        await MongoFixture.MongoDatabase
            .GetCollection<WeatherForecast>("invoices")//nameof(WeatherForecast))
            .InsertOneAsync(creditNote);


        var response = await ApiFixture.Client.GetAsync($"/weatherforecast");
        response.EnsureSuccessStatusCode(); // Status Code 200-299

        await KafkaFixture.Produce("", creditNote);
        await ApiFixture.Server.Consume<WeatherForecast>(TimeSpan.FromMinutes(1));
        //KafkaFixture.Consumer.Consume("");
        //var consumerEvent = JsonSerializer.Deserialize<WeatherForecast>(consumerResult.Message.Value);

        var getSettlementOrder = await _settlementOrderCollection
            .Find(x => x.Id == Guid.NewGuid())
            .FirstOrDefaultAsync();
        var cancellationRespondedEvent = KafkaFixture
            .Consume<WeatherForecast>
            ("").ValueOrDefault;

    }

    [Fact]
    public async Task Get_with_error()
    {
        var response = await ApiFixture.Client.GetAsync($"/weatherforecast/1");
        response.Should().Be400BadRequest().And
                         .MatchInContent("*You need at least one filter value filled.*");
    }
}