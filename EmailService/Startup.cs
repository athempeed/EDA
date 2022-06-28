using EmailService.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EmailService
{
    public class Startup
    {
        public static void ConfigureService(HostBuilderContext context, IServiceCollection services)
        {
            services.AddHostedService<EmailHostedService>();
            services.AddServices(context.Configuration);
        }
    }
}
