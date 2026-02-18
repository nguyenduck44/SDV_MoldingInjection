using EQX.Core.InOut;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDV_MoldingInjection.Defines.Devices
{
    public class AnalogInputs
    {
        #region Properties
        public IAInput FanSpeed => _aInputDevice.AnalogInputs.First(a => a.Id == (int)EAnalogInput.FAN_SPEED);
        public IAInput VacuumGauge => _aInputDevice.AnalogInputs.First(a => a.Id == (int)EAnalogInput.VACUUM_GAUGE);

        // mbar : 6.143
        // Torr : 6.304
        // Pa = 3.572
        // kPa : 7.429
        public double VacuumPressureInTorr => Math.Pow(10, (VacuumGauge.Volt - 6.304) /1.286);
        #endregion

        public AnalogInputs([FromKeyedServices("AnalogInputDevice#1")] IAInputDevice aInputDevice)
        {
            _aInputDevice = aInputDevice;
            Initialize();
        }


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

        #region Privates
        private readonly IAInputDevice _aInputDevice;
        #endregion
    }
}
