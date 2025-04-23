namespace MessageManagerService.Services
{
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// Wraps MMService so it can be managed by the Generic Host.
    /// </summary>
    public class MMHostedService : IHostedService
    {
        private readonly MessageManager _service;
        private CancellationTokenSource? _cts;
        private Task? _runTask;

        public MMHostedService(MessageManager service)
        {
            _service = service;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // 1) Link the host’s shutdown token
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // 2) Kick off the long‑running loop
            _runTask = _service.RunAsync(_cts.Token);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_cts is not null)
            {
                // 3) Signal cancellation to RunAsync
                _cts.Cancel();
            }

            if (_runTask is not null)
            {
                // 4) Wait for either RunAsync to finish or host‑timeout
                await Task.WhenAny(
                    _runTask,
                    Task.Delay(Timeout.Infinite, cancellationToken)
                ).ConfigureAwait(false);
            }

            // 5) Finally, do your cleanup
            _service.Stop();
        }
    }
}
