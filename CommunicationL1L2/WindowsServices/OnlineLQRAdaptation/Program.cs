using MessageBroker.Common;
using MessageBroker.Common.Configurations;
using MessageBroker.Common.Producer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OnlineLQRAdaptation.Services;
using SharedResources;
using DataAccess.Repositories;
using DataAccess.Configurations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TaskLog.Contracts;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OnlineLQRAdaptation
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(cfg => ConfigurationMng.GetConfiguration())
                .ConfigureServices((ctx, services) =>
                {
                    IConfiguration configuration = ctx.Configuration;

                    // 2)  RabbitMQ options + abstractions
                    services.Configure<RabbitMqConfiguration>(
                        configuration.GetSection("RabbitMqConfiguration"));
                    services.Configure<RabbitMqModelSettings>(
                        configuration.GetSection("RabbitMqModelSenderConfig"));

                    services.TryAddSingleton<IRabbitMqService, RabbitMqService>();
                    services.TryAddSingleton<IProducerConsumer, RabbitMqProducerConsumer>();
                    
                    // 3) Database repository
                    services.TryAddSingleton<DatabaseRepositories>();

                    // 4)  Logging
                    services.TryAddSingleton<ILogger, ConsoleLogger>();

                    // 5)  Domain‑specific monitoring service
                    services.AddHostedService<OLQRService>();
                })
                .UseWindowsService()   // no‑op if not running as service
                .UseSystemd()          // no‑op on Windows / when not under systemd
                .UseConsoleLifetime()  // falls back to CTRL‑C friendly console when not a service
                .Build();
                
            await host.RunAsync();
                
        }
    }
}
