using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageModel.Model.Messages
{
    public class L2L2_DynamicData : MessageBase
    {
        public L2L2_DynamicData(float valvePositionFeedback, float inletFlow, float waterLevelTank1, float waterLevelTank2,
                                float inletFlowNLModel, float waterLevelTank1NLModel, float waterLevelTank2NLModel,
                                float inletFlowLinModel, float waterLevelTank1LinModel, float waterLevelTank2LinModel,
                                float outletFlow,
                                DateTime dateTime, bool isPumpActive, int sample, float targetWaterLevelTank2Model, byte priority)
        : base(priority, MessageType.DynamicData)
        {
            ValvePositionFeedback = valvePositionFeedback;
            InletFlow = inletFlow;
            WaterLevelTank1 = waterLevelTank1;
            WaterLevelTank2 = waterLevelTank2;
            InletFlowNonLinModel = inletFlowNLModel;
            WaterLevelTank1NonLinModel = waterLevelTank1NLModel;
            WaterLevelTank2NonLinModel = waterLevelTank2NLModel;
            WaterLevelTank1LinModel = waterLevelTank1LinModel;
            WaterLevelTank2LinModel = waterLevelTank2LinModel;
            InletFlowLinModel = inletFlowLinModel;
            OutletFlow = outletFlow;
            DateTime = dateTime;
            IsPumpActive = isPumpActive;
            Sample = sample;
            TargetWaterLevelTank2Model = targetWaterLevelTank2Model;
        }

        public float ValvePositionFeedback { get; set; }
        public float InletFlow { get; set; }
        public float WaterLevelTank1 { get; set; }
        public float WaterLevelTank2 { get; set; }
        public float InletFlowNonLinModel { get; set; }
        public float WaterLevelTank1NonLinModel { get; set; }
        public float WaterLevelTank2NonLinModel { get; set; }
        public float InletFlowLinModel { get; set; }
        public float WaterLevelTank1LinModel { get; set; }
        public float WaterLevelTank2LinModel { get; set; }
        public float OutletFlow { get; set; }
        public DateTime DateTime { get; set; }
        public bool IsPumpActive { get; set; }
        public int Sample { get; set; }
        public float TargetWaterLevelTank2Model { get; set; }
    }
}
