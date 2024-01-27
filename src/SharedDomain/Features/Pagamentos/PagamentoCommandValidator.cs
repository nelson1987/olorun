using FluentValidation;
using SharedDomain.Features.Pagamentos.Inclusao;

namespace SharedDomain.Features.Pagamentos;

public class PagamentoCommandValidator : AbstractValidator<InclusaoPagamentoCommand>
{
    public PagamentoCommandValidator()
    {
        RuleFor(x => x.Valor).GreaterThan(0);
    }
}
