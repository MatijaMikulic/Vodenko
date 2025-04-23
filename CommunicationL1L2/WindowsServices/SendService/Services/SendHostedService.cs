using Microsoft.Extensions.Hosting;
namespace SendManagerService.Services
{
    /// <summary>
    /// HostedService wrapper for SendManager
    /// </summary>
    public class SendHostedService : IHostedService
    {
        private readonly SendManager _manager;

        public SendHostedService(SendManager manager)
        {
            _manager = manager;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = _manager.RunAsync(cancellationToken);
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _manager.Stop();
            await Task.CompletedTask;
        }
    }
}
