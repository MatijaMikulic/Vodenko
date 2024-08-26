using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageModel.Model.DataBlockModel
{
    [Serializable]
    public class L1L2_Sample : PlcData
    {
        public short Row { get; set; }
        public short Col { get; set; }

        public L1L2_Sample(short row, short col)
        {
            Row = row;
            Col = col;
        }

        public L1L2_Sample()
        {
        }
    }
}
