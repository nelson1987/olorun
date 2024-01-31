using AutoFixture;
using FluentAssertions;
using FluentResults;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SharedDomain;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit.Categories;

namespace TddCourse
{
    #region Fixtures
    public class IntegrationTestFixture : IAsyncLifetime
    {
        public ApiFixture ApiFixture { get; }
        public KafkaFixture KafkaFixture { get; }
        public HttpServerFixture HttpServerFixture { get; }
        public MongoFixture MongoFixture { get; }

        public IntegrationTestFixture()
        {
            ApiFixture = new ApiFixture();
            KafkaFixture = new KafkaFixture(ApiFixture.Server);
            HttpServerFixture = new HttpServerFixture();
            //MongoFixture = new MongoFixture(ApiFixture.Server);
        }
        public async Task DisposeAsync()
        {
            await ApiFixture.DisposeAsync();
            HttpServerFixture.Dispose();
        }

        public async Task InitializeAsync()
        {
            await KafkaFixture.WarmUp();
        }
    }
    public class ApiFixture : IAsyncDisposable
    {
        public Api Server { get; } = new();
        public HttpClient Client { get; }
        public ApiFixture()
        {
            Client = Server.CreateDefaultClient();
        }

        public void Reset()
        {
            Client.DefaultRequestHeaders.Clear();

            Client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(scheme: "TestScheme");
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await Server.DisposeAsync();
        }
    }
    public class KafkaFixture
    {
        internal static readonly string[] Topics = GetKafkaTopicNames();
        private readonly IEventClient _eventClient;
        private readonly IEventClientConsumers _eventClientConsumers;

        public KafkaFixture(Api server)
        {
            _eventClient = server.Services.GetRequiredService<IEventClient>();
            _eventClientConsumers = server.Services.GetRequiredService<IEventClientConsumers>();
        }
        public async Task WarmUp()
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            foreach (var topic in Topics)
            {
                while (!cts.IsCancellationRequested)
                {
                    await Produce(topic, new object());
                    var response = Consume<object>(topic, msTimeout: 5);
                    if (response.IsSuccess)
                        break;
                }
            }

            if (cts.IsCancellationRequested)
                throw new TimeoutException("Unable to warm up Kafka.");
        }

        public void Reset()
        {
            ClearTopicsMessages();
        }

        private void ClearTopicsMessages()
        {
            foreach (var topic in Topics)
                ConsumeAll<object>(topic, msTimeout: 5);
        }

        public Task<bool> Produce<T>(string topic, T data)
        {
            using var cancellationToken = ExpiringCancellationToken();
            return _eventClient.Produce(
                    new EventClientRequest<T>(data, topic, "subject"),
                    obj => JsonConvert.SerializeObject(obj, new StringEnumConverter()),
                    cancellationToken.Token);
        }

        public Result<T> Consume<T>(string topic, int msTimeout = 150)
        {
            try
            {
                if (typeof(T) == typeof(object))
                {
                    // Quando utilizamos o parâmetro de timeout do tipo int
                    // no consumo do kafka temos um ganho significativo
                    // de performance no método ClearTopicsMessages
                    // comparado ao timeout através de cancellationToken
                    var consumeResult = _eventClientConsumers
                        .Get(topic)
                        .Consume(msTimeout);
                    //
                    var isFailed = consumeResult == null || consumeResult.IsPartitionEOF;
                    return isFailed ? Result.Fail("A timeout has ocurred or end of partition found") : (T)new object();
                }

                using var cancellationToken = ExpiringCancellationToken(msTimeout);
                var message = _eventClient.Consume(topic, JsonConvert.DeserializeObject<T>, cancellationToken.Token);

                return message is not null
                    ? Result.Ok(message)
                    : Result.Fail<T>("no messages found");
            }
            catch (OperationCanceledException)
            {
                return Result.Fail<T>("no messages found");
            }
        }

        public IReadOnlyList<T> ConsumeAll<T>(string topic, int msTimeout = 150)
        {
            var messages = new List<T>();
            while (true)
            {
                var result = Consume<T>(topic, msTimeout);
                if (result.IsSuccess)
                {
                    messages.Add(result.Value);
                    continue;
                }

                break;
            }

            return messages;
        }

        private static CancellationTokenSource ExpiringCancellationToken(int msTimeout = 150)
        {
            var timeout = TimeSpan.FromMilliseconds(msTimeout);
            return new CancellationTokenSource(timeout);
        }

