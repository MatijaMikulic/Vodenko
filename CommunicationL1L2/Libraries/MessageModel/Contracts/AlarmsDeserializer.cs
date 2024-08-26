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
    public class AlarmsDeserializer:IMessageDeserializer
    {
        public MessageBase Deserialize(byte[] body)
        {
            string bodyString = Encoding.UTF8.GetString(body);
            var jsonObject = JsonConvert.DeserializeObject<JObject>(bodyString);

            byte priority = jsonObject["Priority"].ToObject<byte>();

            var alarmDataObject = jsonObject["Alarm"] as JObject;
            if (alarmDataObject == null)
            {
                throw new InvalidDataException("Missing 'Alarm' object in JSON");
            }

            byte[] dateTime = alarmDataObject["DateTime"].ToObject<byte[]>();
            if (dateTime.Length != 12)
            {
                throw new InvalidDataException("DateTime byte array must be exactly 12 bytes long");
            }

            int alarmNo = alarmDataObject["AlarmNo"].ToObject<int>();


            L1L2_Alarms alarm = new L1L2_Alarms(dateTime, alarmNo);

            return new L2L2_Alarms(alarm, priority);
        }
    }
}
