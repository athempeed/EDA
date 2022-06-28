using System.Diagnostics.CodeAnalysis;

namespace EDA.Core.Infrastructure.Messaging.Messages
{
    [ExcludeFromCodeCoverage]
    [MessageQueue(QueueName = "eda.emails", ExchangeName = "eda.app", QueueLengthLimit = 100000, RoutingKey = "edamails")]
    public class EmailMessage
    {
        public string Subject { get; set; }
        public string To { get; set; }
        public string From{ get; set; }
        public string Cc{ get; set; }
        public string Bcc{ get; set; }
        public string Body { get; set; }
    }
}
