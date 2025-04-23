using Microsoft.Extensions.Options;
using PlcCommunication.Interfaces;
using PlcCommunication.Model;
using S7.Net;
namespace PlcCommunication
{
    /// <summary>
    /// Provides functionality related to creating a PLC instance and connection.
    /// </summary>
    public class PlcConnectionManager : IConnectionManager, IDisposable
    {
        private readonly Plc _plc;
        private volatile bool _lastKnownState;
        private bool _isInitialized;
        private readonly IList<DataBlockConfig> _blocks;
        public PlcConnectionManager(IOptions<PlcConfiguration> opts, IOptions<PlcCommunicationOptions> libOpts)
        {
            var c = opts.Value;
            _plc = new Plc(Enum.Parse<CpuType>(c.CpuType), c.IpAddress, c.Rack, c.Slot);
            _blocks = libOpts.Value.DataBlocks;
            _isInitialized = true;
        }
        /// <inheritdoc/>
        public void Open()
        {
            if (this._isInitialized)
            {
                _plc.Open();
                    
            }
        }

        ///<inheritdoc/>
        public void Close() => _plc.Close();

        ///<inheritdoc/>
        public bool IsReady => this.PlcInstance != null && this._isInitialized && this._plc.IsConnected;

        ///<inheritdoc/>
        public Plc PlcInstance => _plc;

        public event EventHandler<bool>? ConnectionStatusChanged;

        public void RefreshState()
        {
            var now = IsReady;
            if (now != _lastKnownState)
            {
                _lastKnownState = now;
                ConnectionStatusChanged?.Invoke(this, now);
            }
        }

        public bool Ping()
        {
            if (_blocks is null || !_blocks.Any())
                return false;

            var cfg = _blocks[0];   
            try
            {
                var raw = _plc.Read(
                    DataType.DataBlock,
                    cfg.Id,
                    cfg.ChangeCounterStart,
                    VarType.Word,
                    1
                );
                return raw is not null;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            _plc.Close();
            ConnectionStatusChanged = null;
        }
    }
}
