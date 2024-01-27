using SharedDomain.Shared;

namespace SharedDomain.Features.Pagamentos.Events.Incluido;

public class PagamentoIncluidoEvent : IEvent
{
    public Guid Id { get; set; }
    public PagamentoIncluidoType Type { get; set; }
}
