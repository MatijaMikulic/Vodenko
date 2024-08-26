using MessageModel.Model.DataBlockModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageModel.Model.Messages
{
    public class L2L2_ControllerParams : MessageBase
    {
        public L1L2_ControllerParams ControllerParams { get; set; }

        public L2L2_ControllerParams(L1L2_ControllerParams controllerParams,byte priority) : base(priority, MessageType.ControllerParams)
        {
            ControllerParams = controllerParams;
        }
    }
}
