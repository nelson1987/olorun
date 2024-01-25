using Microsoft.AspNetCore.Http.HttpResults;
using Pagamento.Features;
using Pagamento.Features.Entities;

namespace Pagamento.Services
{
    public static class Pagamento
    {
        public static WebApplication AddweatherforecastEndpoints(this WebApplication app)
        {
            app.MapGet("/weatherforecast", GetWeatherForecast)
            .WithName("GetWeatherForecast")
            .WithOpenApi();

            app.MapPut("/weatherforecast", GetWeatherForecastById)
            .WithName("PutWeatherForecast")
            .WithOpenApi();

            app.MapPost("/weatherforecast", async ([FromServices] IWeatherForecastHandler handler, CancellationToken cancellationToken) =>
            {
                await handler.PostAsync(cancellationToken);
            })
            .WithName("PostWeatherForecast")
            .WithOpenApi();

            app.MapDelete("/weatherforecast", async ([FromServices] IWeatherForecastHandler handler, Guid idClima, CancellationToken cancellationToken) =>
            {
                await handler.DeleteAsync(idClima, cancellationToken);
            })
            .WithName("DeleteWeatherForecast")
            .WithOpenApi();

            return app;
        }
        static async Task<Results<Ok<IList<WeatherForecast>>, NotFound>> GetWeatherForecast(IWeatherForecastHandler handler, CancellationToken cancellationToken) =>
            await handler.GetAsync(cancellationToken) is IList<WeatherForecast> todo
            ? TypedResults.Ok(todo)
            : TypedResults.NotFound();

        static async Task<Results<Ok<WeatherForecast>, NotFound>> GetWeatherForecastById(IWeatherForecastHandler handler, Guid idClima, CancellationToken cancellationToken) =>
            await handler.PutAsync(idClima, cancellationToken) is WeatherForecast todo
            ? TypedResults.Ok(todo)
            : TypedResults.NotFound();
    }
}
