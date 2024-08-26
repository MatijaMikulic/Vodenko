using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageModel.Model.Messages
{
    public class L2L2_ServiceStatus:MessageBase
    {
        public bool IsAlive { get; set; }

        public L2L2_ServiceStatus(bool isAlive, byte priority)
            : base(priority, MessageType.ServiceStatus)
        {
            IsAlive = isAlive;
        }
    }
}
