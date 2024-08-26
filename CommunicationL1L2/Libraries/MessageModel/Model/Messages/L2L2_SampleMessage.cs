using MessageModel.Model.DataBlockModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageModel.Model.Messages
{
    public class L2L2_SampleMessage:MessageBase
    {
        public L1L2_Sample Process { get; set; }
        public L2L2_SampleMessage(L1L2_Sample process, byte priority) 
            : base(priority, MessageType.SampleMessage)
        {
            Process = process;
        }
    }
}
