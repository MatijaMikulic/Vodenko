using MessageModel.Model.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageModel.Contracts
{
    /// <summary>
    /// Implementation of IMessageDeserializer for deserializing LogMessage objects.
    /// </summary>
    public class LogMessageDeserializer : IMessageDeserializer
    {
        public MessageBase Deserialize(byte[] body)
        {
            string bodyString = Encoding.UTF8.GetString(body);
            var jsonObject = JsonConvert.DeserializeObject<JObject>(bodyString);
            
            var taskName = jsonObject["TaskName"].ToObject<string>();
            var messageText = jsonObject["Message"].ToObject<string>();
            var timestamp = jsonObject["TimeStamp"].ToObject<DateTime>();
            var level = jsonObject["Level"].ToObject<Severity>();
            var priority = jsonObject["Priority"].ToObject<byte>();

            return new L2L2_LogMessage(taskName, messageText,level,priority,timestamp);
        }
        
    }
}
