using PlcCommunication.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlcCommunication.Model
{
    public class DataBlockMetaData
    {
        public ushort ChangeCounter { get; set; }
        public ushort BufferPointer { get; set; }
        public ushort AuxiliaryCounter { get; set; }
        public ushort DB { get; private set; }
        public short BufferSize { get; private set; }   


        public DataBlockMetaData()
        {
            ChangeCounter = 0;
            AuxiliaryCounter = 0;
            BufferPointer = 0;
            DB = 0;
        }
        public DataBlockMetaData(ushort changeCounter, ushort auxiliaryCounter, ushort bufferPointer, ushort dB)
        {
            ChangeCounter = changeCounter;
            AuxiliaryCounter = auxiliaryCounter;
            BufferPointer = bufferPointer;
            DB = dB;
            if(dB == DataBlockInfo.L1L2_DBIds.ProcessData) 
            {
                BufferSize = 5;
            }
            else
            {
                BufferSize = 2;
            }
        }

        public DataBlockMetaData(ushort db, short bufferSize)
        {
            ChangeCounter = 0;
            AuxiliaryCounter = 0;
            BufferPointer = 1;

            DB = db;
            BufferSize = bufferSize;
        }

        public int FindBufferPointer()
        {
            return (AuxiliaryCounter % BufferSize)+1;
        }
    }
}
