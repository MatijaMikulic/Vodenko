using System.Globalization;
using System.Security.Cryptography;

namespace VodenkoWeb.Model
{
    public class DynamicDataBuffer: Queue<DataPoints>
    {
        public int? MaxCapacity { get; }
        public DynamicDataBuffer(int capacity) { MaxCapacity = capacity; }
        public int TotalItemsAddedCount { get; private set; }

        public void Add(DataPoints data)
        {
            if (Count == (MaxCapacity ?? -1)) Dequeue();
            Enqueue(data);
            TotalItemsAddedCount++;
        }
        public List<DataPoints> GetLatestData()
        {
            return this.ToList();
        }
    }

    public class DataPoints
    {
        public float ValvePositionFeedback { get; }
        public float InletFlow { get; }
        public float WaterLevelTank1 { get; }
        public float WaterLevelTank2 { get; }
        public float InletFlowNonLinModel { get; }
        public float WaterLevelTank1NonLinModel { get; }
        public float WaterLevelTank2NonLinModel { get; }
        public float InletFlowLinModel { get; }
        public float WaterLevelTank1LinModel { get; }
        public float WaterLevelTank2LinModel { get; }
        public float OutletFlow { get; }
        public DateTime DateTime { get; }
        public bool IsPumpActive { get; }
        public int Sample { get; }
        public float TargetWaterLevelTank2Model { get; set; }


        public DataPoints(float valvePositionFeedback, float inletFlow, float waterLevelTank1, float waterLevelTank2,
            float inletFlowModel, float waterLevelTank1Model, float waterLevelTank2Model, float outletFlow, DateTime dateTime,
            bool flag, int sample, float targetWaterLevelTank2Model)
        {
            ValvePositionFeedback = valvePositionFeedback;
            InletFlow = inletFlow;
            WaterLevelTank1 = waterLevelTank1;
            WaterLevelTank2 = waterLevelTank2;
            InletFlowNonLinModel = inletFlowModel;
            WaterLevelTank1NonLinModel = waterLevelTank1Model;
            WaterLevelTank2NonLinModel = waterLevelTank2Model;
            OutletFlow = outletFlow;
            DateTime = dateTime;
            IsPumpActive = flag;
            Sample = sample;
            TargetWaterLevelTank2Model = targetWaterLevelTank2Model;
        }

        public DataPoints(float valvePositionFeedback, float inletFlow, 
                          float waterLevelTank1, float waterLevelTank2, 
                          float inletFlowNonLinModel, float waterLevelTank1NonLinModel, 
                          float waterLevelTank2NonLinModel, float inletFlowLinModel, 
                          float waterLevelTank1LinModel, float waterLevelTank2LinModel, 
                          float outletFlow, DateTime dateTime, bool isPumpActive, int sample, float targetWaterLevelTank2Model)
        {
            ValvePositionFeedback = valvePositionFeedback;
            InletFlow = inletFlow;
            WaterLevelTank1 = waterLevelTank1;
            WaterLevelTank2 = waterLevelTank2;
            InletFlowNonLinModel = inletFlowNonLinModel;
            WaterLevelTank1NonLinModel = waterLevelTank1NonLinModel;
            WaterLevelTank2NonLinModel = waterLevelTank2NonLinModel;
            InletFlowLinModel = inletFlowLinModel;
            WaterLevelTank1LinModel = waterLevelTank1LinModel;
            WaterLevelTank2LinModel = waterLevelTank2LinModel;
            OutletFlow = outletFlow;
            DateTime = dateTime;
            IsPumpActive = isPumpActive;
            Sample = sample;
            TargetWaterLevelTank2Model = targetWaterLevelTank2Model;
        }
    }

    public static class DynamicDataBufferExtensions
    {
        public static DataPoints AddNewData(this DynamicDataBuffer buffer,DataPoints data)
        {
            buffer.Add(data);
            return data;
        }
    }
}
