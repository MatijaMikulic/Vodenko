using Microsoft.Extensions.Hosting;
using PlcCommunication.Interfaces;

namespace PlcCommunication
{
    ///<summary>
    /// Periodically checks the PLC connection and re‑opens if needed.
    /// Fires ConnectionStatusChanged via IConnectionManager.RefreshState().
    ///</summary>
    public class Heartbeat : IHeartbeat, IDisposable
    {
        private readonly IConnectionManager _conn;
        private readonly Timer _timer;
        private readonly TimeSpan _interval;

        public Heartbeat(IConnectionManager c, TimeSpan interval)
        {
            _conn = c;
            _interval = interval;
            _timer = new Timer(_ => Tick(), null, Timeout.Infinite, Timeout.Infinite);
        }
        public void Start() => _timer.Change(_interval, _interval);
        public void Stop() => _timer.Change(Timeout.Infinite, Timeout.Infinite);

        private void Tick()
        {
            if (!_conn.Ping())
                _conn.Open();
            _conn.RefreshState();
        }
        public void Dispose() => _timer.Dispose();
    }

    public class HeartbeatHostedService : IHostedService
    {
        private readonly IHeartbeat _hb;
        public HeartbeatHostedService(IHeartbeat hb) => _hb = hb;
        public Task StartAsync(CancellationToken _) { _hb.Start(); return Task.CompletedTask; }
        public Task StopAsync(CancellationToken _) { _hb.Stop(); return Task.CompletedTask; }
    }
}
