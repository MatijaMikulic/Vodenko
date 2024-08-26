using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlcCommunication
{
    public class PlcConfiguration
    {
        public string CpuType { get; set; } 
        public string IpAddress {  get; set; }
        public short Rack { get; set; }
        public short Slot { get; set; }
    }
}
