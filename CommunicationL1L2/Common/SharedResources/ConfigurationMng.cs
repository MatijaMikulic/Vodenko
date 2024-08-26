using Microsoft.Extensions.Configuration;

namespace SharedResources
{
    /// <summary>
    /// Provides method for managing configuration settings.
    /// </summary>
    public static class ConfigurationMng
    {
        private static readonly string _appSettingsFileName = "appsettings.json";

        /// <summary>
        /// Retrieves the configuration from the appsettings.json file.
        /// </summary>
        /// <returns>An IConfiguration instance representing the appsettings configuration.</returns>
        public static IConfiguration GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(_appSettingsFileName, optional: false, reloadOnChange: true)
                .Build();

            return builder;
        }
    }
}