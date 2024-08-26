using S7.Net.Types;
using S7.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlcCommunication.Constants
{
    /// <summary>
    /// Contains constants related to Data Blocks in the PLC communication.
    /// </summary>
    public static class DataBlockInfo
    {
        public const short DataOffset = 6;

        // Buffer Element Offsets
        public static class BufferOffsets
        {
            public const short ProcessData = 42;
            public const short Alarms = 16;
            public const short ControllerParams = 30;
            public const short SystemStatus = 4;
            public const short ControlMode = 2;
        }

        // Data Block IDs (L1L2)
        public static class L1L2_DBIds
        {
            public const ushort ProcessData = 250;
            public const ushort Alarms = 5; // Not Implemented on L1
            public const ushort ControllerParams = 260;
            public const ushort SystemStatus = 265;
            public const ushort ControlMode = 275;

            public static readonly Dictionary<int, int> ChangeCounterStartByte = new()
            {
                { ProcessData, 0 },
                //{ Alarms, 0 },
                { ControllerParams, 0 },
                { SystemStatus, 0 },
                { ControlMode, 0 }
            };

            public static readonly Dictionary<int, int> AuxCounterStartByte = new()
            {
                { ProcessData, 216 },
                //{ Alarms, 38 },
                { ControllerParams, 66 },
                { SystemStatus, 14 },
                { ControlMode, 10 }
            };

            public static readonly Dictionary<int, int> BufferPointerStartByte = new()
            {
                { ProcessData, 2 },
                //{ Alarms, 2 },
                { ControllerParams, 2 },
                { SystemStatus, 2 },
                { ControlMode, 2 }
            };
        }

        // Buffer Sizes
        public static class BufferSizes
        {
            public const short ProcessData = 5;
            public const short Alarms = 2;
            public const short ControllerParams = 2;
            public const short SystemStatus = 2;
            public const short ControlMode = 2;
        }

        // Data Block IDs (L2L1)
        public static class L2L1_DBIds
        {
            public const int SetpointDB = 255;
            public const int CntParamsDB = 280;
            public const int RequestCntDB = 270;
        }        
    }
}
