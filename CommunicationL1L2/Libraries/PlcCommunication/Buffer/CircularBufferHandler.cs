using V3.S7Plc.Communication.Interfaces;
using V3.S7Plc.Communication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V3.S7Plc.Communication.Buffer
{
    /// <summary>
    /// Handler for circular-buffer data blocks. Reads multiple new entries based on counters.
    /// </summary>
    public class CircularBufferHandler : IDataBlockHandler
    {
        private readonly DataBlockFrameDefinition _def;
        private readonly IBufferStrategy _buffer;
        private readonly IPlcConnection _conn;
        private readonly IDataBlockMapper _mapper;
        private ushort _lastAux = 0;

        public CircularBufferHandler(
            DataBlockFrameDefinition def,
            IBufferStrategy buffer,
            IPlcConnection conn,
            IDataBlockMapper mapper)
        {
            _def = def;
            _buffer = buffer;
            _conn = conn;
            _mapper = mapper;
        }


        // To be implemented:
        public async Task<IList<T>> ReadAllAsync<T>(DataBlockFrameDefinition d) where T : class, new()
        {
            var headerBytes = await _conn.ReadBytesAsync(d.DbNumber, d.Header.StartOffset, 4);
            ushort change = (ushort)((headerBytes[0] << 8) | headerBytes[1]);
            ushort pointer = (ushort)((headerBytes[2] << 8) | headerBytes[3]);

            var auxBytes = await _conn.ReadBytesAsync(d.DbNumber, d.Footer.StartOffset, 2);
            ushort auxCounter = (ushort)((auxBytes[0] << 8) | auxBytes[1]);

            //Calculate how many new entries to read
            

            // Read those entries

            
            return _buffer.GetAll() as IList<T> ?? new List<T>();
        }

        // To be done
        public async Task WriteAsync<T>(DataBlockFrameDefinition d, T data) where T : class, new()
        {
            
        }
    }
}
