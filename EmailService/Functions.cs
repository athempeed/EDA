using EDA.Core.Infrastructure.Messaging.Contracts;
using EDA.Core.Infrastructure.Messaging.Messages;
using EmailService.Handlers;
using Microsoft.Extensions.Hosting;

namespace EmailService
{
    public class EmailHostedService : BackgroundService
    {
        private readonly ISubscribeMessageToBus _bus;
        private readonly IPublishMessageToBus _publishToMessageBus;

        public EmailHostedService(ISubscribeMessageToBus bus, IPublishMessageToBus publishToMessageBus)
        {
            _bus = bus;
            _publishToMessageBus = publishToMessageBus;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _bus.Subscribe<EmailMessage, EmailMessageHandler>();
            _publishToMessageBus.DeclareOutboundPublisherExchange<EmailMessage>();

            return Task.CompletedTask;
        }
    }
}
