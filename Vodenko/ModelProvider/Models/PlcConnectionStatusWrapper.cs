using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelProvider.Models
{
    public class PlcConnectionStatusWrapper
    {
        private readonly object _lock = new object();
        private PlcConnectionStatus _status;

        public PlcConnectionStatusWrapper()
        {
            _status = new PlcConnectionStatus();
        }

        public PlcConnectionStatus Status
        {
            get
            {
                lock (_lock)
                {
                    return _status;
                }
            }
        }

        public void UpdateStatus(PlcConnectionStatus newStatus)
        {
            lock (_lock)
            {
                _status.IsConnected = newStatus.IsConnected;
            }
        }
    }
}
