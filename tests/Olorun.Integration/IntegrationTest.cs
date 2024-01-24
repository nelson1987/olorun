using AutoFixture;
using Olorun.Integration.Configs.Fixtures;
using Xunit.Categories;

namespace Olorun.Integration
{
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