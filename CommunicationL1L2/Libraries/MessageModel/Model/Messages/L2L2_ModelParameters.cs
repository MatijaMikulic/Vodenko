using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageModel.Model.Messages
{
    public class L2L2_ModelParameters:MessageBase
    {
        public List<double> Parameters { get; set; }

        public L2L2_ModelParameters(List<double> parameters, byte priority) : base(priority, MessageType.ModelParameters)
        {
            Parameters = parameters;
        }

    }
}
