using EQX.Core.InOut;
using EQX.Core.Motion;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.Devices;

namespace SDV_MoldingInjection.Process
{
    public class MoldProcess : MIProcess
    {
        #region Motions
        private IMotion XAxis => _devices.Motions.XAxis;
        private IMotion Z1Axis => _devices.Motions.Z1Axis;
        private IMotion Z2Axis => _devices.Motions.Z2Axis;
        private IMotion Z3Axis => _devices.Motions.Z3Axis;
        private IMotion Z4Axis => _devices.Motions.Z4Axis;
        #endregion

        #region Cylinders
        private ICylinder NozzleClean_H1 => _devices.Cylinders.NozzleClean_H1;
        private ICylinder NozzleClean_H2 => _devices.Cylinders.NozzleClean_H2;
        private ICylinder NozzleClean_H3 => _devices.Cylinders.NozzleClean_H3;
        private ICylinder NozzleClean_H4 => _devices.Cylinders.NozzleClean_H4;
        #endregion

        #region Constructors
        public MoldProcess(Devices devices)
        {
            _devices = devices;
        }
        #endregion

        #region Process Methods
        public enum EMoldProcessOriginStep
        {
            Start,

            End,
        }

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
