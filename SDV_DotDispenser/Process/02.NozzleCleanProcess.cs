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
    public class NozzleCleanProcess : DDProcess
    {
        private readonly Devices _devices;

        public NozzleCleanProcess(Devices devices)
        {
            _devices = devices;
        }
    }
}
