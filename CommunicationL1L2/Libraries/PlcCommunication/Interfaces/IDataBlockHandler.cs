using V3.S7Plc.Communication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V3.S7Plc.Communication.Interfaces
{
    /// <summary>
    /// Handler for reading from / writing to a specific data block.
    /// </summary>
    public interface IDataBlockHandler
    {
        Task<IList<T>> ReadAllAsync<T>(DataBlockFrameDefinition def) where T : class, new();
        Task WriteAsync<T>(DataBlockFrameDefinition def, T data) where T : class, new();
    }
}
