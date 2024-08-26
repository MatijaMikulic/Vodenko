using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageModel.Model.Messages;

namespace MessageModel.Contracts
{
    /// <summary>
    /// Factory class for creating message deserializers based on message types.
    /// </summary>
    public static class MessageDesirializerFactory
    {
        /// <summary>
        /// Gets a message deserializer based on the specified message type.
        /// </summary>
        /// <param name="messageType">The type of the message.</param>
        /// <returns>An instance of the IMessageDeserializer interface for the specified message type.</returns>
        public static IMessageDeserializer GetDeserializer(MessageType messageTyoe)
        {
            switch (messageTyoe)
            {
                case MessageType.LogMessage:
                    return new LogMessageDeserializer();
                case MessageType.RequestMessage:
                    return new RequestMessageDeserializer();
                case MessageType.SampleMessage:
                    return new SampleMessageDeserializer();
                case MessageType.DataBlockHeader:
                    return new DataBlockHeaderDeserializer();
                case MessageType.ProcessData:
                    return new ProcessDataDeserializer();
                case MessageType.Alarms:
                    return new AlarmsDeserializer();
                case MessageType.ControllerParams:
                    return new ControllerParamsDeserializer();
                case MessageType.SystemStatus: 
                    return new SystemStatusDeserializer();
                case MessageType.ControlMode:
                    return new ControlModeDeserializer();
                case MessageType.DynamicData:
                    return new DynamicDataDeserializer();
                case MessageType.ModelParameters:
                    return new ModelParametersDeserializer();
                case MessageType.PlcConnectionStatus:
                    return new PlcStatusConnectionMessageDeserializer();
                case MessageType.ServiceStatus:
                    return new ServiceStatusDeserializer();
                default:
                    throw new ArgumentException("Unsupported message type");
            }
        }
    }
}
