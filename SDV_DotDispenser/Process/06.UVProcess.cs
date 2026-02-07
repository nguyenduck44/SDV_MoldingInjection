using EQX.Core.InOut;
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
    public class UVCureProcess : DDProcess
    {
        private readonly Devices _devices;

        private ICylinder UVCureCyl => _devices.Cylinders.UVCureCyl;

        public UVCureProcess(Devices devices)
        {
            _devices = devices;
        }
    }
}
