using Olorun.Integration.Configs.Fixtures;
using Xunit.Categories;

namespace Olorun.Integration.Configs;
[Collection(nameof(IntegrationCollection))]
[IntegrationTest]
public abstract class IntegrationTest : IAsyncLifetime
{
    public Fixture ObjectGenerator { get; } = new Fixture();
    protected ApiFixture ApiFixture { get; }
    protected KafkaFixture KafkaFixture { get; }
    protected MongoFixture MongoFixture { get; }
    protected IntegrationTest(IntegrationTestFixture integrationTestFixture)
    {
        ApiFixture = integrationTestFixture.ApiFixture;
        KafkaFixture = integrationTestFixture.KafkaFixture;
        MongoFixture = integrationTestFixture.MongoFixture;
    }

    public virtual async Task InitializeAsync()
    {
        ApiFixture.Reset();
        KafkaFixture.Reset();
        //HttpServerFixture.Reset();
        //await MongoFixture.Reset();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
