using Confluent.Kafka;
using Confluent.SchemaRegistry;
using FluentResults;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace SharedDomain
{
    #region Pessoa Entity
    public class CashReserveConsumer : Consumer<OrdersRenegotiationRespondedEvent>
    {
        public override string Topic => EventsTopics.WeatherforecastResponded.Name;
        public override string Subject => nameof(EventsTopics.WeatherforecastResponded.Subjects.WeatherforecastResponded);
        public override string ConsumerName => nameof(CashReserveConsumer);


        public CashReserveConsumer(
        IEventCommunicationService eventCommunicationService,
                IConfiguration configuration) : base(eventCommunicationService, configuration) { }

        protected override Task<Result> Handle(OrdersRenegotiationRespondedEvent @event, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
    public record OrdersRenegotiationRespondedEvent();
    public record CashReserveResponse();
    public record CashReserveRequest();
    public static class PessoaFactory
    {
        public static Pessoa Create()
        {
            return new Pessoa(Guid.NewGuid(), "Nome");
        }
    };
    public record Pessoa(Guid Id, string Nome);

    #endregion

    #region Repository
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
        public async Task Consume(CancellationToken cancellationToken)
        {
            //using var logger = ConsumerContextTrace.New(ConsumerName).Start();
            var consumeResult = _eventCommunicationService.SafeConsumeEvent<TEvent>(Topic, cancellationToken);
            //using var span = Telemetry.Tracer.StartActiveSpan($"{Topic} process", SpanKind.Consumer);

            if (consumeResult.IsFailed)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    //logger.Trace.SetAdditionalData("ResponseStatus", ConsumerStatus.OperationCanceled);
                    return;
                }

                //span.SetStatus(Status.Error.WithDescription(consumeResult.ToString()));
                //logger.Trace.SetAdditionalData("Reasons", consumeResult.Reasons);
                //logger.Trace.SetAdditionalData("ResponseStatus", ConsumerStatus.Error);
                return;
            }

            try
            {
                //var result = await ConsumerRetryPolicy
                //    .JitteredBackOff(_maxRetryAttempts)
                //    .ExecuteAsync(async token => await Handle(consumeResult.Value, token), cancellationToken);
                //
                //if (result.IsSuccess)
                //{
                //    //span.SetStatus(Status.Ok);
                //    //logger.Trace.SetAdditionalData("ResponseStatus", ConsumerStatus.Success);
                //    return;
                //}

                //span.SetStatus(Status.Error);
                //logger.Trace.SetAdditionalData("Reasons", result.Reasons);
                //logger.Trace.SetAdditionalData("ResponseStatus", ConsumerStatus.UnprocessableEntity);
            }
            catch (OperationCanceledException ex)
            {
                //Logger.LogException("Execution canceled, republishing message", ex, LogSeverity.Warning);
                //span.RecordException(ex);
                var republishResult = await _eventCommunicationService.ProduceEvent(consumeResult.Value, Topic, Subject, CancellationToken.None);

                //if (republishResult.IsFailed)
                //    Logger.Log(LogEntry.New()
                //        .SetMessage("Failed to republish message")
                //        .SetSeverity(LogSeverity.Error)
                //        .SetAdditionalData("Reasons", republishResult.Reasons));

                //logger.Trace.SetAdditionalData("ResponseStatus", ConsumerStatus.OperationCanceled);
            }
            catch (Exception ex)
            {
                //Logger.LogException($"Consumer {ConsumerName} has been failed due to an exception", ex);
                //span.RecordException(ex);
                //span.SetStatus(Status.Error);
                //logger.Trace.SetAdditionalData("ResponseStatus", ConsumerStatus.Error);
            }
        }

        protected abstract Task<Result> Handle(TEvent @event, CancellationToken cancellationToken);
    }
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

        public async Task<Result> ProduceEvent<TData>(TData data, string topic, string subject, CancellationToken cancellationToken = default)
        {
            //using var span = Telemetry.Tracer.StartActiveSpan($"{topic} publish", SpanKind.Producer);

            var eventClientRequest = new EventClientRequest<TData>(data, topic, subject);
            var produceResult = await _eventClient.Produce(eventClientRequest, Newtonsoft.Json.JsonConvert.SerializeObject, cancellationToken);

            if (produceResult)
            {
                //span.SetStatus(Status.Ok);
                return Result.Ok();
            }

            //span.SetStatus(Status.Error.WithDescription("Failed to produce event"));
            return Result.Fail(new FluentResults.Error("Failed to produce event")
                                        .WithMetadata("Topic", topic));
        }

        public Result<TResult> SafeConsumeEvent<TResult>(string topic, CancellationToken cancellationToken)
        {
            try
            {
                return _eventClient.Consume(topic, Newtonsoft.Json.JsonConvert.DeserializeObject<TResult>, cancellationToken)!;
            }
            catch (Exception ex)
            {
                const string message = "Failed to consume topic";
                return Result.Fail(new FluentResults.Error(message)
                    .CausedBy(ex)
                    .WithMetadata("topic.name", topic));
            }
        }
    }
    public interface IPessoaReadRepository
    {
        Task<Pessoa> GetById(Guid id);
    }

    public class PessoaReadRepository : IPessoaReadRepository
    {
        public Task<Pessoa> GetById(Guid id)
        {
            throw new NotImplementedException();
        }
    }
    public interface IEventClientProducer
    {
        IProducer<long, string> Producer { get; }
        bool EnableVerboseMessageLogging { get; }
    }
    public sealed class EventClientProducer : IEventClientProducer, IDisposable
    {
        public IProducer<long, string> Producer { get; }
        public bool EnableVerboseMessageLogging { get; }

        public EventClientProducer(EventClientSettings settings)
        {
            if (settings.ProducerTopics is null) return;

            EnableVerboseMessageLogging = settings.EnableVerboseMessageLogging;

            Producer = new ProducerBuilder<long, string>(settings.ProducerConfig)
                .SetKeySerializer(Serializers.Int64)
                .SetValueSerializer(Serializers.Utf8)
                .Build();
        }

        public void Dispose()
        {
            Producer.Flush();
            Producer.Dispose();
        }
    }
    public interface IEventClient
    {
        /// <summary>
        /// Produces an event on a topic.
        /// </summary>
        /// <typeparam name="TData">The object that will be posted on the given topic.</typeparam>
        /// <param name="request">
        /// It has all the information needed to post the event.
        /// <list type="bullet">
        /// <item>"Data": The object that will be posted on the given topic.</item>
        /// <item>"Topic": The topic name that will receive the event.</item>
        /// <item>"Subject": The subject of the event.</item>
        /// <item>"ShouldCompressData": It indicates if the object will be compressed before posting it on the topic (lz4block).</item>
        /// </list>
        /// </param>
        /// <param name="cancellationToken">The cancellation token to stop the process if needed.</param>
        /// <returns>A boolean indicating if the post was successfully executed.</returns>
        //Task<bool> Produce<TData>(EventClientRequest<TData> request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Produces an event on a topic.
        /// </summary>
        /// <typeparam name="TData">The object that will be posted on the given topic.</typeparam>
        /// <param name="request">
        /// It has all the information needed to post the event.
        /// <list type="bullet">
        /// <item>"Data": The object that will be posted on the given topic.</item>
        /// <item>"Topic": The topic name that will receive the event.</item>
        /// <item>"Subject": The subject of the event.</item>
        /// <item>"ShouldCompressData": It indicates if the object will be compressed before posting it on the topic (lz4block).</item>
        /// </list>
        /// </param>
        /// <param name="serializer">A func with a serializer specification to use upon the "request".</param>
        /// <param name="cancellationToken">The cancellation token to stop the process if needed.</param>
        /// <returns>A boolean indicating if the post was successfully executed.</returns>
        Task<bool> Produce<TData>(EventClientRequest<TData> request, Func<object, string> serializer,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Consumes an event from a topic.
        /// </summary>
        /// <typeparam name="TData">The object type that the method will convert from the event to it.</typeparam>
        /// <param name="topicName">The topic name that the method will try to get an event from.</param>
        /// <param name="cancellationToken">The cancellation token to stop the process if needed.</param>
        /// <returns>The event converted to the specified object type.</returns>
        //TData Consume<TData>(string topicName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Consumes an event from a topic, wrapped in a response that provides additional metadata (if available).
        /// </summary>
        /// <typeparam name="TData">The object type that the method will convert from the event to it.</typeparam>
        /// <param name="topicName">The topic name that the method will try to get an event from.</param>
        /// <param name="cancellationToken">The cancellation token to stop the process if needed.</param>
        /// <returns>The event converted to the specified object type.</returns>
        //EventClientResponse<TData> ConsumeResponse<TData>(string topicName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Consumes an event from a topic
        /// </summary>
        /// <typeparam name="TData">The object type that the method will convert from the event to it.</typeparam>
        /// <param name="topicName">The topic name that the method will try to get an event from.</param>
        /// <param name="deserializer">A func with a deserializer specification to convert the event string to the specified object type.</param>
        /// <param name="cancellationToken">The cancellation token to stop the process if needed.</param>
        /// <returns>The event converted to the specified object type.</returns>
        TData Consume<TData>(string topicName, Func<string, TData> deserializer, CancellationToken cancellationToken = default);

        /// <summary>
        /// Consumes an event from a topic, wrapped in a response that provides additional metadata (if available).
        /// </summary>
        /// <typeparam name="TData">The object type that the method will convert from the event to it.</typeparam>
        /// <param name="topicName">The topic name that the method will try to get an event from.</param>
        /// <param name="deserializer">A func with a deserializer specification to convert the event string to the specified object type.</param>
        /// <param name="cancellationToken">The cancellation token to stop the process if needed.</param>
        /// <returns>The event converted to the specified object type.</returns>
        EventClientResponse<TData> ConsumeResponse<TData>(string topicName, Func<string, TData> deserializer, CancellationToken cancellationToken = default);
    }
    public sealed class EventClient : IEventClient
    {
        private readonly IEventClientConsumers _consumers;
        private readonly IEventClientProducer _producer;

        public EventClient(IEventClientConsumers consumers,
            IEventClientProducer producer)
        {
            _consumers = consumers;
            _producer = producer;
        }

        /// <summary>
        /// Produces an event on a topic.
        /// </summary>
        /// <typeparam name="TData">The object that will be posted on the given topic.</typeparam>
        /// <param name="request">
        /// It has all the information needed to post the event.
        /// <list type="bullet">
        /// <item>"Data": The object that will be posted on the given topic.</item>
        /// <item>"Topic": The topic name that will receive the event.</item>
        /// <item>"Subject": The subject of the event.</item>
        /// <item>"ShouldCompressData": It indicates if the object will be compressed before posting it on the topic (lz4block).</item>
        /// </list>
        /// </param>
        /// <param name="cancellationToken">The cancellation token to stop the process if needed.</param>
        /// <returns>A boolean indicating if the post was successfully executed.</returns>
        //public async Task<bool> Produce<TData>(EventClientRequest<TData> request, CancellationToken cancellationToken = default) =>
        //    await ProduceEvent(request, (data) => JsonSerializer.Serialize(data), cancellationToken);

        /// <summary>
        /// Produces an event on a topic.
        /// </summary>
        /// <typeparam name="TData">The object that will be posted on the given topic.</typeparam>
        /// <param name="request">
        /// It has all the information needed to post the event.
        /// <list type="bullet">
        /// <item>"Data": The object that will be posted on the given topic.</item>
        /// <item>"Topic": The topic name that will receive the event.</item>
        /// <item>"Subject": The subject of the event.</item>
        /// <item>"ShouldCompressData": It indicates if the object will be compressed before posting it on the topic (lz4block).</item>
        /// </list>
        /// </param>
        /// <param name="serializer">A func with a serializer specification to use upon the "request".</param>
        /// <param name="cancellationToken">The cancellation token to stop the process if needed.</param>
        /// <returns>A boolean indicating if the post was successfully executed.</returns>
        public async Task<bool> Produce<TData>(EventClientRequest<TData> request, Func<object, string> serializer, CancellationToken cancellationToken = default) =>
            await ProduceEvent(request, serializer, cancellationToken);

        /// <summary>
        /// Consumes an event from a topic.
        /// </summary>
        /// <typeparam name="TData">The object type that the method will convert from the event to it.</typeparam>
        /// <param name="topicName">The topic name that the method will try to get an event from.</param>
        /// <param name="cancellationToken">The cancellation token to stop the process if needed.</param>
        /// <returns>The event converted to the specified object type.</returns>
        //public TData Consume<TData>(string topicName, CancellationToken cancellationToken = default) =>
        //    ConsumeResponse<TData>(topicName, cancellationToken).Data;

        /// <summary>
        /// Consumes an event from a topic, wrapped in a response that provides additional metadata (if available).
        /// </summary>
        /// <typeparam name="TData">The object type that the method will convert from the event to it.</typeparam>
        /// <param name="topicName">The topic name that the method will try to get an event from.</param>
        /// <param name="cancellationToken">The cancellation token to stop the process if needed.</param>
        /// <returns>The event converted to the specified object type.</returns>
        //public EventClientResponse<TData> ConsumeResponse<TData>(string topicName, CancellationToken cancellationToken = default) =>
        //    ConsumeEvent(topicName, (data) => JsonSerializer.Deserialize<TData>(data), cancellationToken);

        /// <summary>
        /// Consumes an event from a topic
        /// </summary>
        /// <typeparam name="TData">The object type that the method will convert from the event to it.</typeparam>
        /// <param name="topicName">The topic name that the method will try to get an event from.</param>
        /// <param name="deserializer">A func with a deserializer specification to convert the event string to the specified object type.</param>
        /// <param name="cancellationToken">The cancellation token to stop the process if needed.</param>
        /// <returns>The event converted to the specified object type.</returns>
        public TData Consume<TData>(string topicName, Func<string, TData> deserializer, CancellationToken cancellationToken = default) =>
            ConsumeResponse(topicName, deserializer, cancellationToken).Data;

        /// <summary>
        /// Consumes an event from a topic, wrapped in a response that provides additional metadata (if available).
        /// </summary>
        /// <typeparam name="TData">The object type that the method will convert from the event to it.</typeparam>
        /// <param name="topicName">The topic name that the method will try to get an event from.</param>
        /// <param name="deserializer">A func with a deserializer specification to convert the event string to the specified object type.</param>
        /// <param name="cancellationToken">The cancellation token to stop the process if needed.</param>
        /// <returns>The event converted to the specified object type.</returns>
        public EventClientResponse<TData> ConsumeResponse<TData>(string topicName, Func<string, TData> deserializer, CancellationToken cancellationToken = default) =>
            ConsumeEvent(topicName, deserializer, cancellationToken);

        private async Task<bool> ProduceEvent<TData>(EventClientRequest<TData> request, Func<object, string> serializer, CancellationToken cancellationToken)
        {
            try
            {
                var eventMessage = new EventMessageBuilder(serializer)
                    .WithValue(request)
                    .WithKey(DateTime.UtcNow.Ticks)
                    .WithTimestamp(Timestamp.Default)
                    //.AddHeaders(request.Metadata)
                    //.AddHeader(DistributedTrace.CreateDistributedTraceHeader())
                    .Create();

                var deliveryResult = await _producer.Producer.ProduceAsync(request.Topic, eventMessage, cancellationToken);

                //if (_producer.EnableVerboseMessageLogging)
                //    deliveryResult.LogVerboseMessage();

                // if (deliveryResult.Status != PersistenceStatus.Persisted)
                //Logger.Log(new LogEntry()
                //    .SetMessage($"Error while producing message, delivery result is '{deliveryResult.Status}'!")
                //    .SetSeverity(deliveryResult.Status == PersistenceStatus.NotPersisted
                //        ? LogSeverity.Error
                //        : LogSeverity.Warning)
                //    .SetAdditionalData("DeliveryResult", deliveryResult)
                //    .AddTags(nameof(EventClient), nameof(ProduceEvent)));

                return deliveryResult.Status is PersistenceStatus.Persisted;
            }
            catch (Exception e)
            {
                //Logger.LogException($"Produce error: {e.Message}", e, LogSeverity.Emergency,
                //    "event-client-producer");

                return false;
            }
        }

        private EventClientResponse<TData> ConsumeEvent<TData>(string topicName, Func<string, TData> deserializer, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
            /*
            var consumer = _consumers.Get(topicName);
            var consumeResult = null as ConsumeResult<long, string>;
            var eventClientData = null as EventClientData;

            try
            {
                consumeResult = consumer.Consume(cancellationToken);
                if (consumeResult is null)
                    return new EventClientResponse<TData>(default, default);

                var eventMessage = consumeResult.Message?.Value;
                var eventMetadata = consumeResult.Message?.Headers?
                    .Where(x => x.Key != DistributedTrace.TraceStackHeader)
                    .Select(x => new KeyValuePair<string, string>(x.Key, EventMessageHeader.Decode(x.GetValueBytes())));

                if (string.IsNullOrWhiteSpace(eventMessage))
                    return new EventClientResponse<TData>(default, eventMetadata);

                //DistributedTrace.AcceptDistributedTraceHeaders(consumeResult.Message);

                eventClientData = JsonSerializer.Deserialize<EventClientData>(eventMessage);
                if (eventClientData is null || string.IsNullOrWhiteSpace(eventClientData.Data))
                    return new EventClientResponse<TData>(default, eventMetadata);

                var decompressedData = (eventClientData.IsDataCompressed is null || eventClientData.IsDataCompressed is true)
                    ? eventClientData.Data.DecompressHexToJson()
                    : eventClientData.Data;

                var deserializedData = deserializer(decompressedData);

                return new EventClientResponse<TData>(deserializedData, eventMetadata);
            }
            catch (Exception ex)
            {
                //Logger.Log(new ExceptionLogEntry(ex)
                //    .SetMessage("Error while trying to consume event!")
                //    .SetAdditionalData($"TopicName", topicName)
                //    .SetSeverity(LogSeverity.Error));

                throw;
            }
            finally
            {
                if (OperationContext.Current is { })
                {
                    OperationContext.Current.LogAdditionalData["ConsumeResult"] = new
                    {
                        Offset = consumeResult?.Offset.Value,
                        Partition = consumeResult?.Partition.Value,
                    };
                }

                if (_consumers.EnableVerboseMessageLogging)
                {
                    //Logger.Log(new LogEntry()
                    //    .SetAdditionalData("ConsumeResult", consumeResult is null ? null : new
                    //    {
                    //        consumeResult.IsPartitionEOF,
                    //        consumeResult.Message?.Headers,
                    //        consumeResult.Message?.Key,
                    //        consumeResult.Message?.Timestamp,
                    //        consumeResult.Offset,
                    //        consumeResult.Partition,
                    //        consumeResult.Topic,
                    //        consumeResult.TopicPartition,
                    //        consumeResult.TopicPartitionOffset
                    //    })
                    //    .SetAdditionalData("AssignedTopicPartitions", consumer.Assignment.Select(x => x.Partition).ToList())
                    //    .SetAdditionalData("EventClientData", eventClientData is null ? null : new
                    //    {
                    //        eventClientData.Id,
                    //        eventClientData.EventType,
                    //        eventClientData.EventTime,
                    //        eventClientData.Topic,
                    //        eventClientData.Subject,
                    //        eventClientData.IsDataCompressed
                    //    })
                    //    .SetSeverity(LogSeverity.Info)
                    //    .SetMessage("Consume finished"));
                }
            }*/
        }
    }
    public interface IEventClientConsumers
    {
        IConsumer<long, string> Get(string topicName);
        bool EnableVerboseMessageLogging { get; }
    }
    public sealed class EventClientConsumers : IEventClientConsumers, IDisposable
    {
        private readonly Dictionary<string, IConsumer<long, string>> _consumers;
        public bool EnableVerboseMessageLogging { get; }

        public EventClientConsumers(EventClientSettings settings)
        {
            _consumers = new Dictionary<string, IConsumer<long, string>>();

            if (settings.ConsumerTopics is null) return;

            EnableVerboseMessageLogging = settings.EnableVerboseMessageLogging;

            foreach (var topic in settings.ConsumerTopics)
            {
                var consumer = GetConsumer(settings);
                consumer.Subscribe(topic);
                _consumers.Add(topic, consumer);
            }
        }

        public IConsumer<long, string> Get(string topicName)
        {
            _consumers.TryGetValue(topicName, out var consumer);
            if (consumer is null)
                throw new Exception($"Consumer for Topic '{topicName}' does not exist in the present context");
            return consumer;
        }

        private IConsumer<long, string> GetConsumer(EventClientSettings settings)
        {
            return new ConsumerBuilder<long, string>(settings.ConsumerConfig)
                .SetKeyDeserializer(Deserializers.Int64)
                .SetValueDeserializer(Deserializers.Utf8)
                //.SetErrorHandler((consumer, error) =>
                //{
                //    var severity = error.IsFatal ? LogSeverity.Error : LogSeverity.Warning;
                //    Logger.Log(LogEntry.New()
                //        .SetMessage(error.Reason)
                //        .SetSeverity(severity)
                //        .SetAdditionalData("ConsumerId", consumer.MemberId)
                //        .SetAdditionalData("Error", error));
                //})
                .SetPartitionsAssignedHandler((consumer, topicPartitions) =>
                {
                    if (topicPartitions.Any() is false) return;

                    var assignedPartitions = topicPartitions
                        .GroupBy(x => x.Topic)
                        .Select(group => new
                        {
                            Topic = group.Key,
                            Partitions = group.Select(x => x.Partition.Value)
                        })
                        .ToList();

                    var currentPartitions = consumer.Assignment
                        .Concat(topicPartitions)
                        .GroupBy(x => x.Topic)
                        .Select(group => new { group.Key, Partitions = group.Select(x => x.Partition.Value) })
                        .ToList();

                    //Logger.Log(LogEntry.New()
                    //    .SetMessage("Partitions assigned to consumer")
                    //    .SetAdditionalData("ConsumerId", consumer.MemberId)
                    //    .SetAdditionalData("CurrentPartitions", currentPartitions)
                    //    .SetAdditionalData("AssignedPartitions", assignedPartitions));
                })
                .SetPartitionsRevokedHandler((consumer, revokedPartitions) =>
                {
                    if (revokedPartitions.Any() is false) return;

                    var revokedPartitionsByTopic = revokedPartitions
                        .GroupBy(x => x.Topic)
                        .Select(group => new
                        {
                            Topic = group.Key,
                            Partitions = group.Select(x => new { Partition = x.Partition.Value, Offset = x.Offset.Value })
                        })
                        .ToList();

                    var currentPartitions = consumer.Assignment
                        .Except(revokedPartitions.Select(x => x.TopicPartition))
                        .GroupBy(x => x.Topic)
                        .Select(group => new { group.Key, Partitions = group.Select(x => x.Partition.Value) })
                        .ToList();

                    //Logger.Log(LogEntry.New()
                    //    .SetMessage("Partitions revoked from consumer")
                    //    .SetAdditionalData("ConsumerId", consumer.MemberId)
                    //    .SetAdditionalData("CurrentPartitions", currentPartitions)
                    //    .SetAdditionalData("RevokedPartitions", revokedPartitionsByTopic));
                })
                .Build();
        }

        public void Dispose()
        {
            foreach (var consumer in _consumers)
            {
                consumer.Value.Close();
            }
        }
    }
    #endregion
    
    #region Settings

    public sealed class EventClientSettings
    {
        /// <summary>
        /// Initial list of brokers as a CSV list of broker host or host:port. The application
        /// may also use `rd_kafka_brokers_add()` to add brokers during runtime. default:'' importance: high
        /// </summary>
        public string BootstrapServers { get; private set; }
        public EventClientCredential ProducerCredential { get; private set; }
        public EventClientCredential ConsumerCredential { get; private set; }
        public SchemaRegistryCredential SchemaRegistryCredential { get; private set; }

        public string ConsumerGroup
        {
            get => ConsumerConfig.GroupId;
            set => ConsumerConfig.GroupId = value;
        }
        public IList<string> ConsumerTopics { get; set; } = new List<string>();
        public IList<string> ProducerTopics { get; set; } = new List<string>();
        public ProducerConfig ProducerConfig { get; private set; }
        public ConsumerConfig ConsumerConfig { get; private set; }
        public SchemaRegistryConfig SchemaRegistryConfig { get; private set; }

        /// <summary>
        /// Log every consumed and produced message
        /// </summary>
        /// <value>default is false</value>
        public bool EnableVerboseMessageLogging { get; set; } = false;
        /// <summary>
        /// Creates settings for unauthenticated events client, for producing and consuming purposes (this constructor
        /// is useful for development and testing scenarios, since our hosted environments require authentication).
        /// </summary>
        /// <param name="bootstrapServers">Comma separated list of brokers.</param>
        /// <param name="producerAndConsumerPrefix">Optional producer and consumer prefix.</param>
        /// <param name="schemaRegistryCredential">Optional schema registry configuration.</param>
        public EventClientSettings(
            string bootstrapServers,
            string? producerAndConsumerPrefix = null,
            SchemaRegistryCredential? schemaRegistryCredential = null)
        {
            BootstrapServers = bootstrapServers;
            EnableVerboseMessageLogging = true;

            ProducerConfig = new ProducerConfig
            {
                BootstrapServers = BootstrapServers,
                SecurityProtocol = SecurityProtocol.Plaintext,
                ClientId = GetClientIdForProducer(producerAndConsumerPrefix!),
            };

            ConsumerConfig = new ConsumerConfig
            {
                BootstrapServers = BootstrapServers,
                SecurityProtocol = SecurityProtocol.Plaintext,
                ClientId = GetClientIdForConsumer(producerAndConsumerPrefix!),
                AutoOffsetReset = AutoOffsetReset.Earliest,
            };

            SchemaRegistryConfig = new SchemaRegistryConfig
            {
                Url = schemaRegistryCredential?.Url,
                BasicAuthUserInfo = schemaRegistryCredential?.GetBasicAuthUserInfo()
            };
        }
        /*
        /// <summary>
        /// Creates settings for authenticated events client, for producing and consuming purposes.
        /// </summary>
        /// <param name="bootstrapServers">list of brokers as a CSV list of broker host or host:port. Eg: localhost:9092</param>
        /// <param name="producerCredential">Credential of producer</param>
        /// <param name="consumerCredential">Credential of consumer</param>
        /// <param name="producerAndConsumerPrefix">prefix is optional and use to for client id name,
        /// generally is application context name. Is is null or empty de
        /// Eg. Define "customer-api" results in "customer-api-producer-machine_name" and "customer-api-consumer-machine_name"
        /// </param>
        /// <param name="schemaRegistryCredential">Optional schema registry configuration.</param>
        public EventClientSettings(
            string bootstrapServers,
            EventClientCredential producerCredential = null,
            EventClientCredential consumerCredential = null,
            string producerAndConsumerPrefix = null,
            SchemaRegistryCredential schemaRegistryCredential = null)
        {
            BootstrapServers = bootstrapServers;
            ProducerCredential = producerCredential;
            ConsumerCredential = consumerCredential;
            SchemaRegistryCredential = schemaRegistryCredential;

            ProducerConfig = new ProducerConfig
            {
                BootstrapServers = BootstrapServers,
                SaslUsername = ProducerCredential?.Username,
                SaslPassword = ProducerCredential?.Password,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                ClientId = GetClientIdForProducer(producerAndConsumerPrefix),
            };

            ConsumerConfig = new ConsumerConfig
            {
                BootstrapServers = BootstrapServers,
                SaslUsername = ConsumerCredential?.Username,
                SaslPassword = ConsumerCredential?.Password,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                ClientId = GetClientIdForConsumer(producerAndConsumerPrefix),
            };

            SchemaRegistryConfig = new SchemaRegistryConfig
            {
                Url = schemaRegistryCredential?.Url,
                BasicAuthUserInfo = schemaRegistryCredential?.GetBasicAuthUserInfo()
            };
        }
        
        [Obsolete("This constructor is recommended only for eventhub." +
                  "Use the constructor with event client name for tracing your events better into log infrastructure")]
        public EventClientSettings(string clientListFQDN, string userName, string password)
            : this(clientListFQDN,
                  new EventClientCredential(userName, password),
                  new EventClientCredential(userName, password),
                  string.Empty)
        {

            // Microsoft's Recommended Configurations
            // https://docs.microsoft.com/en-us/azure/event-hubs/apache-kafka-configurations
            ProducerConfig.SocketTimeoutMs = 60000;

            // Makes metadata fetch requests fail fast for dead connections and happen more frequently than azure event hub kills idle connections (240000 ms)
            ProducerConfig.RequestTimeoutMs = 5000;
            ProducerConfig.MetadataMaxAgeMs = 180000;

            // Makes producer send periodically keep alive signals to prevent firewall, gateway and load balancer closing tcp session (doesn't prevent azure event hub from killing idle connections)
            ProducerConfig.SocketKeepaliveEnable = true;


            ConsumerConfig.SocketTimeoutMs = 60000;
            ConsumerConfig.SessionTimeoutMs = 30000;

            ConsumerConfig.MetadataMaxAgeMs = 180000;
            // Makes consume calls happen more frequently than azure event hub kills idle connections (240000 ms)
            ConsumerConfig.MaxPollIntervalMs = 180000;

            // Makes producer send periodically keep alive signals to prevent firewall, gateway and load balancer closing tcp session (doesn't prevent azure event hub from killing idle connections)
            ConsumerConfig.SocketKeepaliveEnable = true;

            ConsumerConfig.AutoOffsetReset = AutoOffsetReset.Latest;
            ConsumerConfig.BrokerVersionFallback = "1.0.0";
        }
        public static EventClientSettings Create(string clientListFQDN, string userName, string password)
            // Note: don't remove ultil migrate all workers to the new recommended constructor
            => new EventClientSettings(clientListFQDN, userName, password);
        */
        private static string GetClientIdForConsumer(string producerAndConsumerPrefix = null)
        {
            var baseName = $"consumer-{Environment.MachineName}".ToLower();
            return string.IsNullOrWhiteSpace(producerAndConsumerPrefix)
                ? baseName
                : $"{producerAndConsumerPrefix}-{baseName}";
        }

        private static string GetClientIdForProducer(string producerAndConsumerPrefix)
        {
            var baseName = $"producer-{Environment.MachineName}".ToLower();
            return string.IsNullOrWhiteSpace(producerAndConsumerPrefix)
                ? baseName
                : $"{producerAndConsumerPrefix}-{baseName}";
        }
    }

    public sealed class EventClientCredential
    {
        public string Username { get; private set; }
        public string Password { get; private set; }

        public EventClientCredential(string userName, string password)
        {
            Username = userName;
            Password = password;
        }
    }

    public sealed class SchemaRegistryCredential
    {
        private readonly string _username;
        private readonly string _password;

        public string Url { get; }

        public SchemaRegistryCredential(string userName, string password, string url)
        {
            _username = userName;
            _password = password;
            Url = url;
        }

        public string GetBasicAuthUserInfo() => $"{_username}:{_password}";
    }
    public readonly struct EventClientRequest<TData>
    {
        public TData Data { get; }
        public string Topic { get; }
        public string Subject { get; }
        public bool ShouldCompressData { get; }
        public IEnumerable<KeyValuePair<string, string>> Metadata { get; }

        public EventClientRequest(
            TData data,
            string topic,
            string subject,
            bool shouldCompressData = true,
            IEnumerable<KeyValuePair<string, string>> metadata = null)
        {
            Data = data;
            Topic = topic;
            Subject = subject;
            ShouldCompressData = shouldCompressData;
            Metadata = metadata;
        }
    }
    public static class EventsTopics
    {
        public static class WeatherforecastResponded
        {
            public const string Name = "weatherforecast-responded";

            public enum Subjects
            {
                WeatherforecastResponded
            }
        }

    }
    public sealed class EventClientData
    {
        public Guid Id { get; set; }
        public string Topic { get; set; }
        public string Subject { get; set; }
        public string EventType { get; set; }
        public DateTime EventTime { get; set; }
        public string Data { get; set; }
        public bool? IsDataCompressed { get; set; }
        public string TraceKey { get; set; }

        //public static EventClientData Create<TData>(EventClientRequest<TData> request) =>
        //    Create(request, (data) => JsonSerializer.Serialize(data));

        public static EventClientData Create<TData>(EventClientRequest<TData> request, Func<object, string> serializer)
        {
            if (request.Data == null)
                throw new ArgumentException("'Data' should not be null.");

            if (string.IsNullOrWhiteSpace(request.Topic))
                throw new ArgumentException("'TopicName' is invalid.");

            if (string.IsNullOrWhiteSpace(request.Subject))
                throw new ArgumentException("'Subject' is invalid.");

            return new EventClientData
            {
                Id = Guid.NewGuid(),
                //Data = request.ShouldCompressData ? request.Data.ToCompressedHex(serializer) : serializer(request.Data),
                EventTime = DateTime.UtcNow,
                EventType = request.Data.GetType().Name,
                Subject = request.Subject,
                Topic = request.Topic,
                IsDataCompressed = request.ShouldCompressData
            };
        }
    }

    public readonly struct EventClientResponse<TData>
    {
        public TData Data { get; }
        public IEnumerable<KeyValuePair<string, string>> Metadata { get; }

        public EventClientResponse(
            TData data,
            IEnumerable<KeyValuePair<string, string>> metadata)
        {
            Data = data;
            Metadata = metadata;
        }
    }
    public class EventMessageBuilder : MessageBuilder<long, string>
    {
        private readonly Func<object, string> _serializer;

        public EventMessageBuilder(Func<object, string> serializer) =>
            _serializer = serializer;

        public EventMessageBuilder WithValue<TData>(EventClientRequest<TData> value)
        {
            var data = EventClientData.Create(value, _serializer);
            var serializedData = _serializer(data);
            Message.Value = serializedData;

            return this;
        }
    }
    public static class EventMessageHeader
    {
        public static byte[] Encode(string value) =>
            Encoding.UTF8.GetBytes(value);

        public static string Decode(byte[] value) =>
            Encoding.UTF8.GetString(value);
    }

    public class MessageBuilder<TKey, TValue>
    {
        protected readonly Message<TKey, TValue> Message;

        public MessageBuilder()
        {
            Message = new Message<TKey, TValue>()
            {
                Headers = new Headers()
            };
        }

        public MessageBuilder<TKey, TValue> WithKey(TKey key)
        {
            Message.Key = key;

            return this;
        }

        public MessageBuilder<TKey, TValue> WithValue(TValue value)
        {
            Message.Value = value;

            return this;
        }

        public MessageBuilder<TKey, TValue> WithTimestamp(Timestamp value)
        {
            Message.Timestamp = value;

            return this;
        }

        public MessageBuilder<TKey, TValue> AddHeader(Header header)
        {
            if (header is { })
                Message.Headers.Add(header);

            return this;
        }

        public MessageBuilder<TKey, TValue> AddHeaders(IEnumerable<KeyValuePair<string, string>> values)
        {
            if (values?.Any() is true)
                foreach (var item in values)
                    Message.Headers.Add(item.Key, EventMessageHeader.Encode(item.Value));

            return this;
        }

        public Message<TKey, TValue> Create() => Message;
    }
    #endregion
}
