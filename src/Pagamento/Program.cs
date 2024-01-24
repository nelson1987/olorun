using Pagamento.Features;
using Pagamento.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//Mongo
builder.Services.AddDependencies()
                .AddMongoDb(builder.Configuration)
                //Redis
                .AddKafka();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//app.UseHttpsRedirection();

app.MapGet("/weatherforecast", async ([FromServices] IWeatherForecastHandler handler, CancellationToken cancellationToken) =>
{
    return await handler.GetAsync(cancellationToken);
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapPut("/weatherforecast", async ([FromServices] IWeatherForecastHandler handler, Guid idClima, CancellationToken cancellationToken) =>
{
    return await handler.PutAsync(idClima, cancellationToken);
})
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

app.Run();
public partial class Program { }