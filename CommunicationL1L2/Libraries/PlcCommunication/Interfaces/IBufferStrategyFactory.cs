using V3.S7Plc.Communication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V3.S7Plc.Communication.Interfaces
{
    /// <summary>
    /// Factory for creating buffer strategies based on DataBlockDefinition.
    /// </summary>

    public interface IBufferStrategyFactory
    {
        IBufferStrategy Create(DataBlockFrameDefinition definition);
    }
}
