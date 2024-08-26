using MessageModel.Model.DataBlockModel;
using MessageModel.Model.Messages;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageModel.Contracts
{
    public class PlcStatusConnectionMessageDeserializer : IMessageDeserializer
    {
        public MessageBase Deserialize(byte[] body)
        {
            string bodyString = Encoding.UTF8.GetString(body);
            var jsonObject = JsonConvert.DeserializeObject<JObject>(bodyString);

            var isConnected = jsonObject["IsConnected"].ToObject<bool>();
            var priority = jsonObject["Priority"].ToObject<byte>();

            return new L2L2_PlcConnectionStatus(isConnected, priority);
        }
    }
}
