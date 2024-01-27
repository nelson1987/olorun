namespace SharedDomain.Shared;

public interface IEventProducer<T>
    where T : IEvent
{
    string TopicName { get; }
    Task Send(T @event, CancellationToken cancellationToken);
}
