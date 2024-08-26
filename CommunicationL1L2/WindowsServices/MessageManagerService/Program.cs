using MessageBroker.Common;
using MessageBroker.Common.Configurations;
using MessageBroker.Common.Producer;
using MessageManagerService.Constants;
using MessageManagerService.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using PlcCommunication;
using SharedResources;
using Topshelf;
using Unity;

namespace MessageManagerService
{
    internal class Program
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
            container.RegisterInstance<MMService>(
                new MMService(container.Resolve<IProducerConsumer>(),container.Resolve<PlcCommunicationService>()));

            var exitCode = HostFactory.Run(x =>
            {

                x.Service<MMService>(s =>
                {
                    s.ConstructUsing(service => container.Resolve<MMService>());
                    s.WhenStarted(async service => await service.Start());
                    s.WhenStopped(service => service.Stop());
                });

                x.RunAsLocalSystem();
                x.SetServiceName(MessageManagerInfo.ServiceName);
                x.SetDisplayName(MessageManagerInfo.DisplayName);
                x.SetDescription(MessageManagerInfo.Description);
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