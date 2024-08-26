using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Entities
{
    public class Alarm
    {
        [Key]
        public int Id { get; set; }

        public DateTime DateTime { get; set; }

        public int AlarmCodeId { get; set; }

        [ForeignKey("AlarmCodeId")]
        public virtual AlarmCode AlarmCode { get; set; }
        public string Comment { get; set; }
    }
}
