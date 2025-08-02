using V3.S7Plc.Communication.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V3.S7Plc.Communication.Templates.Interfaces
{
    public interface IBufferTemplate
    {
        StructureType Type { get; }

        BufferTemplate Build();
    }
}
