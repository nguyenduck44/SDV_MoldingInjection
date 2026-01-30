using EQX.Core.Motion;
using EQX.Process;
using SDV_DotDispenser.Defines;
using SDV_DotDispenser.Defines.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDV_DotDispenser.Process
{
    public class UVProcess : ProcessBase<ESequence>
    {
        private readonly Devices _devices;

        private IMotion XAxis => _devices.Motions.UVHeadXAxis;

        public UVProcess(Devices devices)
        {
            _devices = devices;
        }
    }
}