        private static string[] GetKafkaTopicNames()
        {
            return typeof(EventsTopics)
                .GetTypeInfo()
                .DeclaredNestedTypes
                .Select(x => x.GetField("Name")!.GetValue(null))
                .Cast<string>()
                .ToArray();
        }
    }
    public class HttpServerFixture : IDisposable
    {
        public WireMockServer ContaServiceServer { get; } = WireMockServer.Start(port: 9080);
        public WireMockServer ClienteServiceServer { get; } = WireMockServer.Start(port: 9081);
        public WireMockServer AntifraudeServiceServer { get; } = WireMockServer.Start(port: 9082);

        public void Reset()
        {
            ContaServiceServer.Reset();
            ClienteServiceServer.Reset();
            AntifraudeServiceServer.Reset();
        }
        public void Dispose()
        {
            ContaServiceServer.Dispose();
            ClienteServiceServer.Dispose();
            AntifraudeServiceServer.Dispose();
        }

    }
    public class MongoFixture
    {
        public IMongoDatabase MongoDatabase { get; }

        public MongoFixture(Api server)
        {
            var configuration = server.Services.GetRequiredService<IConfiguration>();
            var mongoUrl = new MongoUrl(configuration.GetConnectionString("MongoDB"));
            var mongoClient = new MongoClient(mongoUrl);
            MongoDatabase = mongoClient.GetDatabase(mongoUrl.DatabaseName);
        }
        public async Task Reset()
        {
            var collectionNames = MongoDatabase.ListCollectionNames();
            while (await collectionNames.MoveNextAsync())
            {
                foreach (var collectionName in collectionNames.Current)
                {
                    await MongoDatabase
                        .GetCollection<BsonDocument>(collectionName)
                        .DeleteManyAsync(_ => true);
                }
            }
        }
    }
    #endregion

    #region Integration
    public class Api : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
            => builder.UseEnvironment("Test")
                   .ConfigureTestServices(services =>
                   {
                   });

        internal Task Consume<TConsumer>(TimeSpan? timeout = null) where TConsumer : IConsumer
        {
            const int defaultTimeoutInSeconds = 1;
            timeout ??= TimeSpan.FromSeconds(defaultTimeoutInSeconds);

            using var scope = Services.CreateScope();
            var consumer = scope.ServiceProvider.GetRequiredService<TConsumer>();
            if (Debugger.IsAttached)
                return consumer.Consume(CancellationToken.None);

            using var tokenSource = new CancellationTokenSource(timeout.Value);
            return consumer.Consume(tokenSource.Token);
        }

        private static void ConfigureConsumers(IServiceCollection services)
        {
            var workerTypes = Assembly
                .GetAssembly(typeof(IConsumer))!
                .GetTypes()
                .Where(t => t.GetInterfaces().Contains(typeof(IConsumer)))
                .Where(t => !t.IsAbstract);

            foreach (var workerType in workerTypes)
                services.AddScoped(workerType);
        }
    }

    [CollectionDefinition(nameof(IntegrationCollection))]
    public class IntegrationCollection : ICollectionFixture<IntegrationTestFixture>
    {
    }

    [Collection(nameof(IntegrationCollection))]
    [IntegrationTest]
    public abstract class IntegrationTests : IAsyncLifetime
    {
        public Fixture ObjectGenerator { get; } = new Fixture();
        public ApiFixture ApiFixture { get; }
        public KafkaFixture KafkaFixture { get; }
        public HttpServerFixture HttpServerFixture { get; }
        public MongoFixture MongoFixture { get; }
        protected IntegrationTests(IntegrationTestFixture integrationTestFixture)
        {
            ApiFixture = integrationTestFixture.ApiFixture;
            KafkaFixture = integrationTestFixture.KafkaFixture;
            HttpServerFixture = integrationTestFixture.HttpServerFixture;
            //MongoFixture = integrationTestFixture.MongoFixture;
        }

        public async Task InitializeAsync()
        {
            ApiFixture.Reset();
            KafkaFixture.Reset();
            HttpServerFixture.Reset();
            //await MongoFixture.Reset();
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }
    #endregion

    public class ApiIntegrationTests : IntegrationTests
    {
        public ApiIntegrationTests(IntegrationTestFixture integrationTestFixture) : base(integrationTestFixture)
        {
        }

        [Fact]
        public async Task Test1()
        {
            // Arrange
            var pessoa = PessoaFactory.Create();

            await MongoFixture.MongoDatabase
                .GetCollection<Pessoa>(nameof(Pessoa))
                .InsertOneAsync(pessoa);

            var request = new Pessoa(Guid.Parse("da334215-c4eb-4e57-a4a4-80e349242369"), "Nome");

            var expectedResponse = Result.Fail("Credit Note not found");
            // Act
            using var httpResponse = await ApiFixture.Client.PostAsJsonAsync($"/settlements/{request.Id}/total", request);

            // Assert
            //var response = await httpResponse.Content.ReadFromJsonAsync<Pessoa>();
            //response.Should().BeEquivalentTo(expectedResponse);
        }
    }

    public class ConsumerIntegrationTests : IntegrationTests
    {
        private readonly OrdersRenegotiationRespondedEvent _request;
        public ConsumerIntegrationTests(IntegrationTestFixture integrationTestFixture) : base(integrationTestFixture)
        {
            _request = ObjectGenerator
                .Build<OrdersRenegotiationRespondedEvent>()
                .Create();
        }

        [Fact]
        public async Task Test1()
        {
            // Arrange
            await KafkaFixture.Produce(EventsTopics.WeatherforecastResponded.Name, _request);

            // Act
            await ApiFixture.Server.Consume<CashReserveConsumer>(TimeSpan.FromMinutes(1));
            var cashReservesResponded = KafkaFixture.Consume<OrdersRenegotiationRespondedEvent>(EventsTopics.WeatherforecastResponded.Name).ValueOrDefault;

            // Assert
            // var response = GetOutputedResponses();
        }
        //private object GetOutputedResponses()
        //{
        //    var cashReservesResponded = KafkaFixture.Consume<CashReserveResponse>(EventsTopics.CashReservesResponded.Name).ValueOrDefault;
        //    var creditFundingService = HttpServerFixture.AntifraudeServiceServer.LogEntries.Select(x => x.RequestMessage).ToList();

        //    foreach (var request in creditFundingService)
        //    {
        //        request.Headers!.Remove("Integrated-TraceStack");
        //        request.Headers!.Remove("IntegratedWebService-TraceStack");
        //        request.Headers!.Remove("traceparent");
        //    }

        //    return new
        //    {
        //        cashReservesResponded,
        //        creditFundingService
        //    };
        //}
    }

    public class ServerIntegrationTests : IntegrationTests
    {
        private readonly CashReserveRequest _request;
        public ServerIntegrationTests(IntegrationTestFixture integrationTestFixture) : base(integrationTestFixture)
        {
        }

        [Fact]
        public async Task Test1()
        {
            // Arrange
            HttpServerFixture.AntifraudeServiceServer
                .Given(Request.Create()
                    .WithPath("/api/v1/webhook/treasury/updateOrder")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK));

            await KafkaFixture.Produce("", _request);

            // Act
            await ApiFixture.Server.Consume<CashReserveConsumer>();

            // Assert
            var response = GetOutputedResponses();
        }
        private object GetOutputedResponses()
        {
            var cashReservesResponded = KafkaFixture.Consume<CashReserveResponse>(EventsTopics.WeatherforecastResponded.Name).ValueOrDefault;
            var creditFundingService = HttpServerFixture.AntifraudeServiceServer.LogEntries.Select(x => x.RequestMessage).ToList();

            foreach (var request in creditFundingService)
            {
                request.Headers!.Remove("Integrated-TraceStack");
                request.Headers!.Remove("IntegratedWebService-TraceStack");
                request.Headers!.Remove("traceparent");
            }

            return new
            {
                cashReservesResponded,
                creditFundingService
            };
        }
    }

    public class DatabaseIntegrationTests : IntegrationTests
    {
        private readonly IPessoaReadRepository _repository;
        public DatabaseIntegrationTests(IntegrationTestFixture integrationTestFixture) : base(integrationTestFixture)
        {
            _repository = ApiFixture.Server.Services.GetRequiredService<IPessoaReadRepository>();
        }

        [Fact]
        public async Task Test1()
        {
            // Arrange
            var pessoa = PessoaFactory.Create();

            await MongoFixture.MongoDatabase
                .GetCollection<Pessoa>(nameof(Pessoa))
                .InsertOneAsync(pessoa);

            // Act
            var result = await _repository.GetById(pessoa.Id);

            // Assert
            result.Should().BeEquivalentTo(pessoa);
        }
    }
}