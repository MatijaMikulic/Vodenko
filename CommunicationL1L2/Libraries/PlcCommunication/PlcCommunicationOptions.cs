namespace PlcCommunication
{
    using PlcCommunication.Model;
    public class PlcCommunicationOptions
    {
        public IList<DataBlockConfig> DataBlocks { get; set; } = [];
        public TimeSpan HeartbeatInterval { get; set; } = TimeSpan.FromSeconds(2);
        public bool EnableHeartbeat { get; set; } = true;
    }
}
