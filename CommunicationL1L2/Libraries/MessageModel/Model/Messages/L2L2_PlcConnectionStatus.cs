using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageModel.Model.Messages
{
    public class L2L2_PlcConnectionStatus:MessageBase
    {
        public bool IsConnected { get; set; }

        public L2L2_PlcConnectionStatus(bool isConnected, byte priority):base(priority,MessageType.PlcConnectionStatus)
        {
                IsConnected = isConnected;
        }
    }
}
