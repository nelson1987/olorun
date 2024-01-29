using MongoDB.Driver;
using Olorun.Integration.Configs;
using Olorun.Integration.Configs.Fixtures;
using SharedDomain.Features.WeatherForecasts.Create;
using SharedDomain.Features.WeatherForecasts.Entities;
using Xunit.Abstractions;

namespace Olorun.Integration.Integrations
{
    public class IntegrationConsumerTests : IntegrationTest
    {
        private readonly WeatherForecast _renegotiationOrderEvent;
        private readonly IMongoCollection<WeatherForecast> _creditNoteCollection;
        public IntegrationConsumerTests(IntegrationTestFixture integrationTestFixture, ITestOutputHelper testOutputHelper) : base(integrationTestFixture)
        {
            _renegotiationOrderEvent = testOutputHelper.ReadRequestFile<WeatherForecast>();
            _creditNoteCollection = MongoFixture.MongoDatabase
                .GetCollection<WeatherForecast>("invoices");// Collections.CreditNoteAssets.Name);
        }

        [Fact]
        public async Task Given_a_valid_credit_note_renegotiation_should_output_expected_results()
        {
            // Arrange
            var creditNote = WeatherForecastFactory.Create() with { Id = _renegotiationOrderEvent.Id };
            await _creditNoteCollection.InsertOneAsync(creditNote);

            // Act
            //await KafkaFixture.Produce(creditNote);
            //var receivedMessage = await KafkaFixture.ConsumeMessageAsync();

            await KafkaFixture.Produce("", _renegotiationOrderEvent);
            await ApiFixture.Server.Consume<CreateWeatherForecastEvent>(TimeSpan.FromMinutes(1));

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

}