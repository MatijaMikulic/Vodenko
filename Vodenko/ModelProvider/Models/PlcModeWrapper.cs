using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelProvider.Models
{
    public class PlcModeWrapper
    {
        private readonly object _lock = new object();
        private PlcMode _mode;

        public PlcModeWrapper()
        {
            _mode = new PlcMode();
        }

        public PlcMode Mode
        {
            get
            {
                lock (_lock)
                {
                    return _mode;
                }
            }
        }

        public void UpdateMode(PlcMode newMode)
        {
            lock (_lock)
            {
                _mode.ControlMode = newMode.ControlMode;
                _mode.Description = newMode.Description;
            }
        }
    }
}
