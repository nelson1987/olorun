using SharedDomain.Shared;

namespace SharedDomain.Features.Pagamentos.Events.Incluido;
public class PagamentoSubmetidoEventProducer : EventProducer<PagamentoSubmetidoEvent>
{
    public Task Send(PagamentoSubmetidoEvent @event)
    {
        throw new NotImplementedException();
    }
}
public class PagamentoIncluidoConsumer : EventConsumer<PagamentoIncluidoEvent>
{
    private readonly IPagamentoReadRepository _repository;
    private readonly EventProducer<PagamentoSubmetidoEvent> _producer;

    public PagamentoIncluidoConsumer(IPagamentoReadRepository repository, EventProducer<PagamentoSubmetidoEvent> producer)
    {
        _repository = repository;
        _producer = producer;
    }

    //public override async Task Consume(PagamentoIncluidoEvent @event)
    //{
    //    CancellationToken cancellationToken = default(CancellationToken);
    //    var lista = await _repository.GetAsync(cancellationToken);
    //    await _producer.Send(@event.MapTo<PagamentoSubmetidoEvent>(), cancellationToken);
    //}
}