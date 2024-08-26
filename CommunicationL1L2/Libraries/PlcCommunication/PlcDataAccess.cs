using MessageModel.Model.DataBlockModel;
using PlcCommunication.Constants;
using PlcCommunication.Model;
using S7.Net;
using S7.Net.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PlcCommunication
{
    /// <summary>
    /// Provides access and operations for reading and writing data to PLC Data blocks.
    /// </summary>
    public class PlcDataAccess
    {
        private readonly Plc _plc;

        private readonly List<DataItem> _changeCounters;
        private readonly List<DataItem> _auxCounters;
        private readonly List<DataItem> _bufferPointers;

        /// <summary>
        /// Initializes a new instance of the PlcDataAccess class.
        /// </summary>
        /// <param name="plc">The PLC object used for communication.</param>
        public PlcDataAccess(Plc plc)
        {
            _plc = plc;
            _changeCounters = InitializeDataItems(DataBlockInfo.L1L2_DBIds.ChangeCounterStartByte);
            _auxCounters    = InitializeDataItems(DataBlockInfo.L1L2_DBIds.AuxCounterStartByte);
            _bufferPointers = InitializeDataItems(DataBlockInfo.L1L2_DBIds.BufferPointerStartByte);
        }
        private List<DataItem> InitializeDataItems(Dictionary<int, int> startBytes)
        {
            return startBytes.Select(pair => new DataItem
            {
                Count = 1,
                DataType = DataType.DataBlock,
                DB = pair.Key,
                StartByteAdr = pair.Value,
                VarType = VarType.Word
            }).ToList();
        }

        /// <summary>
        /// Reads data from the PLC based on specified parameters.
        /// </summary>
        /// <param name="dataType">The data type to be read.</param>
        /// <param name="db">The data block number.</param>
        /// <param name="startByte">The starting byte address.</param>
        /// <param name="type">The variable type.</param>
        /// <param name="varCount">The number of variables to read.</param>
        /// <returns>The read data from the PLC.</returns>
        public object? Read(DataType dataType, int db, int startByte, VarType type, int varCount)
        {
            return _plc.Read(dataType, db, startByte, type, varCount);
        }
        /// <summary>
        /// Reads and processes multiple data items from the PLC to create a list of DataBlockMetaData.
        /// </summary>
        /// <returns>A list of DataBlockMetaData objects containing processed PLC data.</returns>
        public List<DataBlockMetaData> ReadDBMetaData()
        {
            List<DataBlockMetaData> result = new List<DataBlockMetaData>();

            _plc.ReadMultipleVars(_changeCounters);
            _plc.ReadMultipleVars(_bufferPointers);
            _plc.ReadMultipleVars(_auxCounters);

            result = _changeCounters.Zip(_auxCounters, _bufferPointers)
                       .Select(tuple => new DataBlockMetaData(
                           (ushort)tuple.First.Value,
                           (ushort)tuple.Second.Value,
                           (ushort)tuple.Third.Value,
                           (ushort)tuple.Item1.DB
                       ))
                       .ToList();

            return result;
        }

        /// <summary>
        /// Reads and processes multiple data items asynchronously from the PLC to create a list of DataBlockMetaData.
        /// </summary>
        /// <returns>A list of DataBlockMetaData objects containing processed PLC data.</returns>
        public async Task<List<DataBlockMetaData>> ReadDBMetaDataAsync()
        {
            List<DataBlockMetaData> result = new List<DataBlockMetaData>();

            Task<List<DataItem>> changeCountersTask = _plc.ReadMultipleVarsAsync(_changeCounters);
            Task<List<DataItem>> bufferPointersTask = _plc.ReadMultipleVarsAsync(_bufferPointers);
            Task<List<DataItem>> auxCountersTask    = _plc.ReadMultipleVarsAsync(_auxCounters);

            await Task.WhenAll(changeCountersTask, bufferPointersTask, auxCountersTask);

            List<DataItem> changeCounters = changeCountersTask.Result;
            List<DataItem> bufferPointers = bufferPointersTask.Result;
            List<DataItem> auxCounters    = auxCountersTask.Result;

            var res = changeCounters
                .Zip(auxCounters, (changeCounter, auxCounter) => new { changeCounter, auxCounter })
                .Zip(bufferPointers, (combined, bufferPointer) => new DataBlockMetaData(
                    (ushort)combined.changeCounter.Value,
                    (ushort)combined.auxCounter.Value,
                    (ushort)bufferPointer.Value,
                    (ushort)combined.changeCounter.DB))
                .ToList();

            return res;
        }

        /// <summary>
        /// Reads buffer element from Data block
        /// </summary>
        /// <returns>A PlcData object containing buffer element.</returns>
        public PlcData ReadDBContent(ushort dataBlock, ushort bufferPointer)
        {
            int offset;
            switch (dataBlock)
            {

                case DataBlockInfo.L1L2_DBIds.ProcessData:
                    PlcData process_Data = new L1L2_ProcessData();
                    offset = GetOffset(dataBlock, bufferPointer);
                    _plc.ReadClass(process_Data, dataBlock, offset);
                    return process_Data;
                case DataBlockInfo.L1L2_DBIds.Alarms:
                    PlcData alarms = new L1L2_Alarms();
                    offset = GetOffset(dataBlock, bufferPointer);
                    _plc.ReadClass(alarms, dataBlock, offset);
                    return alarms;
                case DataBlockInfo.L1L2_DBIds.ControllerParams:
                    PlcData cntparams = new L1L2_ControllerParams();
                    offset = GetOffset(dataBlock, bufferPointer);
                    _plc.ReadClass(cntparams, dataBlock, offset);
                    return cntparams;
                case DataBlockInfo.L1L2_DBIds.SystemStatus:
                    PlcData status = new L1L2_SystemStatus();
                    offset = GetOffset(dataBlock, bufferPointer);
                    _plc.ReadClass(status, dataBlock, offset);
                    return status;
                case DataBlockInfo.L1L2_DBIds.ControlMode:
                    PlcData cntMode = new L1L2_ControlMode();
                    offset = GetOffset(dataBlock, bufferPointer);
                    _plc.ReadClass(cntMode, dataBlock, offset);
                    return cntMode;
                default:
                    throw new ArgumentException("Unknown data block type.");
            }          
        }


        /// <summary>
        /// Calculates offset for element inside buffer
        /// </summary>
        /// <returns>An offset.</returns>
        private int GetOffset(ushort dataBlock, ushort bufferPointer)
        {
            switch (dataBlock)
            {
                case DataBlockInfo.L1L2_DBIds.ProcessData:
                    return DataBlockInfo.DataOffset + DataBlockInfo.BufferOffsets.ProcessData * (bufferPointer - 1); // -1 in case bufferPointer starts at 1.
                case DataBlockInfo.L1L2_DBIds.Alarms:
                    return DataBlockInfo.DataOffset + DataBlockInfo.BufferOffsets.Alarms * (bufferPointer - 1); // -1 in case bufferPointer starts at 1.
                case DataBlockInfo.L1L2_DBIds.ControllerParams:
                    return DataBlockInfo.DataOffset + DataBlockInfo.BufferOffsets.ControllerParams * (bufferPointer - 1); // -1 in case bufferPointer starts at 1.
                case DataBlockInfo.L1L2_DBIds.SystemStatus:
                    return DataBlockInfo.DataOffset + DataBlockInfo.BufferOffsets.SystemStatus * (bufferPointer - 1); // -1 in case bufferPointer starts at 1.
                case DataBlockInfo.L1L2_DBIds.ControlMode:
                    return DataBlockInfo.DataOffset + DataBlockInfo.BufferOffsets.ControlMode * (bufferPointer - 1); // -1 in case bufferPointer starts at 1.
                default:
                    throw new ArgumentException("Unknown data block type.");
            }
        }

        /// <summary>
        /// Gets the default value for a specified variable type.
        /// </summary>
        public object GetDefaultValueForType(VarType type)
        {
            switch (type)
            {
                case VarType.Bit:
                    return false;
                case VarType.Byte:
                case VarType.Word:
                case VarType.DInt:
                    return 0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), $"Unsupported variable type: {type}");
            }
        }

        public ushort ReadChangeCounter(int DB)
        {
            ushort result = (ushort)_plc.Read(DataType.DataBlock, DB, 0, VarType.Word,1);
            return result;
        }

        public ushort ReadAuxiliaryCounter(int DB)
        {
            ushort result = 0;
            switch (DB)
            {
                case DataBlockInfo.L2L1_DBIds.SetpointDB:
                    result = (ushort)_plc.Read(DataType.DataBlock, DB, 16, VarType.Word, 1); break;

                case DataBlockInfo.L2L1_DBIds.CntParamsDB:
                    result = (ushort)_plc.Read(DataType.DataBlock, DB, 32, VarType.Word, 1); break;

                case DataBlockInfo.L2L1_DBIds.RequestCntDB:
                    result = (ushort)_plc.Read(DataType.DataBlock, DB, 4, VarType.Word, 1); break;
            }
            return result;
        }

       public void UpdateChangeCounter(int DB,ushort result)
        {
            switch (DB)
            {
                case DataBlockInfo.L2L1_DBIds.SetpointDB:
                    _plc.Write("DB255.DBD0", result); break;

                case DataBlockInfo.L2L1_DBIds.CntParamsDB:
                    _plc.Write("DB280.DBD0", result); break;

                case DataBlockInfo.L2L1_DBIds.RequestCntDB:
                    _plc.Write("DB270.DBD0", result); break;
            }
        }

        public void UpdateAuxCounter(int DB, ushort result)
        {
            switch (DB)
            {
                case DataBlockInfo.L2L1_DBIds.SetpointDB:
                    _plc.Write("DB255.DBD16", result); break;

                case DataBlockInfo.L2L1_DBIds.CntParamsDB:
                    _plc.Write("DB280.DBD32", result); break;

                case DataBlockInfo.L2L1_DBIds.RequestCntDB:
                    _plc.Write("DB270.DBD4", result); break;
            }
        }

        public void WriteToDB(PlcData data)
        {
            if(data is L2L1_SetPoint setPoint)
            {
                _plc.WriteClass(setPoint, 255, 2);
            }
            else if(data is L2L1_RequestControl requestControl)
            {
                _plc.WriteClass(requestControl, 270, 2);
            }
            else if(data is L2L1_ControllerParameters controllerParameters)
            {
                _plc.WriteClass(controllerParameters, 280, 2);
            }
        }

        public void TestReading()
        {
            TestData somedata = new TestData();
            _plc.ReadClass(somedata, 23, 14);
            Console.WriteLine(somedata.ToString());

        }
    }
    public class TestData
    {
        //public System.DateTime _dateTtimeLong { get; set; }
        //public byte[] DateTimeLong { get; set; } = new byte[12];
        //public bool bool1 { get; set; }
        //public bool bool2 { get; set; }
        public short integer1 { get; set; }

        public bool bool3 { get; set; }
        public bool bool4 { get; set; }

        public short integer2 { get; set; }


        public System.DateTime GetDateTime()
        {
            // Extract year (little-endian)
            //byte month = DateTimeLong[2];
            //byte day = DateTimeLong[3];
            //byte hour = DateTimeLong[5]; // Note: Skipping weekday at index 4
            //byte minute = DateTimeLong[6];
            //byte second = DateTimeLong[7];
            //uint nanosecond = BitConverter.ToUInt32(DateTimeLong, 8);

            // Convert nanoseconds to milliseconds (integer division by 1,000,000)
            //int milliseconds = (int)(nanosecond / 1000000);


            //return new System.DateTime(2024, 6, 11, hour, minute, second).AddMilliseconds(milliseconds);
            return System.DateTime.Now;
        }

        public override string ToString()
        {
            return $"Integer1: {integer1}, Bool3: {bool3}, Bool4: {bool4}, Integer2: {integer2}";
        }
    }
}
