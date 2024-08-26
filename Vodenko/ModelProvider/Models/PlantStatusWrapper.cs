using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelProvider.Models
{
    public class PlantStatusWrapper
    {
        private readonly object _lock = new object();
        private PlantStatus _status;

        public PlantStatusWrapper()
        {
            _status = new PlantStatus();
        }

        public PlantStatus Status
        {
            get
            {
                lock (_lock)
                {
                    return _status;
                }
            }
        }

        public void UpdateStatus(PlantStatus newStatus)
        {
            lock (_lock)
            {
                _status.PumpActive = newStatus.PumpActive;
                _status.Tank1Level = newStatus.Tank1Level;
                _status.Tank2Level = newStatus.Tank2Level;
                _status.LevelSwitch = newStatus.LevelSwitch;
                _status.SystemReady = newStatus.SystemReady;
                _status.ValveOperatingRange = newStatus.ValveOperatingRange;
            }
        }
    }
}
