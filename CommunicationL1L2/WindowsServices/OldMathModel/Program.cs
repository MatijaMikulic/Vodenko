using MessageBroker.Common;
using MessageBroker.Common.Configurations;
using MessageBroker.Common.Producer;
using MessageManagerService.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SharedResources;
using System;
using MathModelOnlineInfo.Constants;
using Topshelf;
using Unity;
using Unity.Injection;

namespace MathModelOnline
{
    public class Program
    {
        static void Main(string[] args)
        {
            IConfiguration configuration = ConfigurationMng.GetConfiguration();

            var container = new UnityContainer();
            var rabbitMqConfig = BindOptions<RabbitMqConfiguration>(configuration, "RabbitMqConfiguration");
            var listenConfig = BindOptions<RabbitMqModelSettings>(configuration, "RabbitMqModelSenderConfig");

            container.RegisterInstance<IOptions<RabbitMqConfiguration>>(Options.Create(rabbitMqConfig));
            container.RegisterInstance<IOptions<RabbitMqModelSettings>>(Options.Create(listenConfig));

            container.RegisterType<IRabbitMqService, RabbitMqService>();
            container.RegisterType<IProducerConsumer, RabbitMqProducerConsumer>();
            container.RegisterType<IProducerConsumer, RabbitMqProducerConsumer>();
            container.RegisterInstance<MathService>(
                new MathService(container.Resolve<IProducerConsumer>()));


            var exitCode = HostFactory.Run(x =>
            {

                x.Service<MathService>(s =>
                {
                    s.ConstructUsing(service => container.Resolve<MathService>());
                    s.WhenStarted(async service => await service.Start());
                    s.WhenStopped(service => service.Stop());


                });

                x.RunAsLocalSystem();
                x.SetServiceName("MathModelOnlineInfo.ServiceName");
                x.SetDisplayName("MathModelOnlineInfo.DisplayName");
                x.SetDescription("MathModelOnlineInfo.Description");
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