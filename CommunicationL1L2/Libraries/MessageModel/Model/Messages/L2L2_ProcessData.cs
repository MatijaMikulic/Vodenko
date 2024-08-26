using MessageModel.Model.DataBlockModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageModel.Model.Messages
{
    public class L2L2_ProcessData:MessageBase
    {
        public L1L2_ProcessData ProcessData { get; set; }
        public L2L2_ProcessData(L1L2_ProcessData processData, byte priority)
            : base(priority, MessageType.ProcessData)
        {
            ProcessData = processData;
        }
    }
}
