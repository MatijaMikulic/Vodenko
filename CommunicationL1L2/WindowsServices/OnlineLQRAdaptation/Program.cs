using MessageBroker.Common;
using MessageBroker.Common.Configurations;
using MessageBroker.Common.Producer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OnlineLQRAdaptation.Services;
using SharedResources;
using System;
using Topshelf;
using Unity;
using Unity.Injection;
using DataAccess.Repositories;
using DataAccess.Configurations;

namespace OnlineLQRAdaptation
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IConfiguration configuration = ConfigurationMng.GetConfiguration();

            var container = new UnityContainer();
            var rabbitMqConfig = BindOptions<RabbitMqConfiguration>(configuration, "RabbitMqConfiguration");
            var listenConfig = BindOptions<RabbitMqModelSettings>(configuration, "RabbitMqModelSenderConfig");
            var db = BindOptions<DBConfiguration>(configuration, "Dapper");


            container.RegisterInstance<IOptions<RabbitMqConfiguration>>(Options.Create(rabbitMqConfig));
            container.RegisterInstance<IOptions<RabbitMqModelSettings>>(Options.Create(listenConfig));
            container.RegisterInstance<IOptions<DBConfiguration>>(Options.Create(db));



            container.RegisterType<IRabbitMqService, RabbitMqService>();
            container.RegisterType<IProducerConsumer, RabbitMqProducerConsumer>();
            container.RegisterType<DatabaseRepositories, DatabaseRepositories>();

            container.RegisterInstance<OLQRService>(
                new OLQRService(container.Resolve<IProducerConsumer>(),container.Resolve<DatabaseRepositories>()));


            var exitCode = HostFactory.Run(x =>
            {

                x.Service<OLQRService>(s =>
                {
                    s.ConstructUsing(service => container.Resolve<OLQRService>());
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
