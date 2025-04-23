namespace PlcCommunication.Interfaces
{
    using S7.Net;
    /// <summary>
    /// Manages creation/open/close of the PLC connection.
    /// </summary>
    public interface IConnectionManager
    {
        /// <summary>
        /// Open the underlying PLC connection.
        /// </summary>
        void Open();

        /// <summary>
        /// Close the underlying PLC connection.
        /// </summary>
        void Close();

        /// <summary>
        /// True if Initialize() was called and PLC.IsConnected is true.
        /// </summary>
        bool IsReady { get; }

        /// <summary>
        /// The underlying S7.Net Plc instance for data‐access.
        /// </summary>
        Plc PlcInstance { get; }

        event EventHandler<bool> ConnectionStatusChanged;

        /// <summary>Re‑evaluate <see cref="IsReady"/> and raise <see cref="ConnectionStatusChanged"/> if it changed.</summary>
        void RefreshState();

        /// <summary>
        /// Probe the PLC link with a lightweight read; returns true if the PLC replied.
        /// </summary>
        bool Ping();
    }
}
