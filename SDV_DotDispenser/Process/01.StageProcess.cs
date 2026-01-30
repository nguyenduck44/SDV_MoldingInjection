using EQX.Core.Motion;
using EQX.Process;
using SDV_DotDispenser.Defines;
using SDV_DotDispenser.Defines.Devices;

namespace SDV_DotDispenser.Process
{
    public class StageProcess : ProcessBase<ESequence>
    {
        private readonly Devices _devices;
        private EPort port => Name == EProcess.StageLeft.ToString() ? EPort.Left : EPort.Right;
        private IMotion YAxis => port == EPort.Left ? _devices.Motions.StageY1Axis : _devices.Motions.StageY2Axis;
        public StageProcess(Devices devices)
        {
            _devices = devices;
        }
    }
}
