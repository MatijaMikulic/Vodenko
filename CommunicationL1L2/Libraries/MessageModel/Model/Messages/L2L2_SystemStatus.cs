using MessageModel.Model.DataBlockModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageModel.Model.Messages
{
    public class L2L2_SystemStatus:MessageBase
    {
        public L1L2_SystemStatus SystemStatus { get; set; }
        public L2L2_SystemStatus(L1L2_SystemStatus sysStatus, byte priority)
            : base(priority, MessageType.SystemStatus)
        {
            SystemStatus = sysStatus;
        }
    }
}
