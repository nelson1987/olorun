using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace Olorun.Integration.Configs.Environments;
public class Api : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
        => builder.UseEnvironment("Test")
               .ConfigureTestServices(services =>
               {
                   // services.AddScoped<IWeatherForecastHandler, WeatherForecastHandler>();
               });

    public Task Consume<T>(TimeSpan timeSpan)
    {
        throw new NotImplementedException();
    }
}