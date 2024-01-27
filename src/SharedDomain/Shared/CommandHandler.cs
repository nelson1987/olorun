using SharedDomain.Features;

namespace SharedDomain.Shared;

public abstract class CommandHandler<T, V> : ICommandHandler<T, V>
    where T : ICommand
    where V : class
{
    public abstract Task<V> Handle(T command, CancellationToken cancellationToken);
}
