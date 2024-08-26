
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlcCommunication
{
    public  class PlcCommunicationException:Exception
    {
        public PlcConfiguration PlcConfiguration { get; }
        public PlcCommunicationException(string message, PlcConfiguration plcConfiguration, Exception? innerException = null)
        : base(message, innerException)
        {
            PlcConfiguration = plcConfiguration;
        }

    }
}
