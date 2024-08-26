using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common
{
    /// <summary>
    /// Defines the interface for a RabbitMQ service, providing methods for creating a RabbitMQ connection.
    /// </summary>
    public interface IRabbitMqService
    {
        public IConnection CreateConnection(string clientName = "Client");

    }
}
