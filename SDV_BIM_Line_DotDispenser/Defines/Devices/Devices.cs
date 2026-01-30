using EQX.Core.InOut;
using EQX.Core.Motion;
using EQX.Device.SpeedController;
using Microsoft.Extensions.DependencyInjection;
using SDV_BIM_Line_DotDispenser.Defines.Devices.Cylinder;
using SDV_BIM_Line_DotDispenser.Process;
using SDV_BIM_Line_DotDispenser.Recipe;
using System.Collections.ObjectModel;

namespace SDV_BIM_Line_DotDispenser.Defines.Devices
{
    public class Devices
    {
        public Devices(Inputs inputs,
            Outputs outputs,
            Motions motions,
            Cylinders cylinders,
            AnalogInputs analogInputs,
            MachineStatus machineStatus)
        {
            Inputs = inputs;
            Outputs = outputs;
            Motions = motions;
            Cylinders = cylinders;
            AnalogInputs = analogInputs;
        }

        public Inputs Inputs { get; }
        public Outputs Outputs { get; }
        public Motions Motions { get; }
        public Cylinders Cylinders { get; }
        public AnalogInputs AnalogInputs { get; }

        #region Public Methods
        #endregion
    }
}
