using Olorun.Integration.Configs.Fixtures;

namespace Olorun.Integration.Configs
{
    [CollectionDefinition(nameof(IntegrationCollection))]
    public class IntegrationCollection : ICollectionFixture<IntegrationTestFixture>
    {
    }
}