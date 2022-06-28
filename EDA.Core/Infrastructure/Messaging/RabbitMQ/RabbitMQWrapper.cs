using EasyNetQ;
using EasyNetQ.Internals;
using EasyNetQ.Topology;
using EDA.Core.Infrastructure.Messaging;
using EDA.Core.Infrastructure.Messaging.Contracts;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Text;

namespace EDA.Core.Infrastructure.RabbitMQ
{
    public class RabbitMQWrapper: IPublishMessageToBus, ISubscribeMessageToBus
    {
        #region PRIVATE MEMBERS

        private readonly IAdvancedBus _advancedBus;
        private readonly ConcurrentDictionary<string, IQueue> _declaredQueues = new ConcurrentDictionary<string, IQueue>();
        private readonly IConventions _conventions;
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;

        #endregion

        public RabbitMQWrapper(IConventions conventions, IServiceProvider serviceProvider, ILogger<RabbitMQWrapper> logger, IBus rabbitBus)
        {
            _conventions = conventions ?? throw new ArgumentNullException(nameof(conventions));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger?? throw new ArgumentNullException(nameof(logger));
            _advancedBus = rabbitBus?.Advanced ?? throw new ArgumentNullException(nameof(rabbitBus));
        }

        public void Subscribe<TMessage, THandler>() where TMessage : class where THandler : IMessageHandler<TMessage>
        {
            try
            {
                var queue = DeclareQueue<TMessage>("");
                var exchange = DeclareExchange<TMessage>();
                var routingKey = _conventions.RpcRoutingKeyNamingConvention(typeof(TMessage));
                var handler = (THandler)GetService<IMessageHandler<TMessage>>();
                _advancedBus.Bind(exchange, queue, routingKey);
                _advancedBus.Consume(queue, (body, properties, info) =>
                {
                    var correlationId = properties?.CorrelationId;
                    var message = Deserialize<TMessage>(body);
                    handler.Handle(message);
                });
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed while consume the bus");
                throw;
            }
        }

        public void Publish<T>(T message) where T : class
        {
            if (message == null)
            {
                throw new InvalidOperationException($"Message is Null.");
            }
            var messagePublish = new Message<T>(message);
            var messageAttributes = message.GetType().GetAttribute<MessageQueueAttribute>();
            if (messageAttributes == null)
            {
                throw new InvalidOperationException($"Missing MessageQueueAttribute on message {message}");
            }

            if (string.IsNullOrWhiteSpace(messageAttributes.ExchangeName) ||
                string.IsNullOrWhiteSpace(messageAttributes.RoutingKey))
            {
                throw new InvalidOperationException($"Missing Exchange Name or Routing Key. Received Exchange Name :{messageAttributes.ExchangeName} and RoutingKey : {messageAttributes.RoutingKey}");
            }

            if (string.IsNullOrWhiteSpace(messagePublish.Properties.CorrelationId))
            {
                messagePublish.Properties.CorrelationId = Guid.NewGuid().ToString();
            }

            _logger.LogInformation("Started publishing {MessageType} with correlationId:{CorrelationId}", typeof(T), messagePublish.Properties.CorrelationId);

            var publisherExchange = new Exchange(messageAttributes.ExchangeName);
            _advancedBus.Publish(publisherExchange, messageAttributes.RoutingKey, false, messagePublish);

            _logger.LogInformation("Finished publishing {MessageType} with correlationId:{CorrelationId}", typeof(T), messagePublish.Properties.CorrelationId);
        }

        public void DeclareOutboundPublisherExchange<T>() where T : class
        {
            try
            {
                var exchange = DeclareExchange<T>();
                _logger.LogInformation($"Outbound Publisher Exchange Created. Name : {exchange?.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed while Declaring Outbound Publisher Exchange. Message Type : {typeof(T)}");
                throw;
            }
        }

