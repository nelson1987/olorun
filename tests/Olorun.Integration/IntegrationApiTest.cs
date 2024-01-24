using FluentAssertions;
using Inflames.Api;

namespace Inflames.tests
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
            var creditNote = new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = "frio"
            };

            await MongoFixture.MongoDatabase
                .GetCollection<WeatherForecast>(nameof(WeatherForecast))
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
}