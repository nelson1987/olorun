namespace SharedDomain.Shared;

public interface IEventConsumer<T>
    where T : IEvent
{
    Task Consume(T @event, CancellationToken cancellationToken);
}

public abstract class EventConsumer<T> : IEventConsumer<T>
    where T : IEvent
{
    public abstract Task Consume(T @event, CancellationToken cancellationToken);
}