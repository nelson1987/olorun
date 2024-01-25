using Newtonsoft.Json;
using Olorun.Integration.Configs;
using System.Text;

namespace Olorun.Integration.Integrations;

public class IntegrationTests : IClassFixture<OlorunApiFixture>
{
    private readonly OlorunApiFixture _olorunApiFixture;

    public IntegrationTests(OlorunApiFixture olorunApiFixture)
        => _olorunApiFixture = olorunApiFixture;

    [Fact]
    public void Dado_Request_Valido_Deve_Retornar_True()
    {
        // Act
        var updateFinancialData = new CadastroContaCommand()
        {
            Documento = "Documento",
            Id = Guid.NewGuid()
        };
        var updateFinancialDataAsString = JsonConvert.SerializeObject(updateFinancialData);
        using var stringContent = new StringContent(updateFinancialDataAsString, Encoding.UTF8, "application/json");

        // Act
        var response = _olorunApiFixture.Client.PutAsync("/credit-titles/financial", stringContent);

        // Assert
        Assert.True(response.Result.IsSuccessStatusCode);
    }
}
