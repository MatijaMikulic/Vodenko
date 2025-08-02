
namespace DataMonitoringService
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using MessageBroker.Common;
    using MessageBroker.Common.Configurations;
    using MessageBroker.Common.Producer;
    using SharedResources;
    using V3.S7Plc.Communication.DependancyInjection;

    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration( cfg => ConfigurationMng.GetConfiguration())
                .ConfigureServices((ctx, services) =>
                {
                    IConfiguration configuration = ctx.Configuration;

                    // 1)  PLC‑communication library
                    services.AddPlcClient(
                        plc => configuration
                            .GetSection("PlcConfiguration")
                            .Bind(plc),
                        builder =>
                        {
                            configuration
                                .GetSection("PlcLibrarySettings")
                                .Bind(builder);

                            //builder.DisableHeartbeat();
                        }
                    );

                    // 2)  RabbitMQ options + abstractions
                    services.Configure<RabbitMqConfiguration>(
                        configuration.GetSection("RabbitMqConfiguration"));
                    services.Configure<RabbitMqModelSettings>(
                        configuration.GetSection("RabbitMqModelSenderConfig"));

                    services.AddSingleton<IRabbitMqService, RabbitMqService>();
                    services.AddSingleton<IProducerConsumer, RabbitMqProducerConsumer>();

                    // 3)  Logging
                    services.AddLogging();

                    // 4)  Domain‑specific monitoring service
                    services.AddHostedService<DataMonitoringService.Services.DataMonitoringService>();
                })
                .UseWindowsService()   // no‑op if not running as service
                .UseSystemd()          // no‑op on Windows / when not under systemd
                .UseConsoleLifetime()  // falls back to CTRL‑C friendly console when not a service
                .Build();

            await host.RunAsync();
        }
    }
}