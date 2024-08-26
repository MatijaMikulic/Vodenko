using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Entities
{
    public class MessageHeader
    {

        [Key]
        public int Code { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual ICollection<Message> Alarms { get; set; }
    }
}
