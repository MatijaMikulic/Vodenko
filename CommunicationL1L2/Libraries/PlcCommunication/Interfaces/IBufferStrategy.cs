using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V3.S7Plc.Communication.Interfaces
{
    /// <summary>
    /// Decides how to buffer read items (single latest vs. full FIFO, etc.).
    /// </summary>
    public interface IBufferStrategy
    {
        /// <summary>
        /// Add a newly read item into the buffer.
        /// </summary>
        void Add(object item);

        /// <summary>
        /// Fetch & clear all buffered items.
        /// </summary>
        IReadOnlyList<object> GetAll();
    }
}
