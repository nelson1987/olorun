using AutoFixture;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SharedDomain.Integrations;
using System.Net.Http.Headers;
using WireMock.ResponseBuilders;
using Xunit.Sdk;

namespace TddCourse.Integrations;

public static class ServiceCollectionMockExtensions
{
    internal static IServiceCollection ConfigureKafkaServices(this IServiceCollection services)
    {
        //var settings = new EventClientSettings("localhost:9092", (string?)null)
        //{
        //    ConsumerTopics = KafkaFixture.Topics,
        //    ConsumerGroup = "integration-tests-credit-notes"
        //};

        return services
            .RemoveAll<IEventClient>()
            .RemoveAll<IEventClientProducer>()
            .AddEventClientSettings();
    }
}
public static class EventClientConfigure
{
    /// <summary>
    /// Configures the Messaging Lib in the current project
    /// </summary>
    /// <param name="services">The "ServiceCollection"(DI) to configure and inject the messaging lib services that will be needed to generate and consume events.</param>
    /// <param name="eventClientSettings">This object has all needed settings to configure the event communication.</param>
    /// <returns>The "ServiceCollection" to provide a fluent api.</returns>
    public static IServiceCollection AddEventClientSettings(this IServiceCollection services)
    {
        return services
            .AddSingleton<IEventClient, EventClient>()
            .AddSingleton<IEventClientProducer, EventClientProducer>();
    }
}
/*
//public class IntegrationApi : WebApplicationFactory<Program>
//{
//    protected override void ConfigureWebHost(IWebHostBuilder builder)
//        => builder.UseEnvironment("Test")
//               .ConfigureTestServices(services =>
//               {
//                   //services.ConfigureKafkaServices();
//                   //services.AddSingleton<ICriacaoPessoaHandler, CriacaoPessoaHandler>()
//                   //        .AddSingleton<IClienteApiEventProducer, ClienteApiEventProducer>()
//                   //        .AddSingleton<IEventCommunicationService, EventCommunicationService>()
//                   //        .AddSingleton<IEventClient, EventClient>()
//                   //        .AddSingleton<IEventClientProducer, EventClientProducer>();
//               });
//}
public sealed class KafkaFixture
{
    private readonly IEventClient _eventClient;

    public KafkaFixture(IntegrationApi server)
    {
        _eventClient = server.Services.GetRequiredService<IEventClient>();
    }

    public Task<bool> Produce<T>(string topic, T data)
    {
        using var cancellationToken = ExpiringCancellationToken();
        return _eventClient.Produce(
                new EventClientRequest<T>(data, topic, "subject"),
                obj => Newtonsoft.Json.JsonConvert.SerializeObject(obj, new Newtonsoft.Json.Converters.StringEnumConverter()),
                cancellationToken.Token);
    }

    private static CancellationTokenSource ExpiringCancellationToken(int msTimeout = 150)
    {
        var timeout = TimeSpan.FromMilliseconds(msTimeout);
        return new CancellationTokenSource(timeout);
    }
    public void Reset()
    {
        //ClearTopicsMessages();
    }
}

public sealed class ApiFixture : IAsyncDisposable
{
    public IntegrationApi Server { get; } = new();
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
        //await Server.DisposeAsync();
    }
}

public class IntegrationTestFixture : IAsyncLifetime
{
    public ApiFixture ApiFixture { get; }
    public KafkaFixture KafkaFixture { get; }

    public IntegrationTestFixture()
    {
        ApiFixture = new ApiFixture();
        KafkaFixture = new KafkaFixture(ApiFixture.Server);
    }

    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
        //await KafkaFixture.WarmUp();
    }

    public async Task DisposeAsync()
    {
        await ApiFixture.DisposeAsync();
    }
}

[TraitDiscoverer("Xunit.Categories.IntegrationTestDiscoverer", "Xunit.Categories")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class IntegrationTestAttribute : Attribute, ITraitAttribute
{
}

[CollectionDefinition(nameof(IntegrationCollection))]
public class IntegrationCollection : ICollectionFixture<IntegrationTestFixture>
{
}

[Collection(nameof(IntegrationCollection))]
[IntegrationTest]
public abstract class IntegrationTest : IAsyncLifetime
{
    public Fixture ObjectGenerator { get; } = new Fixture();
    protected ApiFixture ApiFixture { get; }
    protected KafkaFixture KafkaFixture { get; }

    protected IntegrationTest(IntegrationTestFixture integrationTestFixture)
    {
        ApiFixture = integrationTestFixture.ApiFixture;
        KafkaFixture = integrationTestFixture.KafkaFixture;
    }
    public virtual async Task InitializeAsync()
    {
        ApiFixture.Reset();
        KafkaFixture.Reset();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
*/
public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            builder.UseSolutionRelativeContentRoot("src/MyProduct.Web/");
        });
    }
}
public class IntegrationCourseTests : IClassFixture<CustomWebApplicationFactory<Program>> //: IntegrationTest
{
    //public IntegrationCourseTests(IntegrationTestFixture integrationTestFixture) : base(integrationTestFixture)
    //{
    //}
    private readonly CustomWebApplicationFactory<Program> _webApplicationFactory;
    public HttpClient _client { get; set; }

    public IntegrationCourseTests(CustomWebApplicationFactory<Program> webApplicationFactory)
    {
        this._webApplicationFactory = webApplicationFactory;
        _client = _webApplicationFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task GetAsyncTests()
    {
        // Arrange
        var _request = new PessoaCriadaEvent();
        // Act
        var response = await _client.GetAsync("api/v1/weatherforecasts");
        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        Assert.Equal("text/html; charset=utf-8",
            response.Content.Headers.ContentType.ToString());
    }
    [Fact]
    public async Task PostAsyncTests()
    {
        // Arrange
        var _request = new PessoaCriadaEvent();
        // Act
        var response = await _client.PostAsync("api/v1/weatherforecasts");
        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        Assert.Equal("text/html; charset=utf-8",
            response.Content.Headers.ContentType.ToString());
        //await KafkaFixture.Produce("pessoa-criada-request", _request);

        // Act
        //IEventProducer<CreateWeatherForecastEvent>
        //await ApiFixture.Server.Consume<CashReserveConsumer>();
        //KafkaFixture.Consume<CashReserveResponse>("pessoa-criada-responded").ValueOrDefault;

    }
}
