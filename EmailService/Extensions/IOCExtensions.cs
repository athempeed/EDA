using EasyNetQ;
using EDA.Core.Infrastructure.Messaging;
using EDA.Core.Infrastructure.Messaging.Contracts;
using EDA.Core.Infrastructure.Messaging.Messages;
using EDA.Core.Infrastructure.Messaging.RabbitMQ;
using EDA.Core.Infrastructure.RabbitMQ;
using EmailService.Handlers;
using EmailService.Modals;
using EmailService.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmailService.Extensions
{
    public static class IOCExtensions
    {
        public static void AddServices(this IServiceCollection services,IConfiguration configuration)
        {
            var d = new EmailOptions();
            services = services.AddOptions()
               .Configure<EmailOptions>(configuration.GetSection("EmailOptions"));

            services.AddSingleton<IGraphClient, GraphClient>();

            services.RegisterRabbitMq(configuration);
        }


        private static void RegisterRabbitMq(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("RabbitMQConnection");
            services.RegisterEasyNetQ(connectionString,
                r => { r.Register<IConventions, RabbitMQConventions>(); });

            services.AddSingleton<IPublishMessageToBus, RabbitMQWrapper>();
            services.AddSingleton<ISubscribeMessageToBus, RabbitMQWrapper>();
            //TODO: Put a condition for Accessory based on environment variable
            services.AddSingleton<IMessageHandler<EmailMessage>, EmailMessageHandler>();
        }
    }
}
