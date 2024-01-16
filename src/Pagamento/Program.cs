using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Pagamento.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//Mongo
builder.Services.Configure<BookStoreDatabaseSettings>(
    builder.Configuration.GetSection("BookStoreDatabase"));
builder.Services.AddSingleton<IRepository, Repository>();
//Redis
//Kafka
builder.Services.AddMassTransit(x =>
{
    x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
    x.AddRider(rider =>
    {
        rider.AddConsumer<KafkaMessageConsumer>();
        rider.UsingKafka((context, k) =>
        {
            k.Host("kafka:9092");
            k.TopicEndpoint<WeatherForecastEvent>("weatherforecast-requested", "consumer-group-name", e =>
            {
                e.ConfigureConsumer<KafkaMessageConsumer>(context);
            });
        });
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", async ([FromServices] IRepository repository
    , [FromServices] IPublishEndpoint bus) =>
{
    var clima = new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        );
    await bus.Publish<WeatherForecastEvent>(new ()
    {
        Date = clima.Date,
        Id = clima.Id,
        Summary = clima.Summary,
        TemperatureC = clima.TemperatureC
    });
    return await repository.GetAsync();
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapPost("/weatherforecast", async ([FromServices] IRepository repository) =>
{
    var clima = new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        );
    await repository.CreateAsync(clima);
})
.WithName("PostWeatherForecast")
.WithOpenApi();

app.Run();
