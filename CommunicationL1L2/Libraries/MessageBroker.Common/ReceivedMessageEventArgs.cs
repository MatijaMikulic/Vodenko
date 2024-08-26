using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common
{
    /// <summary>
    /// Event arguments for the event raised when a message is received.
    /// Contains body property which represents received message.
    /// </summary>
    public class ReceivedMessageEventArgs:EventArgs
    {
        public byte[] body { get; set; }
    }
}
