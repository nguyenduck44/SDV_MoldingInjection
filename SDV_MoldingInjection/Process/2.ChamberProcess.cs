using EQX.Core.InOut;
using EQX.Core.Motion;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.Devices;

namespace SDV_MoldingInjection.Process
{
    public class ChamberProcess : MIProcess
    {
        #region Motions
        private IMotion YAxis => _devices.Motions.StageYAxis;
        #endregion

        #region Cylinders
        private ICylinder BellowUpDown => _devices.Cylinders.BellowUpDown;
        private ICylinder ChamberOpenClose => _devices.Cylinders.ChamberOpenClose;
        private ICylinder AngleValve => _devices.Cylinders.AngleValve;
        #endregion

        #region Constructors
        public ChamberProcess(Devices devices)
        {
            _devices = devices;
        }
        #endregion

        #region Process Methods
        public override bool ProcessOrigin()
        {
            return base.ProcessOrigin();
        }
        public override bool ProcessRun()
        {
            switch (Sequence)
            {
                case ESequence.Stop:
                    break;
                case ESequence.AutoRun:
                    break;
                case ESequence.Ready:
                    break;
                case ESequence.Loading:
                    break;
                case ESequence.ResinInject:
                    break;
                case ESequence.Unloading:
                    break;
                case ESequence.DummyShot:
                    break;
                case ESequence.NeedleCleaning:
                    break;
                case ESequence.DotWeighting:
                    break;
                case ESequence.HeadAssemble:
                    break;
                case ESequence.HeadDisassemble:
                    break;
                case ESequence.BubbleRemove:
                    break;
            }
            return true;
        }
        #endregion

        #region Privates
        private readonly Devices _devices;
        #endregion
    }
}
