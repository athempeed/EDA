using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;

namespace EmailService
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                GetData();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public static void GetData()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (!string.IsNullOrWhiteSpace(environment))
            {
                Console.WriteLine($"environment mentioned in ASPNETCORE{environment}");
            }
            else
            {
                environment = (string.IsNullOrWhiteSpace(environment) ? "Development" : environment);
            }
            Console.WriteLine($"current environment :{environment}");
            var env = string.Format("appsettings.{0}.json", environment);
            Console.WriteLine("current environment using env : {0}", env);

            var builder = new HostBuilder();
            builder.ConfigureWebJobs((job) =>
            {
                job.AddAzureStorageCoreServices();
                job.AddAzureStorageCoreServices();
                job.AddTimers();
            }).ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile(env, optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
            }).ConfigureLogging((context, log) =>
            {
                log.AddConsole();
                log.SetMinimumLevel(LogLevel.Information);
                log.AddEventLog(new EventLogSettings()
                {
                    SourceName = "Email Service"
                });
            }).ConfigureServices((context, services) =>
            {
                Startup.ConfigureService(context,services);
            });

            var host = builder.Build();
            using (host)
            {
                host.Run();
            }
        }
    }
}