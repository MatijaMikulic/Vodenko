using Microsoft.AspNetCore.SignalR;
using VodenkoWeb.Hubs;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VodenkoWeb.Model;

namespace VodenkoWeb.Services
{
    public class LogMonitoringService : BackgroundService
    {
        private readonly IHubContext<LogHub> _hubContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LogMonitoringService> _logger;
        private readonly Dictionary<string, long> _filePositions;
        private readonly LogBuffer _logBuffer;

        public LogMonitoringService(IHubContext<LogHub> hubContext, IConfiguration configuration, ILogger<LogMonitoringService> logger, LogBuffer logBuffer)
        {
            _hubContext = hubContext;
            _configuration = configuration;
            _logger = logger;
            _filePositions = new Dictionary<string, long>();
            _logBuffer = logBuffer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var config = _configuration.GetSection("FileLogger");
                    var directoryPath = config["DirectoryPath"];

                    if (Directory.Exists(directoryPath))
                    {
                        var files = Directory.GetFiles(directoryPath, "*.txt");

                        foreach (var file in files)
                        {
                            if (!_filePositions.ContainsKey(file))
                            {
                                _filePositions[file] = 0;
                            }

                            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                stream.Seek(_filePositions[file], SeekOrigin.Begin);
                                using (var reader = new StreamReader(stream))
                                {
                                    string line;
                                    while ((line = reader.ReadLine()) != null)
                                    {
                                        if (line != "")
                                        {
                                            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                                            var logMessage = $"[{fileNameWithoutExtension}] - {line}";
                                            _logBuffer.Enqueue(logMessage);
                                            await _hubContext.Clients.All.SendAsync("ReceiveLogMessage", logMessage);
                                        }
                                        
                                    }

                                    _filePositions[file] = stream.Position;
                                }
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Log directory not found: {directoryPath}");
                    }

                    await Task.Delay(TimeSpan.FromMilliseconds(500), stoppingToken); // Delay between checks
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in LogMonitoringService");
                }
            }
        }
    }


}
