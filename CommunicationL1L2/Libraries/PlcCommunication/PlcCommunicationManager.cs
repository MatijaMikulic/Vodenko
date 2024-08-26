using S7.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlcCommunication
{
    /// <summary>
    /// Provides functionality related to creating a PLC instance and connection.
    /// </summary>
    public class PlcCommunicationManager
    {
        private readonly PlcConfiguration _plcConfiguration;
        private Plc? _plc;
        private bool _isInitialized;

        public PlcCommunicationManager(PlcConfiguration plcConfiguration)
        {
            _plcConfiguration = plcConfiguration;
            _isInitialized = false;
        }

        /// <summary>
        /// Initializes PLC object from configuration
        /// </summary>
        public void InitializePlc()
        {
            _plc = new Plc(
                Enum.Parse<CpuType>(_plcConfiguration.CpuType),
                _plcConfiguration.IpAddress,
                _plcConfiguration.Rack,
                _plcConfiguration.Slot
            );

            _isInitialized = true;
        }
        /// <summary>
        /// Opens communication with the PLC.
        /// </summary>
        public void OpenCommunication()
        {  
            _plc?.Open();
        } 
        /// <summary>
        /// Closes communication with the PLC.
        /// </summary>
        public void CloseCommunication()
        {
            _plc?.Close();
        }

        /// <summary>
        /// Checks whether communication with the PLC is ready.
        /// </summary>
        /// <returns>True if communication is ready; otherwise, false.</returns>
        public bool IsCommunicationReady()
        {
            return _isInitialized && _plc.IsConnected;
        }

        public Plc? Plc => _plc;

        public  PlcConfiguration PlcConfiguration => _plcConfiguration;
    }
}
