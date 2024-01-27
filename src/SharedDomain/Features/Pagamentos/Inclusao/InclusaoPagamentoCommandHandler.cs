using FluentResults;
using FluentValidation;
using SharedDomain.Features.Pagamentos.Events.Incluido;
using SharedDomain.Shared;

namespace SharedDomain.Features.Pagamentos.Inclusao;

public class InclusaoPagamentoCommandHandler : CommandHandler<InclusaoPagamentoCommand, Result<Pagamento?>>
{
    private readonly IValidator<InclusaoPagamentoCommand> _validator;
    private readonly IEventProducer<PagamentoIncluidoEvent> _eventProducer;
    private readonly IPagamentoReadRepository _repository;

    public InclusaoPagamentoCommandHandler(IValidator<InclusaoPagamentoCommand> validator,
        IEventProducer<PagamentoIncluidoEvent> pagamentoCriadoEvent,
        IPagamentoReadRepository repository)
    {
        _validator = validator;
        _eventProducer = pagamentoCriadoEvent;
        _repository = repository;
    }

    public override async Task<Result<Pagamento?>> Handle(InclusaoPagamentoCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);
        if (validation.IsInvalid())
            return validation.ToFailResult();

        await _repository.CreateAsync(command.MapTo<Pagamento>(), cancellationToken);
        await _eventProducer.Send(command.MapTo<PagamentoIncluidoEvent>(), cancellationToken);
        return Result.Ok();
    }
}
