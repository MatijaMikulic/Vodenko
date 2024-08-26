using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageModel.Model.DataBlockModel
{
    public class L2L1_SetPoint:PlcData
    {
        public float TargetH2Level { get; set; }
        public float pvInitialValue { get; set; }
        public float pvFinalValue { get; set; }
        public ushort Mode { get; set; }
    }
    
}
