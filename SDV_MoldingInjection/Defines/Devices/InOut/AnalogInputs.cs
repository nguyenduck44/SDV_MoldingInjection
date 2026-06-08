using EQX.Core.Helpers;
using EQX.Core.InOut;
using Microsoft.Extensions.DependencyInjection;

namespace SDV_MoldingInjection.Defines
{
    public class AnalogInputs
    {
        #region Properties
        public IAInput FanSpeed => _aInputDevice.AnalogInputs.First(a => a.Id == (int)EAnalogInput.FAN_SPEED);
        public IAInput VacuumGauge => _aInputDevice.AnalogInputs.First(a => a.Id == (int)EAnalogInput.VACUUM_GAUGE);
        public IAInput MainAir => _aInputDevice.AnalogInputs.First(a => a.Id == (int)EAnalogInput.MAIN_AIR_CDA);
        public IAInput ChamberPressure => _aInputDevice.AnalogInputs.First(a => a.Id == (int)EAnalogInput.CHAMBER_PRESSURE_CDA);
        public IAInput Pump => _aInputDevice.AnalogInputs.First(a => a.Id == (int)EAnalogInput.PUMP_CDA);
        public IAInput PumpVent => _aInputDevice.AnalogInputs.First(a => a.Id == (int)EAnalogInput.PUMP_VENT_CDA);

        public double PumpCDA => AnalogConvertHelper.Convert(Pump.Volt, 0.0, 5.0, 0, 5);
        public double MainAirCDA => AnalogConvertHelper.Convert(MainAir.Volt, 0.0, 5.0, 0, 5);
        public double PumpVentCDA => AnalogConvertHelper.Convert(PumpVent.Volt, 0.0, 5.0, 0, 5);
        public double ChamberPressureCDA => AnalogConvertHelper.Convert(ChamberPressure.Volt, 0.0, 5.0, 0, 5);

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

        #region Methods
        
        #endregion
        #region Privates
        private readonly IAInputDevice _aInputDevice;
        #endregion
    }
}
