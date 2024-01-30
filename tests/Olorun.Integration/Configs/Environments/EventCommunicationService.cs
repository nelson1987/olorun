using FluentResults;
using Olorun.Integration.Configs.Fixtures;

namespace Olorun.Integration.Configs.Environments;
public interface IEventCommunicationService
{
    Task<Result> ProduceEvent<TData>(TData data, string topic, string subject, CancellationToken cancellationToken = default);
    Result<TResult> SafeConsumeEvent<TResult>(string topic, CancellationToken cancellationToken);
}
public class EventCommunicationService : IEventCommunicationService
{
    private readonly IEventClient _eventClient;

    public EventCommunicationService(IEventClient eventClient)
    {
        _eventClient = eventClient;
    }

    public Result ProduceEvent<TData>(TData data, string topic, string subject, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
        //using var span = Telemetry.Tracer.StartActiveSpan($"{topic} publish", SpanKind.Producer);

        //var eventClientRequest = new EventClientRequest<TData>(data, topic, subject);
        //var produceResult = await _eventClient.Produce(eventClientRequest, JsonConvert.SerializeObject, cancellationToken);

        //if (produceResult)
        //{
        //    span.SetStatus(Status.Ok);
        //    return Result.Ok();
        //}

        //span.SetStatus(Status.Error.WithDescription("Failed to produce event"));
        //return Result.Fail(new Error("Failed to produce event")
        //                            .WithMetadata("Topic", topic));
    }

    public Result<TResult> SafeConsumeEvent<TResult>(string topic, CancellationToken cancellationToken)
    {
        try
        {
            throw new NotImplementedException();
            //return _eventClient.Consume(topic, JsonConvert.DeserializeObject<TResult>, cancellationToken)!;
        }
        catch (Exception ex)
        {
            const string message = "Failed to consume topic";
            return Result.Fail(new Error(message)
                .CausedBy(ex)
                .WithMetadata("topic.name", topic));
        }
    }

    Task<Result> IEventCommunicationService.ProduceEvent<TData>(TData data, string topic, string subject, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
