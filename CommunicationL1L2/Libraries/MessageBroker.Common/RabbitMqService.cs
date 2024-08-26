using MessageBroker.Common.Configurations;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common
{
    /// <summary>
    /// Implementation of the IRabbitMqService interface for creating and managing RabbitMQ connections.
    /// </summary>
    public class RabbitMqService : IRabbitMqService
    {
        private readonly RabbitMqConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the RabbitMqService class.
        /// </summary>
        /// <param name="options">The configuration options for RabbitMQ.</param>
        public RabbitMqService(IOptions<RabbitMqConfiguration> options )
        {
            _configuration = options.Value;
            
        }

        /// <summary>
        /// Creates a connection to the RabbitMQ server using the configured settings.
        /// Currently sets to asynchronous listening by default.
        /// </summary>
        /// <returns>An instance of IConnection representing the RabbitMQ connection.</returns>
        public IConnection CreateConnection(string clientName = "Client")
        {
            ConnectionFactory connectionFactory = new ConnectionFactory()
            {
                UserName = _configuration.Username,
                Password = _configuration.Password,
                HostName = _configuration.HostName,
                ClientProvidedName = clientName,
                AutomaticRecoveryEnabled = true, 
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5) 
            };
            connectionFactory.RequestedHeartbeat = TimeSpan.FromSeconds(5);
            connectionFactory.RequestedConnectionTimeout = Timeout.InfiniteTimeSpan;//TimeSpan.FromMinutes(5);

            //Receiving data async (for Consumer)
            connectionFactory.DispatchConsumersAsync = true;

            var connection = connectionFactory.CreateConnection();
            return connection;      
        }
    }
}
