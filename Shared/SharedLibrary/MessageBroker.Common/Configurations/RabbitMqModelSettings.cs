using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common.Configurations
{
    public class RabbitMqModelSettings
    {
        public string ExchangeName { get; set; }
        public Dictionary<string, string> RoutingKeyQueueMap { get; set; }
    }
}
