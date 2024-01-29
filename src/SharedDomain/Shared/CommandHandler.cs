namespace SharedDomain.Shared;
public interface ICommandHandler<T, V>
    where T : ICommand
    where V : class
{
    Task<V> Handle(T command, CancellationToken cancellationToken);
}
public abstract class CommandHandler<T, V> : ICommandHandler<T, V>
    where T : ICommand
    where V : class
{
    public abstract Task<V> Handle(T command, CancellationToken cancellationToken);
}
