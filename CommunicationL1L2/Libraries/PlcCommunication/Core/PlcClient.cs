
using V3.S7Plc.Communication.Interfaces;

namespace V3.S7Plc.Communication.Core
{
    /// <summary>
    /// High-level PLC client: connects, reads (single or circular), writes,
    /// and fires health-change events. Uses attribute-driven mapping
    /// and buffer strategies injected via DI.
    /// </summary>
    public class PlcClient : IPlcClient
    {
        private readonly IPlcConnection _conn;
        private readonly IDataBlockMapper _mapper;
        private readonly IDataBlockHandlerFactory _handlerFactory;
        private bool _isHealthy;
        public bool IsHealthy => _isHealthy;
        public event EventHandler<HealthChangedEventArgs>? HealthChanged;

        public PlcClient(
            IPlcConnection conn,
            IDataBlockMapper mapper,
            IDataBlockHandlerFactory handlerFactory)
        {
            _conn = conn;
            _mapper = mapper;
            _handlerFactory = handlerFactory;
        }

        public IPlcClient AddCircularBuffer<T>(int db, int capacity) where T : class, new()
        {
            var def = _mapper.GetDefinition<T>();
            var handler = _handlerFactory.GetHandler(def);
            return this;
        }

        public IPlcClient AddDataBuffer<T>(int db) where T : class, new()
        {
            var def = _mapper.GetDefinition<T>();
            var handler = _handlerFactory.GetHandler(def);
            return this;
        }

        public async Task ConnectAsync()
        {
            await _conn.ConnectAsync();
            bool status = await _conn.CheckStatusAsync();
            SetHealth(status);
        }
        public void Disconnect()
        {
            _conn.Disconnect();
            SetHealth(false);
        }

        public Task<T?> ReadLatestAsync<T>() where T : class, new()
            => ReadAllAvailableAsync<T>().ContinueWith(t => t.Result.Count > 0 ? t.Result[^1] : null);

        public async Task<IList<T>> ReadAllAvailableAsync<T>() where T : class, new()
        {
            var def = _mapper.GetDefinition<T>();
            var handler = _handlerFactory.GetHandler(def);
            try
            {
                var list = await handler.ReadAllAsync<T>(def);
                SetHealth(true);
                return list;
            }
            catch
            {
                SetHealth(false);
                return new List<T>();
            }
        }

        public async Task<bool> WriteAsync<T>(T data) where T : class, new()
        {
            var def = _mapper.GetDefinition<T>();
            var handler = _handlerFactory.GetHandler(def);
            try
            {
                await handler.WriteAsync(def, data);
                SetHealth(true);
                return true;
            }
            catch
            {
                SetHealth(false);
                return false;
            }
        }

        private void SetHealth(bool healthy)
        {
            if (_isHealthy == healthy) return;
            _isHealthy = healthy;
            HealthChanged?.Invoke(this, new HealthChangedEventArgs(healthy));
        }

        private async Task<TResult> SafeExecuteAsync<TResult>(Func<Task<TResult>> op)
        {
            if (!_conn.IsConnected)
            {
                SetHealth(false);
                return default!;
            }

            try
            {
                var res = await op().ConfigureAwait(false);
                SetHealth(true);
                return res;
            }
            catch
            {
                SetHealth(false);
                return default!;
            }
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
