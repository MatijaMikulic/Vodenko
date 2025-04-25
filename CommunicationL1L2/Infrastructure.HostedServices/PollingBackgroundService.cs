using Microsoft.Extensions.Hosting;

namespace Infrastructure.HostedServices
{
    public abstract class PollingBackgroundService : BackgroundService 
    {
        private readonly PeriodicTimer _timer;
        private readonly TimeSpan _interval;

        protected PollingBackgroundService(TimeSpan interval)
        {
            _interval = interval;
            _timer = new PeriodicTimer(_interval);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await OnStartedAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                while (await _timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false) && !cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await PollOnceAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // expected shutdown
            }
            finally
            {
                _timer.Dispose();
                await OnStoppedAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Called once, before the first poll tick.
        /// </summary>
        protected virtual Task OnStartedAsync(CancellationToken ct) => Task.CompletedTask;

        /// <summary>
        /// Called on every timer tick. Override to implement your 
        /// per‑interval work.
        /// </summary>
        protected abstract Task PollOnceAsync(CancellationToken ct);

        /// <summary>
        /// Called once, after the loop finishes.
        /// </summary>
        protected virtual Task OnStoppedAsync(CancellationToken ct) => Task.CompletedTask;

        protected async ValueTask DisposeAsync()
        {
            await OnStoppedAsync(CancellationToken.None).ConfigureAwait(false);
            _timer.Dispose();
        }

    }
}
