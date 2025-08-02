//namespace V3.S7Plc.Communication.Builders
//{
//    using PlcCommunication.Entities;
//    using PlcCommunication.Model;

//    /// <summary>
//    /// Fluently build PlcCommunicationOptions.
//    /// </summary>
//    public class PlcCommunicationBuilder
//    {
//        private readonly PlcCommunicationOptions _options = new();

//        /// <summary>
//        /// Disable the built‑in heartbeat loop.
//        /// </summary>
//        public PlcCommunicationBuilder DisableHeartbeat()
//        {
//            _options.EnableHeartbeat = false;
//            return this;
//        }
//        /// <summary>
//        /// Add one DataBlock definition.
//        /// </summary>
//        public PlcCommunicationBuilder AddBlock(DataBlockConfig cfg)
//        {
//            _options.DataBlocks.Add(cfg);
//            return this;
//        }

//        /// <summary>
//        /// Return the built options.
//        /// </summary>
//        public PlcCommunicationOptions Build()
//         => new PlcCommunicationOptions
//         {
//             DataBlocks = _options.DataBlocks.ToList(),
//             HeartbeatInterval = _options.HeartbeatInterval,
//             EnableHeartbeat = _options.EnableHeartbeat
//         };
//    }
//}
