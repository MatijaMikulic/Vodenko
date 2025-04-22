using Microsoft.Extensions.Hosting;

namespace DataMonitoringService.Services
{
    public sealed class DMHostedService: IHostedService
    {
        private readonly DataMonitoring _service;
        private CancellationTokenSource? _cts;
        private Task? _runTask;

        public DMHostedService(DataMonitoring service)
        {
            _service = service;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _runTask = _service.RunAsync(_cts.Token);
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cts?.Cancel();
            if (_runTask != null)
                await Task.WhenAny(_runTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }
    }
}
