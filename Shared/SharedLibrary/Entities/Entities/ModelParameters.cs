using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Entities
{
    public class ModelParameters
    {
        [Key]
        public int Id { get; set; }
        public double Theta1 { get; set; }
        public double Theta2 { get; set; }
        public double Theta3 { get; set; }
        public double Theta4 { get; set; }
        public double Theta5 { get; set; }
        public double Theta6 { get; set; }
        public double Theta7 { get; set; }
        public DateTime dateTime { get; set; } = DateTime.Now;
    }
}
