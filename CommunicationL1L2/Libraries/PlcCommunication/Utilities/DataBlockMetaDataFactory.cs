using PlcCommunication.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlcCommunication.Model;

namespace PlcCommunication.Utilities
{
    public static class DataBlockMetaDataFactory
    {
        public static List<DataBlockMetaData> CreateDataBlockMetaDataList()
        {
            return new List<DataBlockMetaData>
            {
                new DataBlockMetaData(DataBlockInfo.L1L2_DBIds.ProcessData, DataBlockInfo.BufferSizes.ProcessData),
                //new DataBlockMetaData(DataBlockInfo.L1L2_DBIds.Alarms, DataBlockInfo.BufferSizes.Alarms),
                new DataBlockMetaData(DataBlockInfo.L1L2_DBIds.ControllerParams, DataBlockInfo.BufferSizes.ControllerParams),
                new DataBlockMetaData(DataBlockInfo.L1L2_DBIds.SystemStatus, DataBlockInfo.BufferSizes.SystemStatus),
                new DataBlockMetaData(DataBlockInfo.L1L2_DBIds.ControlMode, DataBlockInfo.BufferSizes.ControlMode)
            };
        }
    }
}
