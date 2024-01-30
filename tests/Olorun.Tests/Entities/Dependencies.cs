using Microsoft.Extensions.DependencyInjection;

namespace Olorun.Tests.Entities
{
    public static class Dependencies
    {
        public static IServiceCollection AddProducers(this IServiceCollection services)
        {
            services.AddScoped<ICommand, InclusaoAlunoCommand>()
                    .AddScoped<IValidator<InclusaoAlunoCommand>, InclusaoAlunoCommandValidator>()
                    .AddScoped<IInclusaoAlunoHandler, InclusaoAlunoHandler>()
                    .AddScoped<IEvent, AlunoIncluidoEvent>()
                    .AddScoped<IKafkaProducer, KafkaProducer>()
                    .AddScoped<IEntity, Aluno>()
                    .AddScoped<IAlunoWriteRepository, AlunoWriteRepository>();
            return services;
        }
    }
}