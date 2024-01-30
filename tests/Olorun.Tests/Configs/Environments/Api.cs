using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using SharedDomain.Features.WeatherForecasts.Create;
using SharedDomain.Shared;
using System.Diagnostics;

namespace Olorun.Tests.Configs.Environments;

public class Api : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
        => builder.UseEnvironment("Test")
               .ConfigureTestServices(services =>
               {
                   // services.AddScoped<IWeatherForecastHandler, WeatherForecastHandler>();
                   services.AddScoped<IEventConsumer<CreateWeatherForecastEvent>, CreateWeatherForecastConsumer>();
                   services.AddScoped<IEventProducer<CreateWeatherForecastEvent>, CreateWeatherForecastProducer>();
               });

    internal Task Consume<TConsumer>(TimeSpan? timeout = null) where TConsumer : IEvent
    {
        const int defaultTimeoutInSeconds = 1;
        timeout ??= TimeSpan.FromSeconds(defaultTimeoutInSeconds);

        using var scope = Services.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IEventConsumer<TConsumer>>();
        if (Debugger.IsAttached)
            return consumer.Consume(CancellationToken.None);

        using var tokenSource = new CancellationTokenSource(timeout.Value);
        return consumer.Consume(tokenSource.Token);
    }
}