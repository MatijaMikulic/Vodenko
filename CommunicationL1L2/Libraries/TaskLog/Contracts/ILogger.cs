using MessageModel.Model.Messages;

namespace TaskLog.Contracts
{
    /// <summary>
    /// Interface for logging task messages.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs the specified LogMessage.
        /// </summary>
        /// <param name="logMessage">The LogMessage to be logged.</param>
        public void Log(L2L2_LogMessage logMessage);

        /// <summary>
        /// Asynchronously logs the specified LogMessage.
        /// </summary>
        /// <param name="logMessage">The LogMessage to be logged.</param>
        /// <returns>A task representing the asynchronous logging operation.</returns>
        public Task LogAsync(L2L2_LogMessage logMessage);
    }
}