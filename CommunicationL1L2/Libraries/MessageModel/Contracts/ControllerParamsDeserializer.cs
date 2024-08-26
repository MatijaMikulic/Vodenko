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
    public class ControllerParamsDeserializer:IMessageDeserializer
    {
        public MessageBase Deserialize(byte[] body)
        {
            string bodyString = Encoding.UTF8.GetString(body);
            var jsonObject = JsonConvert.DeserializeObject<JObject>(bodyString);

            byte priority = jsonObject["Priority"].ToObject<byte>();

            var controllerParamsJsObject = jsonObject["ControllerParams"] as JObject;
            if (controllerParamsJsObject == null)
            {
                throw new InvalidDataException("Missing 'ControllerParams' object in JSON");
            }

            ushort method = controllerParamsJsObject["Method"].ToObject<ushort>();

            float proportional = controllerParamsJsObject["Proportional"].ToObject<float>();
            float integral = controllerParamsJsObject["Integral"].ToObject<float>();
            float derivative = controllerParamsJsObject["Derivative"].ToObject<float>();

            float K1 = controllerParamsJsObject["K1"].ToObject<float>();
            float K2 = controllerParamsJsObject["K2"].ToObject<float>();
            float K3 = controllerParamsJsObject["K3"].ToObject<float>();
            float K4 = controllerParamsJsObject["K4"].ToObject<float>();


            L1L2_ControllerParams cntparams = new L1L2_ControllerParams(proportional,integral,derivative,method,K1,K2,K3,K4);

            return new L2L2_ControllerParams(cntparams, priority);
        }
    }
}
