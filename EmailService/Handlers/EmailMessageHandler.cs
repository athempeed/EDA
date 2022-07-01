using EDA.Core.Infrastructure.Messaging;
using EDA.Core.Infrastructure.Messaging.Messages;
using EmailService.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EmailService.Handlers
{
    public class EmailMessageHandler : IMessageHandler<EmailMessage>
    {
        private readonly ILogger<EmailMessageHandler> _logger;
        private readonly IGraphClient _graphClient;

        public EmailMessageHandler(ILogger<EmailMessageHandler> logger, IGraphClient graphClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _graphClient = graphClient ?? throw new ArgumentNullException(nameof(logger)); 
        }

        public void Handle(EmailMessage message)
        {
            try
            {
                if(message == null)
                {
                    _logger.LogError($"Message is null.");
                    return;
                }

                _logger.LogInformation($"processing email : {JsonConvert.SerializeObject(message)}");
                _graphClient.SendMail(message);
                _logger.LogInformation($"email sent successfully to : ${message.To}");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Excetion occured while sending the email");
                throw;
            }

        }
    }
}
