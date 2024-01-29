using SharedDomain.Features.WeatherForecasts;
using SharedDomain.Features.WeatherForecasts.Entities;

namespace Pagamento.Services
{
    public static class Controllers
    {
        public static IEndpointRouteBuilder AddWeatherForecastEndpoints(this IEndpointRouteBuilder app)
        {
            var todoItems = app.MapGroup("api/v1/weatherforecasts")
                    //.RequireAuthorization()
                    .WithTags("Weatherforecast");

            todoItems.MapGet("/", GetWeatherForecast)
            .WithName("GetWeatherForecast")
            .WithOpenApi();

            todoItems.MapPut("/{id}", GetWeatherForecastById)
            .WithName("PutWeatherForecast")
            .WithOpenApi();

            todoItems.MapPost("/", async ([FromServices] IWeatherForecastHandler handler, CancellationToken cancellationToken) =>
            {
                await handler.PostAsync(cancellationToken);
            })
            .WithName("PostWeatherForecast")
            .WithOpenApi();

            todoItems.MapDelete("/{id}", async ([FromServices] IWeatherForecastHandler handler, Guid id, CancellationToken cancellationToken) =>
            {
                await handler.DeleteAsync(id, cancellationToken);
            })
            .WithName("DeleteWeatherForecast")
            .WithOpenApi();

            return app;
        }
        static async Task<IResult> GetWeatherForecast(IWeatherForecastHandler handler, CancellationToken cancellationToken) =>
            await handler.GetAsync(cancellationToken) is IList<WeatherForecast> todo
            ? TypedResults.Ok(todo)
            : TypedResults.NotFound();

        static async Task<IResult> GetWeatherForecastById(Guid id, IWeatherForecastHandler handler, CancellationToken cancellationToken) =>
            await handler.PutAsync(id, cancellationToken) is WeatherForecast todo
            ? TypedResults.Ok(todo)
            : TypedResults.NotFound();
    }
}
