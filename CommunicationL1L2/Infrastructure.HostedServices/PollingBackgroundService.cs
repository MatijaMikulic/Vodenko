using Microsoft.Extensions.Hosting;

namespace Infrastructure.HostedServices
{
    public abstract class PollingBackgroundService : BackgroundService 
    {
        private readonly TimeSpan _interval;

        protected PollingBackgroundService(TimeSpan interval) => _interval = interval;

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await OnStartedAsync(cancellationToken).ConfigureAwait(false);

            var timer = new PeriodicTimer(_interval);
            try
            {
                while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
                {
                    try
                    {
                        await PollOnceAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch(Exception ex)
                    {

                    }
                }
            }
            catch (OperationCanceledException)
            {
                timer.Dispose();
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

    }
}
