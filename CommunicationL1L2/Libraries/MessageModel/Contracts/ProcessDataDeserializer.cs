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
    public class ProcessDataDeserializer : IMessageDeserializer
    {
        public MessageBase Deserialize(byte[] body)
        {
            string bodyString = Encoding.UTF8.GetString(body);
            var jsonObject = JsonConvert.DeserializeObject<JObject>(bodyString);

            byte priority = jsonObject["Priority"].ToObject<byte>();

            var processDataObject = jsonObject["ProcessData"] as JObject;
            if (processDataObject == null)
            {
                throw new InvalidDataException("Missing 'ProcessData' object in JSON");
            }

            float valvePositionFeedback = processDataObject["ValvePositionFeedback"].ToObject<float>();
            float inletFlow = processDataObject["InletFlow"].ToObject<float>();
            float waterLevelTank1 = processDataObject["WaterLevelTank1"].ToObject<float>();
            float waterLevelTank2 = processDataObject["WaterLevelTank2"].ToObject<float>();
            float outletFlow = processDataObject["OutletFlow"].ToObject<float>();
            byte[] dateTime = processDataObject["DateTime"].ToObject<byte[]>();
            bool exsperimentFlag = processDataObject["IsPumpActive"].ToObject<bool>();
            int sample = processDataObject["Sample"].ToObject<int>();
            float targetWaterLevelTank2 = processDataObject["TargetWaterLevelTank2"].ToObject<float>();


            L1L2_ProcessData process = new L1L2_ProcessData(valvePositionFeedback,inletFlow,waterLevelTank1,waterLevelTank2,outletFlow,dateTime, exsperimentFlag, sample, targetWaterLevelTank2);

            return new L2L2_ProcessData(process, priority);
        }
    }
}
