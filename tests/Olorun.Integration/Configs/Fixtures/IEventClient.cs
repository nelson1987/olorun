namespace Olorun.Integration.Configs.Fixtures;

public interface IEventClient
{
    Task<bool> Produce<TMessage>(string message, TMessage @event, CancellationToken cancellationToken);
    Task<bool> Consume<TMessage>(string message, TMessage @event, CancellationToken cancellationToken);
}
//public interface IEventClientConsumers { }