var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//Mongo
builder.Services.AddDependencies()
                .AddMongoDb()
                //Redis
                .AddKafka();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//app.UseHttpsRedirection();

app.MapGet("/weatherforecast", async ([FromServices] IWeatherForecastHandler handler) =>
{
    return await handler.GetAsync();
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapPut("/weatherforecast", async ([FromServices] IWeatherForecastHandler handler) =>
{
    return await handler.PutAsync();
})
.WithName("PutWeatherForecast")
.WithOpenApi();

app.MapPost("/weatherforecast", async ([FromServices] IWeatherForecastHandler handler) =>
{
    await handler.PostAsync();
})
.WithName("PostWeatherForecast")
.WithOpenApi();

app.MapDelete("/weatherforecast", async ([FromServices] IWeatherForecastHandler handler) =>
{
    await handler.DeleteAsync();
})
.WithName("DeleteWeatherForecast")
.WithOpenApi();

app.Run();