namespace PlcCommunication.Interfaces
{
    using PlcCommunication.Model;

    /// <summary>
    /// Reads both content and metadata from the PLC data blocks.
    /// </summary>
    public interface IPlcDataAccess
    {
        /// <summary>
        /// Read a POCO TModel from DB dataBlockId at the given bufferPointer.
        /// </summary>
        TModel ReadContent<TModel>(ushort dataBlockId, ushort bufferPointer)
            where TModel : new();

        /// <summary>
        /// Dynamic read: instantiate the configured ModelType.
        /// </summary>
        object ReadContent(ushort dataBlockId, ushort bufferPointer);

        /// <summary>
        /// Read change/aux/pointer counters for all configured blocks.
        /// </summary>
        IReadOnlyList<DataBlockMetaData> ReadMetaData();

        /// <summary>
        /// Async version of ReadMetaData().
        /// </summary>
        Task<IReadOnlyList<DataBlockMetaData>> ReadMetaDataAsync();

        // ─── Write back a full POCO to its DB at ContentStart ───────────
        /// <summary>
        /// Write a model instance back to the PLC at the configured ContentStart offset.
        /// The <see cref="DataBlockConfig.ModelType"/> determines which DB to use.
        /// </summary>
        void WriteContent(object model);
    }
}
