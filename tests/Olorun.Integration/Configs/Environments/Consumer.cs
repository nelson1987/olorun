using FluentResults;
using Microsoft.Extensions.Configuration;

namespace Olorun.Integration.Configs.Environments;
public interface IConsumer
{
    Task Consume(CancellationToken cancellationToken);
}
public abstract class Consumer<TEvent> : IConsumer
{
    private readonly IEventCommunicationService _eventCommunicationService;
    private readonly int _maxRetryAttempts;

    public abstract string Topic { get; }
    public abstract string Subject { get; }
    public abstract string ConsumerName { get; }

    protected Consumer(
        IEventCommunicationService eventCommunicationService,
        IConfiguration configuration)
    {
        _eventCommunicationService = eventCommunicationService;
        _maxRetryAttempts = configuration.GetValue($"HostedServices:{ConsumerName}:MaxRetryAttempts", defaultValue: 3);
    }

    public void Consume(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        //using var logger = ConsumerContextTrace.New(ConsumerName).Start();
        //var consumeResult = _eventCommunicationService.SafeConsumeEvent<TEvent>(Topic, cancellationToken);
        //
        //using var span = Telemetry.Tracer.StartActiveSpan($"{Topic} process", SpanKind.Consumer);
        //
        //if (consumeResult.IsFailed)
        //{
        //    if (cancellationToken.IsCancellationRequested)
        //    {
        //        logger.Trace.SetAdditionalData("ResponseStatus", ConsumerStatus.OperationCanceled);
        //        return;
        //    }
        //
        //    span.SetStatus(Status.Error.WithDescription(consumeResult.ToString()));
        //    logger.Trace.SetAdditionalData("Reasons", consumeResult.Reasons);
        //    logger.Trace.SetAdditionalData("ResponseStatus", ConsumerStatus.Error);
        //    return;
        //}
        //
        //try
        //{
        //    var result = await ConsumerRetryPolicy
        //        .JitteredBackOff(_maxRetryAttempts)
        //        .ExecuteAsync(async token => await Handle(consumeResult.Value, token), cancellationToken);
        //
        //    if (result.IsSuccess)
        //    {
        //        span.SetStatus(Status.Ok);
        //        logger.Trace.SetAdditionalData("ResponseStatus", ConsumerStatus.Success);
        //        return;
        //    }
        //
        //    span.SetStatus(Status.Error);
        //    logger.Trace.SetAdditionalData("Reasons", result.Reasons);
        //    logger.Trace.SetAdditionalData("ResponseStatus", ConsumerStatus.UnprocessableEntity);
        //}
        //catch (OperationCanceledException ex)
        //{
        //    Logger.LogException("Execution canceled, republishing message", ex, LogSeverity.Warning);
        //    span.RecordException(ex);
        //    var republishResult = await _eventCommunicationService.ProduceEvent(consumeResult.Value, Topic, Subject, CancellationToken.None);
        //
        //    if (republishResult.IsFailed)
        //        Logger.Log(LogEntry.New()
        //            .SetMessage("Failed to republish message")
        //            .SetSeverity(LogSeverity.Error)
        //            .SetAdditionalData("Reasons", republishResult.Reasons));
        //
        //    logger.Trace.SetAdditionalData("ResponseStatus", ConsumerStatus.OperationCanceled);
        //}
        //catch (Exception ex)
        //{
        //    Logger.LogException($"Consumer {ConsumerName} has been failed due to an exception", ex);
        //    span.RecordException(ex);
        //    span.SetStatus(Status.Error);
        //    logger.Trace.SetAdditionalData("ResponseStatus", ConsumerStatus.Error);
        //}
    }

    protected abstract Task<Result> Handle(TEvent @event, CancellationToken cancellationToken);

    Task IConsumer.Consume(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
