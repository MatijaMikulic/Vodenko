using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using PlcCommunication.Interfaces;

namespace PlcCommunication
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPlcCommunication(
          this IServiceCollection services,
          Action<PlcConfiguration> configurePlc,
          Action<PlcCommunicationBuilder>? configureLibrary = null)
        {
            services.Configure(configurePlc);

            if (configureLibrary != null)
            {
                var opts = new PlcCommunicationBuilder();
                configureLibrary(opts);
                services.Configure<PlcCommunicationOptions>(o =>
                {
                    var built = opts.Build();
                    o.EnableHeartbeat = built.EnableHeartbeat;
                    o.HeartbeatInterval = built.HeartbeatInterval;
                    foreach (var block in built.DataBlocks)
                        o.DataBlocks.Add(block);
                });
            }

            services.AddSingleton<IConnectionManager, PlcConnectionManager>();
            services.AddSingleton<IPlcDataAccess, PlcDataAccess>();

            services.AddSingleton<IHeartbeat>(sp =>
            {
                var libOpts = sp.GetRequiredService<IOptions<PlcCommunicationOptions>>().Value;
                return new Heartbeat(
                    sp.GetRequiredService<IConnectionManager>(),
                    libOpts.HeartbeatInterval);
            });
            services.AddSingleton<IHostedService, HeartbeatHostedService>();
            return services;
        }
    }
}
