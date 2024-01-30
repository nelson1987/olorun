using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Olorun.Tests.Configs.Environments;

namespace Olorun.Tests.Configs.Fixtures;

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
//public interface IEventClientConsumers { }