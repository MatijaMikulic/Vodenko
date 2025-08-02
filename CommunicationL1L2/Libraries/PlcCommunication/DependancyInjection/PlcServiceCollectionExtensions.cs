using MessageModel.Model.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using V3.S7Plc.Communication.Core;
using V3.S7Plc.Communication.Entities;
using V3.S7Plc.Communication.Interfaces;
using V3.S7Plc.Communication.Templates;
using V3.S7Plc.Communication.Templates.Interfaces;
using V3.S7Plc.Communication.Buffer;

namespace V3.S7Plc.Communication.DependancyInjection
{
    public static class PlcServiceCollectionExtensions
    {
        /// <summary>
        /// Registers PLC client, connection, mapper, and buffer strategy factory.
        /// Uses attribute-based mapping—no manual offsets needed in your app code.
        /// </summary>
        public static IServiceCollection AddPlcClient(this IServiceCollection services, IConfiguration config)
        {
            // 1) Connection options from appsettings.json
           // services.Configure<PlcConnectionOptions>(config.GetSection("Plc:Connection"));

            // 2) Low-level S7.NetPlus connection
            services.AddSingleton<IPlcConnection>(sp =>
            {
                var opts = sp.GetRequiredService<IOptions<PlcConnectionOptions>>().Value;
                return new S7PlcConnection(opts);
            });

            // 3) Attribute-based mapper
            services.AddSingleton<IDataBlockMapper, AttributeDataBlockMapper>();

            // 4) Buffer strategy factory
            services.AddSingleton<IBufferStrategyFactory, BufferStrategyFactory>();

            // 5) High-level client
            services.AddSingleton<IPlcClient, PlcClient>();


            services.AddSingleton<IBufferTemplate, CircularBufferTemplate>();
            services.AddSingleton<IBufferTemplate, DataBufferTemplate>();

            services.AddSingleton<BufferTemplateFactory>();

            return services;
        }
    }
}
