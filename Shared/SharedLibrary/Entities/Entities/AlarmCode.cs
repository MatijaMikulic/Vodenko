using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Entities
{
    public class AlarmCode
    {
        [Key]
        public int Code { get; set; }

        [Required]
        public string Description { get; set; }

        public string Severity { get; set; }

        public virtual ICollection<Alarm> Alarms { get; set; }  // Virtual
    }
}
