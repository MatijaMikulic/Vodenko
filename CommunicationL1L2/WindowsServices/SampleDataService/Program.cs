using MessageBroker.Common.Configurations;
using MessageBroker.Common.Producer;
using MessageBroker.Common;
using Microsoft.Extensions.Configuration;
using SharedResources;
using DataAccess.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SampleDataService
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
                    services.AddLogging();

                    // 5)  Domain‑specific monitoring service
                    services.AddHostedService<SampleDataService.Services.SampleDataService>();
                })
                .UseWindowsService()   // no‑op if not running as service
                .UseSystemd()          // no‑op on Windows / when not under systemd
                .UseConsoleLifetime()  // falls back to CTRL‑C friendly console when not a service
                .Build();

            await host.RunAsync();
        }
    }
}