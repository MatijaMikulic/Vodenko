using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } 
        public string password {  get; set; }
        public string group { get; set; }   
    }
}
