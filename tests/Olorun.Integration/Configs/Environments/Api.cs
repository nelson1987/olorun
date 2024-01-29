using FluentResults;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Olorun.Integration.Configs.Fixtures;
using System.Diagnostics;

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

public class Api : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
        => builder.UseEnvironment("Test")
               .ConfigureTestServices(services =>
               {
                   // services.AddScoped<IWeatherForecastHandler, WeatherForecastHandler>();
                   services.AddScoped<ITesteMessageProducer<TesteMessage>, TesteMessageProducer<TesteMessage>>();
                   services.AddScoped<ITesteMessageConsumer<TesteMessage>, TesteMessageConsumer<TesteMessage>>();
               });

    internal Task Consume<TConsumer>(TimeSpan? timeout = null) where TConsumer : IConsumer
    {
        const int defaultTimeoutInSeconds = 1;
        timeout ??= TimeSpan.FromSeconds(defaultTimeoutInSeconds);

        using var scope = Services.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<TConsumer>();
        if (Debugger.IsAttached)
            return consumer.Consume(CancellationToken.None);

        using var tokenSource = new CancellationTokenSource(timeout.Value);
        return consumer.Consume(tokenSource.Token);
        throw new NotImplementedException();
    }
}