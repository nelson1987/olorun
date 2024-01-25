using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Olorun.Integration.Configs;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
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

public class IntegrationTests1 : IClassFixture<OlorunApiFixture>
{
    private readonly OlorunApiFixture _olorunApiFixture;

    public IntegrationTests1(OlorunApiFixture olorunApiFixture)
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
public class UnitTest1
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

    private readonly CadastroContaCommandHandler _controller;
    private readonly CadastroContaCommand _request;
    private readonly Mock<IValidator<CadastroContaCommand>> _validator;
    private readonly Mock<IHttpAntifraudeService> _antifraudeService;
    private readonly Mock<IContaRepository> contaRepository;
    private readonly Mock<IContaProducer> contaProducer;

    public UnitTest1()
    {
        _request = _fixture.Build<CadastroContaCommand>()
            .Create();

        _validator = _fixture.Freeze<Mock<IValidator<CadastroContaCommand>>>();
        _validator
            .Setup(x => x.Validate(It.IsAny<CadastroContaCommand>()))
            .Returns(true);

        _antifraudeService = _fixture.Freeze<Mock<IHttpAntifraudeService>>();
        _antifraudeService
            .Setup(x => x.Get(It.IsAny<string>()))
            .Returns(true);

        contaRepository = _fixture.Freeze<Mock<IContaRepository>>();
        contaProducer = _fixture.Freeze<Mock<IContaProducer>>();

        _controller = _fixture.Build<CadastroContaCommandHandler>()
            .Create();
    }

    [Fact]
    public void Dado_Request_Valido_Deve_Retornar_True()
    {
        // Act
        var result = _controller.Handle(_request);

        // Assert
        Assert.True(result);
    }
    [Fact]
    public void Dado_Request_Invalido_Deve_Retornar_False()
    {
        _validator
            .Setup(x => x.Validate(It.IsAny<CadastroContaCommand>()))
            .Returns(false);

        // Act
        var result = _controller.Handle(_request);

        // Assert
        Assert.False(result);
    }
    [Fact]
    public void Dado_Documento_Invalido_Antifraude_Deve_Retornar_False()
    {
        _antifraudeService
            .Setup(x => x.Get(It.IsAny<string>()))
            .Returns(false);

        // Act
        var result = _controller.Handle(_request);

        // Assert
        Assert.False(result);
    }
    [Fact]
    public void Dado_Insercao_Persistencia_Erro_Deve_Retornar_False()
    {
        contaRepository
         .Setup(x => x.Insert(It.IsAny<Conta>()))
         .Throws<Exception>();

        // Act
        var result = _controller.Handle(_request);

        // Assert
        Assert.False(result);
    }
    [Fact]
    public void Dado_Publicacao_Evento_Erro_Deve_Retornar_False()
    {
        contaProducer
         .Setup(x => x.Send(It.IsAny<CadastroContaCommandEvent>()))
         .Throws<Exception>();
        // Act
        var result = _controller.Handle(_request);

        // Assert
        Assert.False(result);
    }
}
