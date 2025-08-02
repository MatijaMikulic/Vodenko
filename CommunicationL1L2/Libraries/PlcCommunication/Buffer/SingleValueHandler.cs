using V3.S7Plc.Communication.Interfaces;
using V3.S7Plc.Communication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace V3.S7Plc.Communication.Buffer
{
    /// <summary>
    /// Handler for single-value data blocks. Reads/writes one element with change/aux counters.
    /// </summary>
    public class SingleValueHandler : IDataBlockHandler
    {
        private readonly DataBlockFrameDefinition _def;
        private readonly IBufferStrategy _buffer;
        private readonly IPlcConnection _conn;
        private readonly IDataBlockMapper _mapper;
        private ushort _lastChange = 0, _lastAux = 0;

        public SingleValueHandler(
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

        public async Task<IList<T>> ReadAllAsync<T>(DataBlockFrameDefinition d) where T : class, new()
        {
            // Read change counter (2 bytes) and aux counter (2 bytes)
            var chBytes = await _conn.ReadBytesAsync(d.DbNumber, d.Header.StartOffset, 2).ConfigureAwait(false);
            ushort change = (ushort)((chBytes[0] << 8) | chBytes[1]);

            var auxBytes = await _conn.ReadBytesAsync(d.DbNumber, d.Footer.StartOffset, 2).ConfigureAwait(false);
            ushort aux = (ushort)((auxBytes[0] << 8) | auxBytes[1]);

            var results = new List<T>();

            if (change == aux || change != _lastChange)
            {
                (var numOfBytes, var item) = await _conn.ReadClassAsync<T>(d.DbNumber, d.Body.StartOffset, CancellationToken.None).ConfigureAwait(false);

                //var item = _mapper.MapBytesToModel<T>(raw);
                //await _conn.ReadBytesAsync(d.DbNumber, d.Body.StartOffset, item).ConfigureAwait(false);

                _buffer.Add(item!);
            }

            _lastChange = change;
            _lastAux = aux;

            return _buffer.GetAll() as IList<T> ?? new List<T>();
        }

        public async Task WriteAsync<T>(DataBlockFrameDefinition d, T data) where T : class, new()
        {
            // Increment change counter
            var chBytes = await _conn.ReadBytesAsync(d.DbNumber, d.Header.StartOffset, 2);
            ushort ch = (ushort)(((chBytes[0] << 8) | chBytes[1]) + 1);
            await _conn.WriteBytesAsync(d.DbNumber, d.Header.StartOffset, new byte[] { (byte)(ch >> 8), (byte)ch }).ConfigureAwait(false);

            // Write payload
            var raw = _mapper.MapModelToBytes(data);
            await _conn.WriteBytesAsync(d.DbNumber, d.Body.StartOffset, raw).ConfigureAwait(false);
            //or writeClassAsync

            // Increment aux counter
            await _conn.WriteBytesAsync(d.DbNumber, d.Footer.StartOffset, new byte[] { (byte)(ch >> 8), (byte)ch }).ConfigureAwait(false);

        }
    }
}
