using EQX.Device.Indicator;
using Microsoft.Extensions.DependencyInjection;

namespace SDV_MoldingInjection.Defines
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
            [FromKeyedServices("PanelIndicator")] NEOSHSDIndicator panelIndicator)
        {
            Inputs = inputs;
            Outputs = outputs;
            Motions = motions;
            Cylinders = cylinders;
            AnalogInputs = analogInputs;
            Balances = balances;
            PanelIndicator = panelIndicator;
        }

        public Inputs Inputs { get; }
        public Outputs Outputs { get; }
        public Motions Motions { get; }
        public Cylinders Cylinders { get; }
        public AnalogInputs AnalogInputs { get; }
        public Balances Balances { get; }
        public NEOSHSDIndicator PanelIndicator { get; }
        #region Public Methods
        #endregion
    }
}
