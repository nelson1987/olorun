using FluentResults;

namespace SharedDomain;
public interface IEntity 
{
    Guid Id { get; set; }
}
public class Pagamento : IEntity
{
    public Guid Id { get; set; }
    public string ContaDebitada { get; set; }
    public string ContaCreditada { get; set; }
    public decimal Valor { get; set; }
}
public interface ICommand
{
}
public class PagamentoCommand : ICommand 
{
    public string ContaDebitada { get; set; }
    public string ContaCreditada { get; set; }
    public decimal Valor { get; set; }
}
public interface ICommandHandler<T, V> 
    where T : ICommand
    where V : class
{
    Task<V> Handle(T command, CancellationToken cancellationToken);
}
public class CommandHandler : ICommandHandler<PagamentoCommand, Result<Pagamento?>>
{
    public Task<Result<Pagamento?>> Handle(PagamentoCommand command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}