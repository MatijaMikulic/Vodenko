using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Entities
{
    public class DynamicData
    {
        [Key]
        public int Id { get; set; }
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
        public float Target { get; set; }
    }
}
