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

        // mbar : 6.000
        // Torr : 6.125
        // Pascals : 4.000

        // 1 pa =  1/ strVacuumPressureUnit torr
        public double VacuumPressureInTorr
        {
            get
            {
                double volt = VacuumGauge.Volt;

                if (volt > 9.0) volt = 9.0;
                if (volt < 2.7) volt = 2.7;
                //if (volt < 2.7 || volt > 9.0)
                //    return -1;

                return Math.Pow(10, (volt - 6.125) / 1.0);
            }
        }
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
