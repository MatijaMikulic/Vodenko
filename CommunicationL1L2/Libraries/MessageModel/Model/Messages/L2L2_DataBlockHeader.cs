using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageModel.Model.Messages
{
    public class L2L2_DataBlockHeader : MessageBase
    {
        public ushort DB { get; set; }
        public ushort BufferPointer { get; set; }
        public L2L2_DataBlockHeader(ushort db, ushort bufferPointer, byte priority)
            : base(priority, MessageType.DataBlockHeader)
        {
            DB = db;
            BufferPointer = bufferPointer;
        }

        public override string ToString()
        {
            return $"{DB}, {BufferPointer}";
        }

    }
}
