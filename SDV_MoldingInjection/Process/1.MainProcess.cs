using EQX.Core.Motion;
using SDV_MoldingInjection.Defines.Devices;

namespace SDV_MoldingInjection.Process
{
    public class MainProcess : MIProcess
    {
        #region Motions
        private IMotion XAxis => _devices.Motions.XAxis;
        private IMotion YAxis => _devices.Motions.StageYAxis;
        private IMotion Z1Axis => _devices.Motions.Z1Axis;
        private IMotion Z2Axis => _devices.Motions.Z2Axis;
        private IMotion Z3Axis => _devices.Motions.Z3Axis;
        private IMotion Z4Axis => _devices.Motions.Z4Axis;
        #endregion

        #region Constructors
        public MainProcess(Devices devices)
        {
            _devices = devices;
        }
        #endregion

        #region Privates
        private readonly Devices _devices;
        #endregion
    }
}
