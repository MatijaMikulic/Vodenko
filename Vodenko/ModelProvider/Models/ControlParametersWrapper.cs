using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelProvider.Models
{
    public class ControllerParametersWrapper
    {
        private readonly object _lock = new object();
        private ControllerParameters _parameters;

        public ControllerParametersWrapper()
        {
            _parameters = new ControllerParameters();
        }

        public ControllerParameters Parameters
        {
            get
            {
                lock (_lock)
                {
                    return _parameters;
                }
            }
        }

        public void UpdateParameters(ControllerParameters newParameters)
        {
            lock (_lock)
            {
                _parameters.Proportional = newParameters.Proportional;
                _parameters.Integral = newParameters.Integral;
                _parameters.Derivative = newParameters.Derivative;

                _parameters.Method = newParameters.Method;
                _parameters.K1 = newParameters.K1;
                _parameters.K2 = newParameters.K2;
                _parameters.K3 = newParameters.K3;
                _parameters.K4 = newParameters.K4;
            }
        }
    }
}
