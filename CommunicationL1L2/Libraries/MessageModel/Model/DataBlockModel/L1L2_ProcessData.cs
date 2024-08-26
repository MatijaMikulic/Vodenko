using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using S7.Net;
using S7.Net.Types;


namespace MessageModel.Model.DataBlockModel
{
    [Serializable]
    public class L1L2_ProcessData:PlcData
    {

        public float ValvePositionFeedback { get; set; }
        public float InletFlow {  get; set; }
        public float WaterLevelTank1 {  get; set; }
        public float WaterLevelTank2 {  get; set; }
        public float OutletFlow { get; set; }

        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] DateTime { get; set; } = new byte[12];
        public float TargetWaterLevelTank2 { get; set; }
        public int Sample { get; set; }
        public bool IsPumpActive { get; set; }
        public L1L2_ProcessData( 
            float valvePositionFeedback, 
            float inletFlow, 
            float waterLevelTank1, 
            float waterLevelTank2, 
            float outletFlow, 
            byte[] dateTime,
            bool isPumpActive,
            int sample,
            float targetWaterLevelTank2
            )
        {
            ValvePositionFeedback = valvePositionFeedback;
            InletFlow = inletFlow;
            WaterLevelTank1 = waterLevelTank1;
            WaterLevelTank2 = waterLevelTank2;
            OutletFlow = outletFlow;
            DateTime = dateTime;
            IsPumpActive = isPumpActive;
            Sample = sample;
            TargetWaterLevelTank2 = targetWaterLevelTank2;
        }

        public L1L2_ProcessData() { }

        public System.DateTime GetDateTime()
        {
            return System.DateTime.Now;
            return DateTimeLong.FromByteArray(DateTime);
            
        }
    }
}
