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
    public class RequestMessageDeserializer : IMessageDeserializer
    {
        public MessageBase Deserialize(byte[] body)
        {
            string bodyString = Encoding.UTF8.GetString(body);
            var jsonObject = JsonConvert.DeserializeObject<JObject>(bodyString);

            byte priority = jsonObject["Priority"].ToObject<byte>();

            var requestObject = jsonObject["Request"] as JObject;
            if (requestObject == null)
            {
                throw new InvalidDataException("Missing 'Request' object in JSON");
            }

            short row = requestObject["Row"].ToObject<short>();
            short col = requestObject["Col"].ToObject<short>();
            L1L2_Request request = new L1L2_Request(row, col);

            return new L2L2_RequestMessage(request, priority);
        }
    }
}
