using V3.S7Plc.Communication.Entities;
using V3.S7Plc.Communication.Interfaces;
using S7.Net;
using V3.S7Plc.Communication.Model;

namespace V3.S7Plc.Communication.Core
{
    public class S7PlcConnection : IPlcConnection
    {
        private readonly Plc _plc;
        public bool IsConnected => _plc.IsConnected;

        public S7PlcConnection(PlcConnectionOptions options)
        {
            if (!Enum.TryParse<CpuType>(options.CpuType, ignoreCase: true, out var cpu))
                throw new ArgumentException($"Invalid CPU type '{options.CpuType}' in configuration.");

            _plc = new Plc(cpu, options.IpAddress, options.Rack, options.Slot);

        }

        public async Task<bool> CheckStatusAsync()
        {
            try
            {
                byte status = await _plc.ReadStatusAsync().ConfigureAwait(false);
                return (status & 0x08) == 0x08;
            }
            catch
            {
                return false;
            }
        }

        public void Connect()
        {
            if (!IsConnected)
            {
                _plc.Open();
            }
        }

        public async Task ConnectAsync()
        {
            if(!IsConnected)
            {
                await _plc.OpenAsync().ConfigureAwait(false);
            }
        }

        public void Disconnect()
        {
            if (_plc.IsConnected)
                _plc.Close();
        }

        public void Dispose()
        {
            Disconnect();
        }

        public async Task<byte[]> ReadBytesAsync(int dbNumber, int startByte, int count)
        {
            //return await _plc.ReadBytesAsync(DataType.DataBlock, dbNumber, startByte, count).ConfigureAwait(false);

            const int MaxBytesPerRead = 200;
            List<byte> result = new List<byte>(count);
            int bytesRemaining = count;
            int offset = startByte;
            while (bytesRemaining > 0)
            {
                int toRead = bytesRemaining > MaxBytesPerRead ? MaxBytesPerRead : bytesRemaining;
                byte[] chunk = (byte[])await (_plc.ReadBytesAsync(DataType.DataBlock, dbNumber, offset, toRead).ConfigureAwait(false));

                result.AddRange(chunk);
                bytesRemaining -= toRead;
                offset += toRead;
            }
            return result.ToArray();
        }

        public async Task WriteBytesAsync(int dbNumber, int startByte, byte[] data)
        {
            await _plc.WriteBytesAsync(DataType.DataBlock, dbNumber, startByte, data).ConfigureAwait(false);
        }

        public async Task<(int bytesRead, T result)> ReadClassAsync<T>(int db, int offset, CancellationToken ct) where T : class, new()
        {
            var instance = new T();

            var tuple = await _plc.ReadClassAsync(
                sourceClass: instance,
                db: db,
                startByteAdr: offset,
                cancellationToken: ct
            ).ConfigureAwait(false);

            int bytesRead = tuple.Item1;
            object rawObj = tuple.Item2;

            return (bytesRead, (T)rawObj);
        }

        public Task<object?> ReadAsync(string address)
        {
            throw new NotImplementedException();
        }

        // perhaps I could just provide other logic like readAsync, writeAsync, readClassAsync etc...
    }
}
