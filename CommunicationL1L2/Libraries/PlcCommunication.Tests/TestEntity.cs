using V3.S7Plc.Communication.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlcCommunication.Tests
{
    [PlcDataBlockAttribute(10,V3.S7Plc.Communication.Enums.StructureType.CircularBuffer,5)]
    public class TestEntity
    {
        [PlcField(offset:0,BitIndex = 0)]
        public ushort Id { get; set; }

        [PlcField(offset: 2, BitIndex = 0)]
        public bool IsActive { get; set; }

        [PlcField(offset: 4, BitIndex = 0)]
        public float Temperature { get; set; }

        [PlcField(offset: 8, BitIndex = 0)]
        public float Tolerance { get; set; }
    }

    [PlcDataBlockAttribute(10, V3.S7Plc.Communication.Enums.StructureType.CircularBuffer, 5)]
    public class Entity
    {
        [PlcField(offset: 0, BitIndex = 0)]
        public ushort Id { get; set; }

        [PlcField(offset: 2, BitIndex = 0)]
        public bool Flag1 { get; set; }

        [PlcField(offset: 2, BitIndex = 1)]
        public bool Flag2 { get; set; }

    }
}
