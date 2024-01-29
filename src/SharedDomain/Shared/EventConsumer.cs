namespace SharedDomain.Shared;

public interface IEventConsumer<T>
    where T : IEvent
{
    string TopicName { get; }
    Task<T> Consume();
}
