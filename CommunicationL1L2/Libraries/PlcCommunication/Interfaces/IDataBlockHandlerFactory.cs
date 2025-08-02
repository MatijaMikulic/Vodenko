using V3.S7Plc.Communication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V3.S7Plc.Communication.Interfaces
{
    /// <summary>
    /// Factory to get the appropriate data block handler (circular or single).
    /// </summary>
    public interface IDataBlockHandlerFactory
    {
        IDataBlockHandler GetHandler(DataBlockFrameDefinition definition);
    }
}
