using AutoMapper;
using SharedDomain.Features.Pagamentos.Events.Incluido;
using SharedDomain.Features.Pagamentos.Inclusao;

namespace SharedDomain.Features.Pagamentos;

public class PagamentoMapping : Profile
{
    public PagamentoMapping()
    {
        CreateMap<InclusaoPagamentoCommand, Pagamento>();
        CreateMap<InclusaoPagamentoCommand, PagamentoIncluidoEvent>();
    }
}
