namespace SharedDomain.Shared;

/*
 InclusaoPagamentoCommand -> CommandHandler -> (Insert) ReadRepository -> Event -> Producer(Fire And Forget)
 Consumer -> (insert) WriteRepository -> Next Event -> Producer -> (Update) ReadRepository
 Consumer -> (Update) WriteRepository -> Next Event -> Producer -> (Update) ReadRepository
 */
public interface IEntity
{
    Guid Id { get; set; }
}
