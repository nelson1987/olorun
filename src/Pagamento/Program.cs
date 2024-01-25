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
app.AddweatherforecastEndpoints();

app.Run();
public partial class Program { }