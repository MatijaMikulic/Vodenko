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
    public class ModelParametersDeserializer : IMessageDeserializer
    {
        public MessageBase Deserialize(byte[] body)
        {
            string bodyString = Encoding.UTF8.GetString(body);
            var jsonObject = JsonConvert.DeserializeObject<JObject>(bodyString);

            // Extract Parameters and Priority
            var parametersArray = jsonObject["Parameters"].ToObject<double[]>();
            var priority = jsonObject["Priority"].ToObject<byte>();

            var parametersList = new List<double>(parametersArray);

            return new L2L2_ModelParameters(parametersList, priority);
        }
    }
}
