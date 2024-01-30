using Olorun.Tests.Configs;
using Olorun.Tests.Configs.Fixtures;
using SharedDomain.Configs;
using SharedDomain.Features.WeatherForecasts.Create;
using System.Text;

namespace Olorun.Tests.Integrations.Apis
{
    public class PagamentoApiIntegrationTests : IntegrationTest
    {
        private readonly CreateWeatherForecastCommand _request;

        public PagamentoApiIntegrationTests(IntegrationTestFixture integrationTestFixture) : base(integrationTestFixture)
        {
            _request = ObjectGenerator.Build<CreateWeatherForecastCommand>()
                .Create();
        }

        [Fact]
        public async Task Executar_Post_Com_Sucesso()
        {
            //Arrange
            var updateFinancialDataAsString = Serializer.Serialize(_request);
            using var stringContent = new StringContent(updateFinancialDataAsString, Encoding.UTF8, "application/json");

            // Act
            var response = await ApiFixture.Client.PostAsync($"/api/v1/weatherforecasts", stringContent);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
        }
    }
}