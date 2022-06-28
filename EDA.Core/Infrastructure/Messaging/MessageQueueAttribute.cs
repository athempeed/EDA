using System.Diagnostics.CodeAnalysis;

namespace EDA.Core.Infrastructure.Messaging
{
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple =false)]
    internal class MessageQueueAttribute:Attribute
    {
        public string? QueueName { get; set; }
        public int QueueLengthLimit { get; set; }
        public string? RoutingKey { get; set; }
        public byte Priority { get; set; }
        public string? ExchangeName { get; set; }
        public string? ExchangeType { get; set; }
        public string? Header { get; set; }
    }
}
