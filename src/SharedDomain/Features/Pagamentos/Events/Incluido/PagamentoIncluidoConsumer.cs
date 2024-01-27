using SharedDomain.Shared;

namespace SharedDomain.Features.Pagamentos.Events.Incluido;

public class PagamentoIncluidoConsumer : EventConsumer<PagamentoIncluidoEvent>
{
    private readonly IPagamentoReadRepository? _repository;
    private readonly IProducer<PagamentoSubmetidoEvent> _producer;

    public PagamentoIncluidoConsumer(IPagamentoReadRepository? repository, IProducer<PagamentoSubmetidoEvent> producer)
    {
        _repository = repository;
        _producer = producer;
    }

    public override async Task Consume(PagamentoIncluidoEvent @event, CancellationToken cancellationToken)
    {
        var lista = await _repository.GetAsync(cancellationToken);
        await _producer.Send(@event.MapTo<PagamentoSubmetidoEvent>());
    }
}