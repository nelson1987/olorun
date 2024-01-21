
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//Mongo
builder.Services.Configure<BookStoreDatabaseSettings>(
    builder.Configuration.GetSection("BookStoreDatabase"));
builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<IWeatherForecastHandler, WeatherForecastHandler>();
//Redis

//Kafka
builder.Services.AddMassTransit(x =>
{
    x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
    x.AddRider(rider =>
    {
        rider.AddProducer<CreateWeatherForecastEvent>("weatherforecast-requested");
        rider.AddConsumer<CreateWeatherForecastConsumer>();
        rider.AddProducer<DeleteWeatherForecastEvent>("weatherforecast-requested");
        rider.AddConsumer<DeleteteWeatherForecastConsumer>();
        rider.UsingKafka((context, k) =>
        {
            k.Host("kafka:9092");
            k.TopicEndpoint<CreateWeatherForecastEvent>("weatherforecast-requested", "consumer-group-name", e =>
            {
                e.ConfigureConsumer<CreateWeatherForecastConsumer>(context);
            });
            k.TopicEndpoint<DeleteWeatherForecastEvent>("weatherforecast-requested", "consumer-group-name", e =>
            {
                e.ConfigureConsumer<DeleteteWeatherForecastConsumer>(context);
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

app.MapGet("/weatherforecast", async ([FromServices] IWeatherForecastHandler repository) =>
{
    return await repository.GetAsync();
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapPut("/weatherforecast", async ([FromServices] IWeatherForecastHandler repository) =>
{
    return await repository.PutAsync();
})
.WithName("PutWeatherForecast")
.WithOpenApi();

app.MapPost("/weatherforecast", async ([FromServices] IWeatherForecastHandler repository) =>
{
    await repository.PostAsync();
})
.WithName("PostWeatherForecast")
.WithOpenApi();
app.MapDelete("/weatherforecast", async ([FromServices] IWeatherForecastHandler repository) =>
{
    await repository.DeleteAsync();
})
.WithName("DeleteWeatherForecast")
.WithOpenApi();

app.Run();

public interface IWeatherForecastHandler
{
    Task<IList<WeatherForecast>> GetAsync();
    Task<WeatherForecast> PutAsync();
    Task PostAsync();
    Task DeleteAsync();
}
public class WeatherForecastHandler : IWeatherForecastHandler
{
    private string[] summaries = new[]
    {
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};
    private readonly IRepository _repository;
    private readonly ITopicProducer<CreateWeatherForecastEvent> _bus;

    public WeatherForecastHandler(IRepository repository, ITopicProducer<CreateWeatherForecastEvent> bus)
    {
        _repository = repository;
        _bus = bus;
    }

    public async Task<IList<WeatherForecast>> GetAsync()
    {
        return await _repository.GetAsync();
    }

    public async Task PostAsync()
    {
        CreateWeatherForecastEvent @event = new CreateWeatherForecastEvent()
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
            Id = Guid.NewGuid(),
            Summary = summaries[Random.Shared.Next(summaries.Length)],
            TemperatureC = Random.Shared.Next(-20, 55)
        };
        await _bus.Produce(@event);
    }

    public async Task<WeatherForecast> PutAsync()
    {
        var climaAsync = await _repository.GetAsync();
        var clima = climaAsync.First() with
        {
            Date =
                DateOnly.FromDateTime(DateTime.Now.AddDays(2))
        };
        await _repository.UpdateAsync(climaAsync.First().Id, clima);
        return clima;
    }

    public async Task DeleteAsync()
    {
        var climaAsync = await _repository.GetAsync();
        DeleteWeatherForecastEvent @event = new DeleteWeatherForecastEvent()
        {
            Id = climaAsync.First().Id
        };
        await _bus.Produce(@event);
    }

}