using EQX.Process;
using SDV_DotDispenser.Defines;
using SDV_DotDispenser.Defines.Devices;

namespace SDV_DotDispenser.Process
{
    public class StageProcess : ProcessBase<ESequence>
    {
        private readonly Devices _devices;
        private EPort port => Name == EProcess.StageLeft.ToString() ? EPort.Left : EPort.Right;

        public StageProcess(Devices devices)
        {
            _devices = devices;
        }
    }
}
