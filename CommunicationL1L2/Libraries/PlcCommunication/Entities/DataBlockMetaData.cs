namespace V3.S7Plc.Communication.Model
{
    /// <summary>
    /// Class represeting Runtime snapshot of header/footer counters for one DB.
    /// </summary>
    public sealed class DataBlockMetaData
    {
        public ushort ChangeCounter { get; set; }
        public ushort BufferPointer { get; set; }
        public ushort AuxiliaryCounter { get; set; }
        public ushort DB { get; init; }

        /// <summary>Total elements in the circular buffer (static per DB).</summary>
        public short BufferSize { get; init; }
        public DataBlockMetaData(
            ushort changeCounter, 
            ushort auxiliaryCounter, 
            ushort bufferPointer, 
            ushort dB,
            short  bufferSize)
        {
            this.ChangeCounter = changeCounter;
            this.AuxiliaryCounter = auxiliaryCounter;
            this.BufferPointer = bufferPointer;
            this.DB = dB;
            this.BufferSize = bufferSize;
        }
    }
}
