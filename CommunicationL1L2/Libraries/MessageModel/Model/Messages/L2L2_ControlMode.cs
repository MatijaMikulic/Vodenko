using MessageModel.Model.DataBlockModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageModel.Model.Messages
{
    public class L2L2_ControlMode:MessageBase
    {
        public L1L2_ControlMode ControlMode { get; set; }

        public L2L2_ControlMode(L1L2_ControlMode controlMode, byte priority) : base(priority, MessageType.ControlMode)
        {
            ControlMode = controlMode;
        }
    }
}
