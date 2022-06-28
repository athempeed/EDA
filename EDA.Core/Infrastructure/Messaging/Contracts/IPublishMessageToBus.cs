using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDA.Core.Infrastructure.Messaging.Contracts
{
    public interface IPublishMessageToBus
    {
        void Publish<T>(T message) where T : class;
        void DeclareOutboundPublisherExchange<T>() where T : class;
        void DeclareAppPublisherExchangeAndQueue<T>() where T : class;
    }
}
