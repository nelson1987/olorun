using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Olorun.Integration.Configs;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Olorun.Integration;
public class OlorunApi : WebApplicationFactory<Program>
{
    static OlorunApi()
        => Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Test");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
        => builder.UseEnvironment("Test")
                  .ConfigureTestServices(services =>
                  {
                      services.AddScoped<ICadastroContaCommandHandler, CadastroContaCommandHandler>();
                      services.AddAuthentication(defaultScheme: "TestScheme")
                              .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });

                      //KafkaFixture.ConfigureKafkaServices(services);
                  });

    private class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[] {
                new Claim(ClaimTypes.Name, "Test user"),
                new Claim("preferred_username", "user@stone.com.br")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "TestScheme");

            var result = AuthenticateResult.Success(ticket);

            return Task.FromResult(result);
        }
    }
}
public class OlorunApiFixture
{
    private static readonly OlorunApi _server;
    private static readonly HttpClient _client;

    public OlorunApi Server => _server;
    public HttpClient Client => _client;

    static OlorunApiFixture()
    {
        _server = new();
        _client = _server.CreateDefaultClient();
    }

    public OlorunApiFixture()
    {
        _client.DefaultRequestHeaders.Clear();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(scheme: "TestScheme");
    }
}

//public sealed class ApiFixture : IAsyncDisposable
//{
//    public Api Server { get; } = new();
//    public HttpClient Client { get; }

//    public ApiFixture()
//    {
//        Client = Server.CreateDefaultClient();
//    }

//    public void Reset()
//    {
//        Client.DefaultRequestHeaders.Clear();

//        Client.DefaultRequestHeaders.Authorization =
//            new AuthenticationHeaderValue(scheme: "TestScheme");
//    }

//    public async ValueTask DisposeAsync()
//    {
//        Client.Dispose();
//        await Server.DisposeAsync();
//    }

//    public class Api : WebApplicationFactory<Program>
//    {
//        static Api()
//            => Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Test");
//        protected override void ConfigureWebHost(IWebHostBuilder builder)
//            => builder.UseEnvironment("Test")
//                   .ConfigureTestServices(services =>
//                   {
//                       services.AddMockedAzureAdCredentials();
//                       services.AddMockedApiAuthentication();
//                       services.Configure<HealthCheckServiceOptions>(options =>
//                       {
//                           options.Registrations.Clear();
//                       });
//                       services.ConfigureKafkaServices();
//                       ConfigureConsumers(services);
//                   });

//        internal Task Consume<TConsumer>(TimeSpan? timeout = null) where TConsumer : IConsumer
//        {
//            const int defaultTimeoutInSeconds = 1;
//            timeout ??= TimeSpan.FromSeconds(defaultTimeoutInSeconds);

//            using var scope = Services.CreateScope();
//            var consumer = scope.ServiceProvider.GetRequiredService<TConsumer>();
//            if (Debugger.IsAttached)
//                return consumer.Consume(CancellationToken.None);

//            using var tokenSource = new CancellationTokenSource(timeout.Value);
//            return consumer.Consume(tokenSource.Token);
//        }

//        private static void ConfigureConsumers(IServiceCollection services)
//        {
//            var workerTypes = Assembly
//                .GetAssembly(typeof(IConsumer))!
//                .GetTypes()
//                .Where(t => t.GetInterfaces().Contains(typeof(IConsumer)))
//                .Where(t => !t.IsAbstract);

//            foreach (var workerType in workerTypes)
//                services.AddScoped(workerType);
//        }
//    }
//}

//public class MongoFixture
//{
//    public IMongoDatabase MongoDatabase { get; }

//    public MongoFixture(Api server)
//    {
//        var configuration = server.Services.GetRequiredService<IConfiguration>();
//        var mongoUrl = new MongoUrl(configuration.GetConnectionString("MongoDB"));
//        var mongoClient = new MongoClient(mongoUrl);
//        MongoDatabase = mongoClient.GetDatabase(mongoUrl.DatabaseName);
//    }

//    public async Task Reset()
//    {
//        var collectionNames = MongoDatabase.ListCollectionNames();
//        while (await collectionNames.MoveNextAsync())
//        {
//            foreach (var collectionName in collectionNames.Current)
//            {
//                await MongoDatabase
//                    .GetCollection<BsonDocument>(collectionName)
//                    .DeleteManyAsync(_ => true);
//            }
//        }
//    }
//}

//public class ConsumerTests 
//{
//    private readonly CadastroContaCommand _command;

//    [Fact]
//    public async Task Given_a_valid_bank_credit_note_settlement_should_output_expected_results()
//    {        
//        //await KafkaFixture.Produce(EventsTopics.SettlementOrdersCreated.Name, _settlementOrderEvent);
//        //await ApiFixture.Server.Consume<SettlementConsumer>(TimeSpan.FromMinutes(1));

//    }
//}
