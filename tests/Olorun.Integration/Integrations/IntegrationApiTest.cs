using FluentAssertions;
using MongoDB.Driver;
using Newtonsoft.Json;
using Olorun.Integration.Configs;
using Olorun.Integration.Configs.Fixtures;
using Pagamento.Features.Entities;
using System.Reflection;
using Xunit.Abstractions;

namespace Olorun.Integration.Integrations
{
    public class IntegrationApiTest : IntegrationTest
    {
        public IntegrationApiTest(IntegrationTestFixture integrationTestFixture) : base(integrationTestFixture)
        {
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
        }

        [Fact]
        public async Task Get_with_error()
        {
            var response = await ApiFixture.Client.GetAsync($"/weatherforecast/1");
            response.Should().Be400BadRequest().And
                             .MatchInContent("*You need at least one filter value filled.*");
        }
    }

    public static class CreditNoteFactory
    {
        public static WeatherForecast Create()
        {
            return new WeatherForecast(DateOnly.MaxValue, 12, "cool");
        }
    }

    public static class TestOutputHelperExtensions
    {
        private static string GetFilePath(ITestOutputHelper testOutputHelper)
        {
            var type = testOutputHelper.GetType();
            var testMember = type.GetField("test", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var test = ((ITest)testMember.GetValue(testOutputHelper)!)!;
            var className = test.TestCase.TestMethod.TestClass.Class.Name.Split('.').Last().Split('+').First();
            var testDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

            return Path.Combine(testDirectory, "Files", $"{className}.request.json");
        }

        public static T ReadRequestFile<T>(this ITestOutputHelper testOutputHelper)
        {
            var path = GetFilePath(testOutputHelper);
            using var stream = new StreamReader(path);
            using var reader = new JsonTextReader(stream);
            return JsonSerializer.Create()!.Deserialize<T>(reader)!;
        }
    }

    public class ConsumerTests : IntegrationTest
    {
        private readonly WeatherForecast _renegotiationOrderEvent;
        private readonly IMongoCollection<WeatherForecast> _creditNoteCollection;
        public ConsumerTests(IntegrationTestFixture integrationTestFixture, ITestOutputHelper testOutputHelper) : base(integrationTestFixture)
        {
            _renegotiationOrderEvent = testOutputHelper.ReadRequestFile<WeatherForecast>();
            _creditNoteCollection = MongoFixture.MongoDatabase
                .GetCollection<WeatherForecast>("invoices");// Collections.CreditNoteAssets.Name);
        }

        [Fact]
        public async Task Given_a_valid_credit_note_renegotiation_should_output_expected_results()
        {
            // Arrange
            var creditNote = CreditNoteFactory.Create() with { Id = _renegotiationOrderEvent.Id };
            await _creditNoteCollection.InsertOneAsync(creditNote);

            //await KafkaFixture.Produce(EventsTopics.OrdersRenegotiationRequested.Name, _renegotiationOrderEvent);

            //// Act
            //await ApiFixture.Server.Consume<RenegotiationConsumer>(TimeSpan.FromMinutes(1));

            // Assert
            var response = await GetOutputedResponses();
        }

        private async Task<object> GetOutputedResponses()
        {
            var creditNote = await _creditNoteCollection
                .Find(x => x.Id == _renegotiationOrderEvent.Id)
                .FirstOrDefaultAsync();

            //var ordersRenegotiationResponded = KafkaFixture
            //    .Consume<WeatherForecast>(EventsTopics.OrdersRenegotiationResponded.Name)
            //    .ValueOrDefault;

            return new
            {
                creditNote,
                //ordersRenegotiationResponded,
            };
        }

    }