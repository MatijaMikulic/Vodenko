using ModelProvider.Interfaces;
using ModelProvider.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelProvider
{
    public class ModelProvider : IModelProvider
    {
        public ControllerParametersWrapper ControllerParameters { get; private set; }
        public PlantStatusWrapper PlantStatus { get; private set; }
        public PlcModeWrapper PlcMode { get; private set; }
        public MathematicalModelWrapper MathematicalModel { get; private set; }
        public PlcConnectionStatusWrapper PlcConnectionStatus { get; private set; }

        public ModelProvider()
        {
            ControllerParameters = new ControllerParametersWrapper();
            PlantStatus = new PlantStatusWrapper();
            PlcMode = new PlcModeWrapper();
            MathematicalModel = new MathematicalModelWrapper();
            PlcConnectionStatus = new PlcConnectionStatusWrapper();
        }

        public void UpdateControllerParameters(ControllerParameters newParameters)
        {
            ControllerParameters.UpdateParameters(newParameters);
        }

        public void UpdatePlantStatus(PlantStatus newStatus)
        {
            PlantStatus.UpdateStatus(newStatus);
        }

        public void UpdatePlcMode(PlcMode newMode)
        {
            PlcMode.UpdateMode(newMode);
        }

        public void UpdateMathematicalModel(MathematicalModel newModel)
        {
            MathematicalModel.UpdateModel(newModel);
        }

        public void UpdatePlcConnectionStatus(PlcConnectionStatus newModel)
        {
            PlcConnectionStatus.UpdateStatus(newModel);
        }

    }
}
