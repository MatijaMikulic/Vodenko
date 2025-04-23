using DataAccess.Configurations;
using DataAccess.Repositories;
using MessageBroker.Common;
using MessageBroker.Common.Configurations;
using MessageBroker.Common.Producer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PlcCommunication;
using SendManagerService.Services;
using SharedResources;
using TaskLog.Contracts;

namespace SendManagerService
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(cfg =>
                {
                    cfg.AddConfiguration(ConfigurationMng.GetConfiguration());
                })
                .ConfigureServices((ctx, services) =>
                {
                    IConfiguration configuration = ctx.Configuration;

                    // 1) PLC‐communication library
                    services.AddPlcCommunication(
                        plc => configuration.GetSection("PlcConfiguration").Bind(plc),
                        builder => configuration.GetSection("PlcLibrarySettings").Bind(builder)
                    );

                    // 2) RabbitMQ and DB options
                    services.Configure<RabbitMqConfiguration>(
                        configuration.GetSection("RabbitMqConfiguration"));
                    services.Configure<RabbitMqModelSettings>(
                        configuration.GetSection("RabbitMqModelSenderConfig"));
                    services.Configure<DBConfiguration>(
                        configuration.GetSection("Dapper"));

                    // 3) Services and repositories
                    services.AddSingleton<DatabaseRepositories>();
                    services.AddSingleton<IRabbitMqService, RabbitMqService>();
                    services.AddSingleton<IProducerConsumer, RabbitMqProducerConsumer>();
                    

                    // 4) Core manager service
                    services.AddSingleton<ILogger, ConsoleLogger>();    
                    services.AddSingleton<SendManager>();
                    services.AddHostedService<SendHostedService>();
                })
                .UseWindowsService()
                .UseSystemd()
                .UseConsoleLifetime()
                .Build();

            await host.RunAsync();
        }
    }
}