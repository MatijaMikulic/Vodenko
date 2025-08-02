using V3.S7Plc.Communication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V3.S7Plc.Communication.Interfaces
{
    /// <summary>
    /// High-level PLC client interface for connecting, reading, and writing typed messages.
    /// </summary>
    public interface IPlcClient : IDisposable
    {
        Task ConnectAsync();
        void Disconnect();
        Task<T?> ReadLatestAsync<T>() where T : class, new();
        Task<IList<T>> ReadAllAvailableAsync<T>() where T : class, new();
        Task<bool> WriteAsync<T>(T data) where T : class, new();
        public IPlcClient AddCircularBuffer<T>(int db, int capacity) where T : class, new();
        public IPlcClient AddDataBuffer<T>(int db) where T : class, new();
        bool IsHealthy { get; }

        event EventHandler<HealthChangedEventArgs>? HealthChanged;
    }
    public class HealthChangedEventArgs : EventArgs
    {
        public bool IsHealthy { get; }
        public HealthChangedEventArgs(bool healthy) => IsHealthy = healthy;
    }

}
