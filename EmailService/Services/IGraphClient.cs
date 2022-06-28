using EDA.Core.Infrastructure.Messaging.Messages;

namespace EmailService.Services
{
    public interface IGraphClient
    {
        Task SendMail(EmailMessage message);
    }
}