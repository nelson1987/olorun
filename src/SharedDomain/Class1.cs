namespace SharedDomain;

public record Leilao
{
    public Guid Id { get; init; }
    public required Produto Objeto { get; init; }
    public required List<Lance> Lances { get; init; }
    public void DarLance(Lance lance)
    {
        Lances.Add(lance);
    }
}
public record Produto
{
    public required string Descricao { get; init; }
}
public record Cliente
{
    public required string Email { get; init; }
}
public record Lance
{
    public required Cliente Cliente { get; init; }
    public required decimal Valor { get; init; }
}
//Comando
public record IncluirLanceCommand(Guid IdLeilao, string email, decimal valor);
//Eventos
public record LanceAceitoEvent();
public record LanceRejeitadoEvent();

public class LanceCommandHandler
{
    public Result Handle(IncluirLanceCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

public class Result { }