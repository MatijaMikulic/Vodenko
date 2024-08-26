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
    public class ControlModeDeserializer:IMessageDeserializer
    {
        public MessageBase Deserialize(byte[] body)
        {
            string bodyString = Encoding.UTF8.GetString(body);
            var jsonObject = JsonConvert.DeserializeObject<JObject>(bodyString);

            byte priority = jsonObject["Priority"].ToObject<byte>();

            var alarmDataObject = jsonObject["ControlMode"] as JObject;
            if (alarmDataObject == null)
            {
                throw new InvalidDataException("Missing 'ControlMode' object in JSON");
            }

            ushort cntMode = alarmDataObject["ControlMode"].ToObject<ushort>();


            L1L2_ControlMode alarm = new L1L2_ControlMode(cntMode);

            return new L2L2_ControlMode(alarm, priority);
        }
    }
}
