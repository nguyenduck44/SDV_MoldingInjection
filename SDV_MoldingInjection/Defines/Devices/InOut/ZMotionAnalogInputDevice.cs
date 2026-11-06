using cszmcaux;
using EQX.InOut.InOut.Analog;
using EQX.Motion.ByVendor.ZMotion;

namespace SDV_MoldingInjection.Defines
{
    public class ZMotionAnalogInputDevice<TEnum> : AnalogInputDeviceBase<TEnum> where TEnum : Enum
    {
        #region Properties
        public MotionMasterZMotion MotionMaster { get; init; } = null!;

        public override bool IsConnected { get => MotionMaster?.IsConnected == true; }
        #endregion

        #region Public methods
        public override double GetVolt(int channel)
        {
            return ReadAD(channel);
        }

        public override double GetCurrent(int channel)
        {
            return ReadAD(channel);
        }
        #endregion

        #region Private methods
        private double ReadAD(int channel)
        {
            if (IsConnected == false) return 0;

            float fValue = 0;
            zmcaux.ZAux_Direct_GetAD(MotionMaster.CardID, channel, ref fValue);

            return fValue;
        }
        #endregion
    }
}
