namespace SharedDomain.Shared;

public interface IEventConsumer<T>
    where T : IEvent
{
    Task Consume(T @event, CancellationToken cancellationToken);
}
