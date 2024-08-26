using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageModel.Model.Messages;

namespace MessageModel.Contracts
{
    /// <summary>
    /// Implementation of IMessageDeserializer for deserializing DataBlockHedaer objects.
    /// </summary>
    public class DataBlockHeaderDeserializer : IMessageDeserializer
    {
        public MessageBase Deserialize(byte[] body)
        {
            string bodyString = Encoding.UTF8.GetString(body);
            var jsonObject = JsonConvert.DeserializeObject<JObject>(bodyString);

            var db = jsonObject["DB"].ToObject<ushort>();
            var bufferPointer = jsonObject["BufferPointer"].ToObject<ushort>();
            var priority = jsonObject["Priority"].ToObject<byte>();

            return new L2L2_DataBlockHeader(db,bufferPointer, priority);
        }
    }
}
