namespace MessageModel.Model.Messages
{

    public enum MessageType
    {
        LogMessage,
        DataBlockHeader,
        RequestMessage,
        SampleMessage,
        ProcessData,
        Alarms,
        ControllerParams,
        SystemStatus,
        ControlMode,
        DynamicData,
        ModelParameters,
        PlcConnectionStatus,
        ServiceStatus
    }
    public class MessageBase
    {
        public byte Priority { get; set; }
        public MessageType MessageType { get; set; }

        public MessageBase(byte priority, MessageType messageType)
        {
            Priority = priority;
            MessageType = messageType;
        }
    }
}