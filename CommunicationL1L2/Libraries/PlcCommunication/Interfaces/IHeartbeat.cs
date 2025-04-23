namespace PlcCommunication.Interfaces
{
    /// <summary>
    /// Periodically checks & reconnects the PLC.
    /// </summary>
    public interface IHeartbeat
    {
        /// <summary>
        /// Start the recurring health‐check.
        /// </summary>
        void Start();

        /// <summary>
        /// Stop the health‐check.
        /// </summary>
        void Stop();
    }
}
