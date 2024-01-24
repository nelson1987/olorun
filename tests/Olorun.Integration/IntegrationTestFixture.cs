using Inflames.tests.Configs.Fixtures;

namespace Inflames.tests
{
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

        public Task InitializeAsync()
        {
            //throw new NotImplementedException();
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            //throw new NotImplementedException();
            return Task.CompletedTask;
        }
    }
}