using SDV_DotDispenser.Defines.Devices;

namespace SDV_DotDispenser.Process
{
    public class PlasmaProcess : DDProcess
    {
        private readonly Devices _devices;

        public PlasmaProcess(Devices devices)
        {
            _devices = devices;
        }
    }
}
