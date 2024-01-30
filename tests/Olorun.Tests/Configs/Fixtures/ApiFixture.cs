using Olorun.Tests.Configs.Environments;
using System.Net.Http.Headers;

namespace Olorun.Tests.Configs.Fixtures;

public sealed class ApiFixture : IAsyncDisposable
{
    public Api Server { get; } = new();
    public HttpClient Client { get; }

    public ApiFixture()
    {
        Client = Server.CreateDefaultClient();
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await Server.DisposeAsync();
    }
    public void Reset()
    {
        Client.DefaultRequestHeaders.Clear();

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(scheme: "TestScheme");
    }
}