using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Olorun.Integration.Configs.Environments;

namespace Olorun.Integration.Configs.Fixtures
{
    public sealed class ApiFixture : IAsyncDisposable
    {
        public Api Server { get; } = new();
        public HttpClient Client { get; }

        public ApiFixture()
        {
            Client = Server.CreateDefaultClient();
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
    public sealed class KafkaFixture
    {
        private readonly IEventClient _eventClient;
        public KafkaFixture(Api server)
        {
            _eventClient = server.Services.GetRequiredService<IEventClient>();
        }
    }
    public sealed class MongoFixture
    {
        public IMongoDatabase MongoDatabase { get; }
        public MongoFixture(Api server)
        {
            var configuration = server.Services.GetRequiredService<IConfiguration>();
            var mongoUrl = new MongoUrl(configuration.GetConnectionString("MongoDB"));
            var mongoClient = new MongoClient(mongoUrl);
            MongoDatabase = mongoClient.GetDatabase(mongoUrl.DatabaseName);
        }
    }
    public interface IEventClient { }
}