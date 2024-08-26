using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using PlcCommunication;
using PLCTestService.Service;
using SharedResources;
using System;
using Topshelf;
using Unity;

namespace DataMonitoringService
{
    public class Program
    {
        static void Main(string[] args)
        {
            IConfiguration configuration = ConfigurationMng.GetConfiguration();

            var container = new UnityContainer();
            var plcConfig = BindOptions<PlcConfiguration>(configuration, "PlcConfiguration");

            container.RegisterInstance<IOptions<PlcConfiguration>>(Options.Create(plcConfig));

            container.RegisterType<PlcCommunicationService, PlcCommunicationService>();
            container.RegisterInstance<Test>(
                new Test(container.Resolve<PlcCommunicationService>()));

            var exitCode = HostFactory.Run(x =>
            {

                x.Service<Test>(s =>
                {
                    s.ConstructUsing(service => container.Resolve<Test>());
                    s.WhenStarted(async service => await service.Start());
                    s.WhenStopped(service => service.Stop());
                });

                x.RunAsLocalSystem();
                x.SetServiceName("TestPLC");
                x.SetDisplayName("TestPLC");
                x.SetDescription("Testing PLC Communication");
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