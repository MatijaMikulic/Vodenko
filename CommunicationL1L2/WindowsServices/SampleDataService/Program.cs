using MessageBroker.Common.Configurations;
using MessageBroker.Common.Producer;
using MessageBroker.Common;
using MessageModel.Model.DataBlockModel;
using MessageModel.Model.Messages;
using MessageModel.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SharedResources;
using SharedResources.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using SampleDataService.Services;
using Topshelf;
using DataAccess.Configurations;
using DataAccess.Repositories;
using SampleDataService.Constants;

namespace SampleDataService
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IConfiguration configuration = ConfigurationMng.GetConfiguration();

            var container = new UnityContainer();
            var rabbitMqConfig = BindOptions<RabbitMqConfiguration>(configuration, "RabbitMqConfiguration");
            var sendConfig = BindOptions<RabbitMqModelSettings>(configuration, "RabbitMqModelSenderConfig");
            var db = BindOptions<DBConfiguration>(configuration, "Dapper");


            container.RegisterInstance<IOptions<RabbitMqConfiguration>>(Options.Create(rabbitMqConfig));
            container.RegisterInstance<IOptions<RabbitMqModelSettings>>(Options.Create(sendConfig));
            container.RegisterInstance<IOptions<DBConfiguration>>(Options.Create(db));


            container.RegisterType<IRabbitMqService, RabbitMqService>();
            container.RegisterType<IProducerConsumer, RabbitMqProducerConsumer>();
            container.RegisterType<DatabaseRepositories, DatabaseRepositories>();

            container.RegisterInstance<SDService>(
                new SDService(container.Resolve<IProducerConsumer>(),container.Resolve<DatabaseRepositories>()));


            var exitCode = HostFactory.Run(x =>
            {

                x.Service<SDService>(s =>
                {
                    s.ConstructUsing(service => container.Resolve<SDService>());
                    s.WhenStarted(async service => await service.Start());
                    s.WhenStopped(service => service.Stop());


                });

                x.RunAsLocalSystem();
                x.SetServiceName(SampleDataInfo.ServiceName);
                x.SetDisplayName(SampleDataInfo.DisplayName);
                x.SetDescription(SampleDataInfo.Description);
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