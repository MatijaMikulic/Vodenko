
using DataMonitoringService.Constants;
using DataMonitoringService.Services;
using MessageBroker.Common;
using MessageBroker.Common.Configurations;
using MessageBroker.Common.Producer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using PlcCommunication;
using System;
using Topshelf;
using Unity;
using TaskLog;
using TaskLog.Contracts;
using SharedResources;

namespace DataMonitoringService
{
    public class Program
    {
        static void Main(string[] args)
        {
            IConfiguration configuration = ConfigurationMng.GetConfiguration();

            var container = new UnityContainer();
            var rabbitMqConfig = BindOptions<RabbitMqConfiguration>(configuration, "RabbitMqConfiguration");
            var plcConfig = BindOptions<PlcConfiguration>(configuration, "PlcConfiguration");
            var sendConfig = BindOptions<RabbitMqModelSettings>(configuration, "RabbitMqModelSenderConfig");

            container.RegisterInstance<IOptions<PlcConfiguration>>(Options.Create(plcConfig));
            container.RegisterInstance<IOptions<RabbitMqConfiguration>>(Options.Create(rabbitMqConfig));
            container.RegisterInstance<IOptions<RabbitMqModelSettings>>(Options.Create(sendConfig));

            container.RegisterType<PlcCommunicationService, PlcCommunicationService>();
            container.RegisterType<IRabbitMqService, RabbitMqService>();
            container.RegisterType<IProducerConsumer, RabbitMqProducerConsumer>();
            container.RegisterType<ILogger,ConsoleLogger>();

            container.RegisterInstance<DMService>(
                new DMService(container.Resolve<IProducerConsumer>(), 
                              container.Resolve<PlcCommunicationService>(),
                              container.Resolve<ILogger>()));

            var exitCode = HostFactory.Run(x =>
            {

                x.Service<DMService>(s =>
                {
                    s.ConstructUsing(service => container.Resolve<DMService>());
                    s.WhenStarted(async service => await service.Start());
                    s.WhenStopped(service => service.Stop());
                });

                x.RunAsLocalSystem();
                x.SetServiceName(DataMonitoringServiceInfo.ServiceName);
                x.SetDisplayName(DataMonitoringServiceInfo.DisplayName);
                x.SetDescription(DataMonitoringServiceInfo.Description);
                x.StartAutomatically();


            });

            int exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
            Environment.ExitCode = exitCodeValue;
        }

        private static T BindOptions<T>(IConfiguration configuration, string sectionName) where T : class, new()
        {
            var section = configuration.GetSection(sectionName);
            var options = new T();
            configuration.Bind(sectionName, options);
            return options;
        }
    }
}