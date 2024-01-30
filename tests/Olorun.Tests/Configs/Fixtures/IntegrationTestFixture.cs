namespace Olorun.Tests.Configs.Fixtures;

public class IntegrationTestFixture : IAsyncLifetime
{
    public ApiFixture ApiFixture { get; }
    public KafkaFixture KafkaFixture { get; }
    public MongoFixture MongoFixture { get; }

    public IntegrationTestFixture()
    {
        ApiFixture = new ApiFixture();
        KafkaFixture = new KafkaFixture(ApiFixture.Server);
        MongoFixture = new MongoFixture(ApiFixture.Server);
    }

    public async Task InitializeAsync()
    {
        //await KafkaFixture.WarmUp();
    }

    public async Task DisposeAsync()
    {
        await ApiFixture.DisposeAsync();
        //throw new NotImplementedException();
        //return Task.CompletedTask;
    }
}