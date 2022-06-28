using EasyNetQ;
using EDA.Core.Infrastructure.Messaging.Contracts;
using EDA.Core.Infrastructure.Messaging.RabbitMQ;
using EDA.Core.Infrastructure.RabbitMQ;

namespace EDA.Extensions
{
    public static class IOCExtensions
    {
        public static void AddServices(this IServiceCollection services,IConfiguration configuration)
        {
            services.RegisterRabbitMq(configuration);
        }


        private static void RegisterRabbitMq(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("RabbitMQConnection");
            services.RegisterEasyNetQ(connectionString,
                r => { r.Register<IConventions, RabbitMQConventions>(); });
            services.AddSingleton<IPublishMessageToBus, RabbitMQWrapper>();
        }
    }
}
