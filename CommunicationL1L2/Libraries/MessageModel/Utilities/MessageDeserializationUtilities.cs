using MessageModel.Contracts;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageModel.Model.Messages;

namespace MessageModel.Utilities
{
    /// <summary>
    /// Utility class for deserializing messages using the appropriate deserializer.
    /// </summary>
    public static class MessageDeserializationUtilities
    {
        /// <summary>
        /// Deserializes the provided byte array into a message object based on its type.
        /// </summary>
        /// <param name="body">The byte array representing the serialized message.</param>
        /// <returns>An instance of the MessageBase class representing the deserialized message.</returns>
        public static MessageBase DeserializeMessage(byte[] body)
        {
            var message = Encoding.UTF8.GetString(body);
            var jsonObject = JsonConvert.DeserializeObject<JObject>(message);
            var messageTypeString = jsonObject["MessageType"].ToObject<string>();
            var messageType = Enum.Parse<MessageType>(messageTypeString);


            var deserializer = MessageDesirializerFactory.GetDeserializer(messageType);
            return deserializer.Deserialize(body);
        }
    }
}
