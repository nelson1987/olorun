using SharedDomain.Shared;

namespace SharedDomain.Features;
public class Pagamento : IEntity
{
    public Guid Id { get; set; }
    public required string ContaDebitada { get; set; }
    public required string ContaCreditada { get; set; }
    public decimal Valor { get; set; }
    public PagamentoIncluidoType Type { get; set; }
}
public enum PagamentoIncluidoType { Open = 1, Submitted = 2, Rejected = 3, Closed = 4 }

public class PagamentoSubmetidoEvent : IEvent
{
    public Guid Id { get; set; }
    public PagamentoIncluidoType Type { get; set; }
}


public class BookStoreDatabaseSettings
{
    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public string BooksCollectionName { get; set; } = null!;
}