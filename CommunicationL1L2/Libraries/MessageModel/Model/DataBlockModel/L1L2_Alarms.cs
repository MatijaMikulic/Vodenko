using S7.Net.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageModel.Model.DataBlockModel
{
    public class L1L2_Alarms:PlcData
    {
        public int AlarmNo { get; set; }
        public byte[] DateTime { get; set; } = new byte[12];
        public L1L2_Alarms(byte[] dateTime, int alarmNo)
        {
            DateTime = dateTime;
            AlarmNo = alarmNo;
        }

        public System.DateTime GetDateTime()
        {
            // Extract year (little-endian)
            return DateTimeLong.FromByteArray(DateTime);
        }

        public L1L2_Alarms() { }
    }
}
