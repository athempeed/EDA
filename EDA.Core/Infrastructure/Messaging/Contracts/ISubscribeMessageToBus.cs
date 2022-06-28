namespace EDA.Core.Infrastructure.Messaging.Contracts
{
    public interface ISubscribeMessageToBus
    {
        void Subscribe<TMessage, THandler>()
           where TMessage : class
           where THandler : IMessageHandler<TMessage>;
    }
}
