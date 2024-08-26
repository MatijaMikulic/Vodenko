using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageModel.Model.DataBlockModel
{
    public class L1L2_ControlMode:PlcData
    {
        public ushort ControlMode { get; set; }

        public L1L2_ControlMode(ushort controlMode)
        {
            ControlMode = controlMode;
        }

        public L1L2_ControlMode() { }
    }
}
