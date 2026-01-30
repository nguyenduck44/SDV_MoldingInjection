using EQX.Core.InOut;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDV_DotDispenser.Defines.Devices
{
    public class AnalogInputs
    {
        private readonly IAInputDevice _aInputDevice;

        public AnalogInputs([FromKeyedServices("AnalogInputDevice#1")] IAInputDevice aInputDevice)
        {
            _aInputDevice = aInputDevice;
            Initialize();
        }

        public IAInput Laser => _aInputDevice.AnalogInputs.First(a => a.Id == (int)EAnalogInput.LASER);
        public IAInput PlasmaVoltage => _aInputDevice.AnalogInputs.First(a => a.Id == (int)EAnalogInput.PLASMA_VOLTAGE);
        public IAInput PlasmaPower => _aInputDevice.AnalogInputs.First(a => a.Id == (int)EAnalogInput.PLASMA_POWER);
        public IAInput PlasmaN2FlowRate => _aInputDevice.AnalogInputs.First(a => a.Id == (int)EAnalogInput.PLASMA_N2_FLOW_RATE);
        public IAInput PlasmaCDAFlowRate => _aInputDevice.AnalogInputs.First(a => a.Id == (int)EAnalogInput.PLASMA_CDA_FLOW_RATE);
        public IAInput PlasmaTemperature => _aInputDevice.AnalogInputs.First(a => a.Id == (int)EAnalogInput.PLASMA_TEMPERATURE);

        public bool Initialize()
        {
            return _aInputDevice.Initialize();
        }

        public bool Connect()
        {
            return _aInputDevice.Connect();
        }

        public bool Disconnect()
        {
            return _aInputDevice.Disconnect();
        }
    }
}
