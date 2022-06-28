namespace EDA.Core.Infrastructure.Messaging
{
    public interface IMessageHandler<in TMessage> where TMessage : class
    {
        void Handle(TMessage message);
    }
}