        public void DeclareAppPublisherExchangeAndQueue<T>() where T : class 
        {
            try
            {
                var exchange = DeclareExchange<T>();
                _logger.LogInformation($"Publisher Exchange Created. Name : {exchange?.Name}");
                var queue = DeclareQueue<T>("");
                _logger.LogInformation($"Publisher Queue Created. Name : {queue?.Name}");
                var routingKey = _conventions.RpcRoutingKeyNamingConvention(typeof(T));
                _advancedBus.Bind(exchange, queue, routingKey);
                _logger.LogInformation($"Publisher Binding . Exchange : {exchange?.Name}. Queue : {queue?.Name}.RoutingKey:{routingKey}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed while Declaring Publisher Exchange");
            }
        }

        #region PRIVATE METHODS

        private T Deserialize<T>(byte[] body) where T : class
        {
            var bodyContent = Encoding.UTF8.GetString(body);
            try
            {
                
                if (!string.IsNullOrWhiteSpace(bodyContent))
                {
                    return JsonConvert.DeserializeObject<T>(bodyContent);
                }
                return default(T);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not deserialize the message body to the specified type of {typeof(T)}. Message body: {body}", ex);
            }
        }

        private IQueue DeclareQueue<T>(string subscriptionId)
        {
            return DeclareQueue(typeof(T), subscriptionId);
        }

        public IExchange DeclareExchange<T>()
        {
            var messageType = typeof(T);
            var name = _conventions.ExchangeNamingConvention(messageType);
            var messageAttributes = messageType.GetAttribute<MessageQueueAttribute>();

            if (messageAttributes.ExchangeType == ExchangeType.Header && string.IsNullOrWhiteSpace(messageAttributes.Header))
            {
                throw new InvalidOperationException("Exchange type declared as Header without any header arguments");
            }
            return _advancedBus.ExchangeDeclare(name, messageAttributes.ExchangeType ?? ExchangeType.Topic);
        }

        private IQueue DeclareQueue(Type messageType, string subscriptionId)
        {
            //validate queue attributes
            var queueAttribute = messageType.GetAttribute<MessageQueueAttribute>();
            if (queueAttribute == null)
                throw new InvalidOperationException($"MessageQueueAttribute is missing for type {messageType.FullName}");

            // Policy created to set queue length as 100000 in RMQ server. Queue length can be set at Policy level or Queue level.
            // RMQ uses whichever has minimum queue length.
            var queueLength = queueAttribute.QueueLengthLimit;
            if (queueLength <= 0)
                throw new InvalidOperationException($"Queue length limit must be greater than zero. Provided value: {queueLength}");
            var queuePriority = (int)queueAttribute.Priority;

            var queueName = _conventions.QueueNamingConvention(messageType, subscriptionId);

            IQueue queue = null;
            try
            {
                TryDeclareQueue();
            }
            catch (Exception e)
            {
                //When queue declaration attributes are different from those that the queue already has, a channel-level exception with code 406 (PRECONDITION_FAILED) will be raised and
                //in that case we'd need to drop the queue and re-create the queue with the new attributes.
                if (!e.Message.ToLowerInvariant().Contains("precondition_failed"))
                {
                    throw;
                }
                _logger.LogInformation("Declaring queue '{queueName}' failed. Deleting the queue and retrying again.", queueName);
                _advancedBus.QueueDelete(new Queue(queueName, false));
                //Try again
                TryDeclareQueue();
            }
            return queue;

            void TryDeclareQueue()
            {
                _declaredQueues.AddOrUpdate(
                    queueName,
                    key =>
                    {
                        return queue = _advancedBus.QueueDeclare(queueName, op =>
                        {
                            op.WithMaxLength(queueLength);   
                            op.WithMaxPriority(queuePriority);
                        });
                    },
                    (key, value) => queue = value);
            }
        }

        

        private T GetService<T>()
        {
            return (T)_serviceProvider.GetService(typeof(T));
        }

        #endregion
    }
}
