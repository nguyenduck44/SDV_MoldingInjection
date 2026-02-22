using EQX.Core.InOut;
using EQX.Core.Motion;
using EQX.Device.SpeedController;
using Microsoft.Extensions.DependencyInjection;
using SDV_MoldingInjection.Defines.Devices.Balance;
using SDV_MoldingInjection.Defines.Devices.Cylinder;
using SDV_MoldingInjection.Recipe;
using System.Collections.ObjectModel;

namespace SDV_MoldingInjection.Defines.Devices
{
    public class Devices
    {
        public Devices(Inputs inputs,
            Outputs outputs,
            Motions motions,
            Cylinders cylinders,
            AnalogInputs analogInputs,
            Balances balances,
            MachineStatus machineStatus)
        {
            Inputs = inputs;
            Outputs = outputs;
            Motions = motions;
            Cylinders = cylinders;
            AnalogInputs = analogInputs;
            Balances = balances;
        }

        public Inputs Inputs { get; }
        public Outputs Outputs { get; }
        public Motions Motions { get; }
        public Cylinders Cylinders { get; }
        public AnalogInputs AnalogInputs { get; }
        public Balances Balances { get; }

        #region Public Methods
        #endregion
    }
}
