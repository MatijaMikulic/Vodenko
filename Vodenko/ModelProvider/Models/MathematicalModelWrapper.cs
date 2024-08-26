using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelProvider.Models
{
    public class MathematicalModelWrapper
    {
        private readonly object _lock = new object();
        private MathematicalModel _model;

        public MathematicalModelWrapper()
        {
            _model = new MathematicalModel();
        }

        public MathematicalModel Model
        {
            get
            {
                lock (_lock)
                {
                    return _model;
                }
            }
        }

        public void UpdateModel(MathematicalModel newModel)
        {
            lock (_lock)
            {
                _model.A = newModel.A;
                _model.B = newModel.B;
            }
        }
    }
}
