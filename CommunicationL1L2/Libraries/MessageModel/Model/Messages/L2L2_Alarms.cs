using MessageModel.Model.DataBlockModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageModel.Model.Messages
{
    public class L2L2_Alarms:MessageBase
    {
        public L1L2_Alarms Alarm { get; set; }
        public L2L2_Alarms(L1L2_Alarms alarms, byte priority)
            : base(priority, MessageType.Alarms)
        {
            Alarm = alarms;
        }

    }
}
