using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageModel.Model.DataBlockModel
{
    public class L1L2_SystemStatus:PlcData
    {
        public bool IsProportionalValveActive {  get; set; }
        public bool IsLowLevelSwitchActive { get; set; }
        public bool IsTank1CrtiticalLevel { get; set; }
        public bool IsTank2CrtiticalLevel { get; set; }
        public bool IsPumpActive { get; set; }
        public ushort Spare { get; set; }


        public L1L2_SystemStatus() { }

        public L1L2_SystemStatus(bool isProportionalValveActive, bool isLowLevelSwitchActive, bool isTank1CrtiticalLevel, bool isTank2CrtiticalLevel, bool isPumpActive)
        {
            IsProportionalValveActive = isProportionalValveActive;
            IsLowLevelSwitchActive = isLowLevelSwitchActive;
            IsTank1CrtiticalLevel = isTank1CrtiticalLevel;
            IsTank2CrtiticalLevel = isTank2CrtiticalLevel;
            IsPumpActive = isPumpActive;
        }
    }
}
