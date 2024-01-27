using SharedDomain.Shared;

namespace SharedDomain.Features.Pagamentos.Inclusao;

public record InclusaoPagamentoCommand : ICommand
{
    public Guid Id { get; set; }
    public required string ContaDebitada { get; set; }
    public required string ContaCreditada { get; set; }
    public decimal Valor { get; set; }
    public PagamentoIncluidoType Type { get; set; }
}
