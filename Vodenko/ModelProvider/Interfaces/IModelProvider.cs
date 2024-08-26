using ModelProvider.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelProvider.Interfaces
{
    public interface IModelProvider
    {
        ControllerParametersWrapper ControllerParameters { get; }
        PlantStatusWrapper PlantStatus { get; }
        PlcModeWrapper PlcMode { get; }
        MathematicalModelWrapper MathematicalModel { get; }
        PlcConnectionStatusWrapper PlcConnectionStatus { get; }

        void UpdateControllerParameters(ControllerParameters newParameters);
        void UpdatePlantStatus(PlantStatus newStatus);
        void UpdatePlcMode(PlcMode newMode);
        void UpdateMathematicalModel(MathematicalModel newModel);
        void UpdatePlcConnectionStatus(PlcConnectionStatus newModel);

    }
}
