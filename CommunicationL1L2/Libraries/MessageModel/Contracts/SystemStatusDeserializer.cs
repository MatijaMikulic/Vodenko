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
    public class SystemStatusDeserializer:IMessageDeserializer
    {
        public MessageBase Deserialize(byte[] body)
        {
            string bodyString = Encoding.UTF8.GetString(body);
            var jsonObject = JsonConvert.DeserializeObject<JObject>(bodyString);

            byte priority = jsonObject["Priority"].ToObject<byte>();

            var processObject = jsonObject["SystemStatus"] as JObject;
            if (processObject == null)
            {
                throw new InvalidDataException("Missing 'SystemStatus' object in JSON");
            }

            bool b1 = processObject["IsProportionalValveActive"].ToObject<bool>();
            bool b2 = processObject["IsLowLevelSwitchActive"].ToObject<bool>();
            bool b3 = processObject["IsTank1CrtiticalLevel"].ToObject<bool>();
            bool b4 = processObject["IsTank2CrtiticalLevel"].ToObject<bool>();
            bool b5 = processObject["IsPumpActive"].ToObject<bool>();

            L1L2_SystemStatus sysStatus = new L1L2_SystemStatus(b1,b2,b3,b4,b5);

            return new L2L2_SystemStatus(sysStatus, priority);
        }
    }
}
