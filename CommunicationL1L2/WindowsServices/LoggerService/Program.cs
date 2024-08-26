using LoggerService.Constants;
using LoggerService.Services;
using MessageBroker.Common;
using MessageBroker.Common.Configurations;
using MessageBroker.Common.Producer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SharedResources;
using System;
using TaskLog.Configurations;
using TaskLog.Contracts;
using Topshelf;
using Unity;
using Unity.Injection;

namespace LoggerService
{
    public class Program
    {
        static void Main(string[] args)
        {
            IConfiguration configuration = ConfigurationMng.GetConfiguration();

            var container = new UnityContainer();
            var rabbitMqConfig = BindOptions<RabbitMqConfiguration>(configuration, "RabbitMqConfiguration");
            var listenConfig = BindOptions<RabbitMqModelSettings>(configuration, "RabbitMqModelLoggerConfig");
            var logConfig = BindOptions<FileLoggerConfiguration>(configuration, "FileLogger");

            container.RegisterInstance<IOptions<RabbitMqConfiguration>>(Options.Create(rabbitMqConfig));
            container.RegisterInstance<IOptions<RabbitMqModelSettings>>(Options.Create(listenConfig));
            container.RegisterInstance<IOptions<FileLoggerConfiguration>>(Options.Create(logConfig));

            container.RegisterType<IRabbitMqService, RabbitMqService>();
            container.RegisterType<ILogger, FileLogger>();
            container.RegisterType<IProducerConsumer, RabbitMqProducerConsumer>();
            container.RegisterInstance<Service>(
                new Service(container.Resolve<IProducerConsumer>(), container.Resolve<ILogger>()));


            var exitCode = HostFactory.Run(x =>
            {

                x.Service<Service>(s =>
                {
                    s.ConstructUsing(service => container.Resolve<Service>());
                    s.WhenStarted(async service => await service.Start());
                    s.WhenStopped(service => service.Stop());


                });

                x.RunAsLocalSystem();
                x.SetServiceName(LoggerServiceInfo.ServiceName);
                x.SetDisplayName(LoggerServiceInfo.DisplayName);
                x.SetDescription(LoggerServiceInfo.Description);
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