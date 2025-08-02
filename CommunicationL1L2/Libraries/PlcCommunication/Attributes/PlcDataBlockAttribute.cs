using V3.S7Plc.Communication.Enums;

namespace V3.S7Plc.Communication.Attributes
{
    /// <summary>
    /// Marks a class as representing a PLC Data Block (DB) with a given number.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PlcDataBlockAttribute : Attribute
    {
        /// <summary>PLC Data Block number (DB number).</summary>
        public int DbNumber { get; }

        /// <summary>Type of buffer to be used./// </summary>
        public StructureType Type { get; set; } = StructureType.DataBuffer;

        /// <summary>Buffer capacity (number of entries) for circular blocks. Default=1.</summary>
        public int Capacity { get; set; } = 1;
     

        public PlcDataBlockAttribute(int dbNumber, StructureType type)
        {
            DbNumber = dbNumber;
            Type = type;
        }

        public PlcDataBlockAttribute(int dbNumber, StructureType type, int capacity)
        {
            DbNumber = dbNumber;
            Type = type;
            Capacity = capacity;
        }
    }
}
