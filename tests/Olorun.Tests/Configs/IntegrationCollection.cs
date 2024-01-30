using Olorun.Tests.Configs.Fixtures;

namespace Olorun.Tests.Configs
{
    [CollectionDefinition(nameof(IntegrationCollection))]
    public class IntegrationCollection : ICollectionFixture<IntegrationTestFixture>
    {
    }
}