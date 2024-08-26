using MessageModel.Model.DataBlockModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageModel.Model.Messages
{
    public class L2L2_RequestMessage : MessageBase
    {
        public L1L2_Request Request { get; set; }
        public L2L2_RequestMessage(L1L2_Request request, byte priority) : 
            base(priority, MessageType.RequestMessage)
        {
            Request = request;
        }
    }
}
