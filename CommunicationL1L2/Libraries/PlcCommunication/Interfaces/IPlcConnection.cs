using V3.S7Plc.Communication.Model;

namespace V3.S7Plc.Communication.Interfaces
{
    /// <summary>Low-level abstraction over S7.NetPlus’s Plc connection.</summary>
    public interface IPlcConnection : IDisposable
    {
        bool IsConnected { get; }

        /// <summary>
        /// Open the underlying PLC connection.
        /// </summary>
        void Connect();

        /// <summary>
        /// Async version of connection open <see cref="Open"/>.
        /// </summary>
        /// <returns></returns>
        Task ConnectAsync();

        /// <summary>
        /// Close the underlying PLC connection.
        /// </summary>
        void Disconnect();

        ///<summary>
        ///Read raw bytes from a Data Block.
        ///</summary>
        Task<byte[]> ReadBytesAsync(int dbNumber, int startByte, int count);

        ///<summary>
        ///Write raw bytes into a Data Block.
        ///</summary>
        Task WriteBytesAsync(int dbNumber, int startByte, byte[] data);


        /// <summary>
        /// Reads a class instance of type T from the PLC using reflection
        /// or driver-specific class‐mapping.
        /// </summary>
        Task<(int bytesRead, T result)> ReadClassAsync<T>(
           int db, int offset, CancellationToken ct) where T : class, new();

        Task<object?> ReadAsync(string address);

        /// <summary>
        /// Ping the PLC to check if it is responsive.
        /// </summary>
        Task<bool> CheckStatusAsync();
    }
}
