using MessageModel.Model.Messages;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessageModel.Contracts
{
    public class DynamicDataDeserializer : IMessageDeserializer
    {
        public MessageBase Deserialize(byte[] body)
        {
            string jsonString = Encoding.UTF8.GetString(body);
            var jsonObject = JsonConvert.DeserializeObject<JObject>(jsonString);

            // Extract the fields from the JSON object
            float valvePositionFeedback = jsonObject["ValvePositionFeedback"].ToObject<float>();
            float inletFlow = jsonObject["InletFlow"].ToObject<float>();
            float waterLevelTank1 = jsonObject["WaterLevelTank1"].ToObject<float>();
            float waterLevelTank2 = jsonObject["WaterLevelTank2"].ToObject<float>();
            float inletFlowNonLinModel = jsonObject["InletFlowNonLinModel"].ToObject<float>();
            float waterLevelTank1NonLinModel = jsonObject["WaterLevelTank1NonLinModel"].ToObject<float>();
            float waterLevelTank2NonLinModel = jsonObject["WaterLevelTank2NonLinModel"].ToObject<float>();

            float inletFlowLinModel = jsonObject["InletFlowLinModel"].ToObject<float>();
            float waterLevelTank1LinModel = jsonObject["WaterLevelTank1LinModel"].ToObject<float>();
            float waterLevelTank2LinModel = jsonObject["WaterLevelTank2LinModel"].ToObject<float>();

            float outletFlow = jsonObject["OutletFlow"].ToObject<float>();
            DateTime dateTime = jsonObject["DateTime"].ToObject<DateTime>();
            bool flag = jsonObject["IsPumpActive"].ToObject<bool>();
            int sample = jsonObject["Sample"].ToObject<int>();
            float targetWaterLevelTank2Model = jsonObject["TargetWaterLevelTank2Model"].ToObject<float>();
            byte priority = jsonObject["Priority"].ToObject<byte>();

            // Create an instance of L2L2_DynamicData
            L2L2_DynamicData dynamicData = new L2L2_DynamicData(
                valvePositionFeedback, inletFlow, waterLevelTank1, waterLevelTank2,
                inletFlowNonLinModel, waterLevelTank1NonLinModel, waterLevelTank2NonLinModel, 
                inletFlowLinModel, waterLevelTank1LinModel, waterLevelTank2LinModel,
                outletFlow,
                dateTime, flag, sample, targetWaterLevelTank2Model, priority);

            return dynamicData;
        }
    }
}
