using EasyNetQ;
using System.Diagnostics.CodeAnalysis;

namespace EDA.Core.Infrastructure.Messaging.RabbitMQ
{
    [ExcludeFromCodeCoverage]
    public class RabbitMQConventions:Conventions
    {
        public RabbitMQConventions(ITypeNameSerializer typeNameSerializer) : base(typeNameSerializer)
        {
            QueueNamingConvention = (messageType, subscriptionId) =>
            {
                var attribute = GetMessageQueueAttribute(messageType);
                var queueName = attribute.QueueName?.Trim() ?? throw new InvalidOperationException($"Type '{messageType.FullName}' is missing the queue name");
                return string.IsNullOrEmpty(subscriptionId) ? queueName : $"{queueName}_{subscriptionId}";
            };
            ExchangeNamingConvention = messageType => GetMessageQueueAttribute(messageType).ExchangeName?.Trim() ?? throw new InvalidOperationException($"Type '{messageType.FullName}' is missing the exchange name");
            RpcRoutingKeyNamingConvention = messageType => GetMessageQueueAttribute(messageType).RoutingKey?.Trim() ?? "";
            ConsumerTagConvention = () => $"Machine: {Environment.MachineName} User: {Environment.UserName}";
        }
        private static MessageQueueAttribute GetMessageQueueAttribute(Type messageType)
            => (MessageQueueAttribute)(messageType.GetCustomAttributes(typeof(MessageQueueAttribute), true).SingleOrDefault()
                                       ?? throw new InvalidOperationException($"Type '{messageType.FullName} is missing the mandatory MessageQueueAttribute'"));
    }
}
