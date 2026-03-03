using EQX.Device.Indicator;
using Microsoft.Extensions.DependencyInjection;
using SDV_MoldingInjection.Defines.Devices.Balance;
using SDV_MoldingInjection.Defines.Devices.Cylinder;

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
            MachineStatus machineStatus,
            [FromKeyedServices("PanelIndicator")] NEOSHSDIndicator panelIndicator,
            [FromKeyedServices("PumpIndicator")] NEOSHSDIndicator pumpIndicator)
        {
            Inputs = inputs;
            Outputs = outputs;
            Motions = motions;
            Cylinders = cylinders;
            AnalogInputs = analogInputs;
            Balances = balances;
            PanelIndicator = panelIndicator;
            PumpIndicator = pumpIndicator;
        }

        public Inputs Inputs { get; }
        public Outputs Outputs { get; }
        public Motions Motions { get; }
        public Cylinders Cylinders { get; }
        public AnalogInputs AnalogInputs { get; }
        public Balances Balances { get; }
        public NEOSHSDIndicator PanelIndicator { get; }
        public NEOSHSDIndicator PumpIndicator { get; }

        #region Public Methods
        #endregion
    }
}
