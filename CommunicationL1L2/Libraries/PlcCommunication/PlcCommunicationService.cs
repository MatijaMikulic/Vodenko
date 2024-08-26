using Microsoft.Extensions.Options;
using S7.Net;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PlcCommunication
{
    /// <summary>
    /// Provides functionality related to communication with a PLC (Programmable Logic Controller).
    /// </summary>
    public class PlcCommunicationService:IDisposable,INotifyPropertyChanged
    {
        private readonly PlcCommunicationManager _connectionManager;
        private readonly PlcDataAccess _dataAccess;
        private bool _isConnected;
        private readonly Heartbeat _heartbeat;
        private bool _hasHeartbeatStarted;
        private PlcException? _lastException;

        /// <summary>
        /// Initializes a new instance of the PlcCommunicationService class.
        /// </summary>
        /// <param name="options">The configuration options for the PLC.</param>
        public PlcCommunicationService(IOptions<PlcConfiguration> options)
        {
            var plcConfiguration = options.Value;
            _connectionManager = new PlcCommunicationManager(plcConfiguration);
            _connectionManager.InitializePlc();
            _dataAccess = new PlcDataAccess(_connectionManager.Plc);
            _isConnected = true;
            _hasHeartbeatStarted = false;

            _heartbeat = new Heartbeat();
        }

        /// <summary>
        /// Indicates whether the communicator is currently connected to the PLC.
        /// This property is automatically set by Heartbeat.
        /// </summary>
        public bool IsConnected
        {
            get { return _isConnected; }
            set 
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Starts heartbeat to periodically check for plc connectivity.
        /// Heartbeat performs automatic reconnection.
        /// </summary>
        public void Start()
        {
            if (!_hasHeartbeatStarted)
            {
                _hasHeartbeatStarted = true;
                _heartbeat.Start(IsConnectionActive,AttemptReconnection);
            }
        }

        /// <summary>
        /// Reopens communication with the PLC.
        /// </summary>
        public void AttemptReconnection()
        {
            try
            {
                _connectionManager.OpenCommunication();
            }
            catch (PlcException e)
            {
                _lastException = e;
            }
        }

        /// <summary>
        /// Checks whether the communicator is connected to the PLC.
        /// </summary>
        /// <returns>True if connected; otherwise, false.</returns>
        public bool IsConnectionActive()
        {
            bool status = PingConnection();
            // Attempting to read data will automatically set plc.IsConnected to true or false
            this.IsConnected = _connectionManager.IsCommunicationReady();
            return IsConnected;
        }

        /// <summary>
        /// Pings connection by attempting to read some data
        /// </summary>
        /// <returns>True if data was read successfully; otherwise, false.</returns>
        private bool PingConnection()
        {
            try
            {
                var data = DataAccess.Read(DataType.DataBlock, 4, 0, VarType.Int, 1);
                return data is not null;
            }
            catch (PlcException e)
            {
                //_lastException = e;
                return false;
            }
        }

        /// <summary>
        /// Gets the PlcDataAccess component for accessing PLC data.
        /// </summary>
        public PlcDataAccess DataAccess => _dataAccess;

        /// <summary>
        /// Gets the last thrown Plc exception.
        /// </summary>
        public PlcException? LastException => _lastException;

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public PlcConfiguration PlcConfiguration => _connectionManager.PlcConfiguration;

        public void Dispose()
        {
            _heartbeat.Stop();
            _connectionManager.CloseCommunication();
        }

    }
}