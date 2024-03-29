using FluentAssertions;
using MongoDB.Driver;
using Newtonsoft.Json;
using Olorun.Integration.Configs;
using Olorun.Integration.Configs.Fixtures;
using SharedDomain.Configs;
using SharedDomain.Features.WeatherForecasts.Create;
using SharedDomain.Features.WeatherForecasts.Entities;
using System.Reflection;
using System.Text;
using Xunit.Abstractions;

namespace Olorun.Integration.Integrations
{
    public class IntegrationApiTest : IntegrationTest
    {
        private WeatherForecast _weatherForecast { get; set; }
        public IntegrationApiTest(IntegrationTestFixture integrationTestFixture) : base(integrationTestFixture)
        {
            _weatherForecast = WeatherForecastFactory.Create();
        }

        [Fact]
        public async Task Get()
        {
            // Act

            await MongoFixture.MongoDatabase
                .GetCollection<WeatherForecast>("invoices")//nameof(WeatherForecast))
                .InsertOneAsync(_weatherForecast);

            var updateFinancialData = new CadastroContaCommand()
            {
                Documento = "Documento",
                Id = Guid.NewGuid()
            };
            var updateFinancialDataAsString = JsonConvert.SerializeObject(updateFinancialData);
            using var stringContent = new StringContent(updateFinancialDataAsString, Encoding.UTF8, "application/json");

            // Act
            var response = await ApiFixture.Client.PutAsync($"/weatherforecast", stringContent);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
        }

        [Fact]
        public async Task Get_with_error()
        {
            var response = await ApiFixture.Client.GetAsync($"/weatherforecast/1");
            response.Should().Be404NotFound();
            //response.Should().Be400BadRequest().And
            //                 .MatchInContent("*You need at least one filter value filled.*");
        }

        [Fact]
        public async Task Post_with_Success()
        {
            var command = new CreateWeatherForecastCommand()
            {
                Date = DateTime.Now.AddDays(1),
                Id = Guid.NewGuid(),
                Summary = "calor",
                TemperatureC = Random.Shared.Next(-20, 55)
            };
            var updateFinancialDataAsString = JsonConvert.SerializeObject(command);
            using var stringContent = new StringContent(updateFinancialDataAsString, Encoding.UTF8, "application/json");

            var response = await ApiFixture.Client.PostAsync($"/api/v1/weatherforecasts", stringContent);
            var receivedMessage = await KafkaFixture.ConsumeMessageAsync();
            response.Should().Be200Ok();
            receivedMessage.Should().Be(command.MapTo<CreateWeatherForecastEvent>());
        }

        [Fact]
        public async Task GivenEventProduced_WhenConsumed_ThenShouldReceiveSameEvent()
        {
            // Arrange
            var message = new CreateWeatherForecastEvent()
            {
                Date = DateTime.Now.AddDays(1),
                Id = Guid.NewGuid(),
                Summary = "calor",
                TemperatureC = Random.Shared.Next(-20, 55)
            };

            // Act
            await KafkaFixture.Produce<CreateWeatherForecastEvent>("weatherforecast-requested",message);
            var receivedMessage = await KafkaFixture.ConsumeMessageAsync();

            // Assert
            receivedMessage.Should().Be(message);
        }
    }
    public static class WeatherForecastFactory
    {
        public static WeatherForecast Create()
        {
            return new WeatherForecast
            {
                Date = DateTime.Today,
                TemperatureC = 35,
                Summary = "cool",
                Id = Guid.NewGuid(),
            };
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

}