using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageModel.Model.DataBlockModel
{
    public class L2L1_ControllerParameters : PlcData
    {
        public ushort Method { get; set; }
        public float Proportional { get; set; }
        public float Integral { get; set; }
        public float Derivative { get; set; }
        public float K1 { get; set; }
        public float K2 { get; set; }
        public float K3 { get; set; }
        public float K4 { get; set; }
    }
}
