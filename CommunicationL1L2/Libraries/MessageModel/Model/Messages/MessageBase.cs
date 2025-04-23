namespace MessageModel.Model.Messages
{

    public enum MessageType
    {
        [System.ComponentModel.Description("log.message")]
        LogMessage,

        [System.ComponentModel.Description("data.block.header")]
        DataBlockHeader,

        [System.ComponentModel.Description("request.message")]
        RequestMessage,

        [System.ComponentModel.Description("sample.message")]
        SampleMessage,
        
        [System.ComponentModel.Description("process.data")]
        ProcessData,

        [System.ComponentModel.Description("alarms")]
        Alarms,

        [System.ComponentModel.Description("alarms")]
        ControllerParams,

        [System.ComponentModel.Description("system.status")]
        SystemStatus,

        [System.ComponentModel.Description("control.mode")]
        ControlMode,

        [System.ComponentModel.Description("dynamic.data")]
        DynamicData,

        [System.ComponentModel.Description("model.params")]
        ModelParameters,

        [System.ComponentModel.Description("plc.connection.status")]
        PlcConnectionStatus,

        [System.ComponentModel.Description("service.status")]
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