using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Entities
{
    public class AuxTable
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Parameter {  get; set; }
        public int MinMinVaue {  get; set; }
        [Required]
        public int MinVaue { get; set; }
        public int MaxMaxValue { get; private set; }
        [Required]
        public int MaxValue { get; private set; }
        public string MeasurementUnit { get; set; }

    }
}
