using FluentResults;

namespace Olorun.Tests.Entities
{
    public interface IInclusaoAlunoHandler
    {
        Task<Result> Handle(InclusaoAlunoCommand request, CancellationToken cancellationToken);
    }

    public class InclusaoAlunoHandler : IInclusaoAlunoHandler
    {
        private readonly IValidator<InclusaoAlunoCommand> _validator;
        private readonly IAlunoWriteRepository _repository;
        private readonly IKafkaProducer _producer;
        public InclusaoAlunoHandler(IValidator<InclusaoAlunoCommand> validator,
                IAlunoWriteRepository repository,
                IKafkaProducer producer)
        {
            _validator = validator;
            _repository = repository;
            _producer = producer;
        }

        public async Task<Result> Handle(InclusaoAlunoCommand request, CancellationToken cancellationToken)
        {
            await _validator.ValidateAsync(request, cancellationToken);
            await _repository.Incluir(request, cancellationToken);
            await _producer.Send(new AlunoIncluidoEvent());
            return Result.Ok();
        }
    }

    public class KafkaConsumer { }
}