using MessageModel.Model.Messages;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskLog.Configurations;

namespace TaskLog.Contracts
{
    public class FileLogger : ILogger
    {
        private readonly string _directoryPath;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> fileSemaphores;

        public FileLogger(IOptions<FileLoggerConfiguration> options)
        {
            _directoryPath = options.Value.DirectoryPath;
            fileSemaphores = new ConcurrentDictionary<string, SemaphoreSlim>();


            if (!Directory.Exists(_directoryPath))
            {
                Directory.CreateDirectory(_directoryPath);
            }
        }

        public void Log(L2L2_LogMessage logMessage)
        {
            string path = GetLogFilePath(logMessage.TaskName);
            SemaphoreSlim semaphore = fileSemaphores.GetOrAdd(path, new SemaphoreSlim(1, 1));

            semaphore.Wait();
            LogToFile(path, logMessage.ToString());
            semaphore.Release();
        }
        public async Task LogAsync(L2L2_LogMessage logMessage)
        {
            string filePath = GetLogFilePath(logMessage.TaskName);
            await fileSemaphores.GetOrAdd(filePath, new SemaphoreSlim(1, 1)).WaitAsync();

            await LogToFileAsync(filePath, logMessage.ToString());

            fileSemaphores[filePath].Release();
        }

        private string GetLogFilePath(string taskName)
        {
            string safeTaskName = Path.GetInvalidFileNameChars().Aggregate(taskName, (current, c) => current.Replace(c.ToString(), string.Empty));
            return Path.Combine(_directoryPath, $"{safeTaskName}.txt");
        }

        private void LogToFile(string path, string message)
        {

            byte[] encodedText = Encoding.UTF8.GetBytes(message);

            using (FileStream sourceStream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None, bufferSize: 4096))
            {
                sourceStream.Write(encodedText, 0, encodedText.Length);
                sourceStream.Flush();
            }

        }
        private async Task LogToFileAsync(string path, string message)
        {
            byte[] encodedText = Encoding.UTF8.GetBytes(message);

            using (FileStream sourceStream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite, bufferSize: 4096, useAsync: true))
            {
                await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
                await sourceStream.FlushAsync();
            }
            // Update the last write time
            File.SetLastWriteTime(path, DateTime.Now);
        }
    }

}
