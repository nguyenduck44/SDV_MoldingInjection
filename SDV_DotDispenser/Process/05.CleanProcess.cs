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
    public class CleanProcess : DDProcess
    {
        private readonly Devices _devices;

        public CleanProcess(Devices devices)
        {
            _devices = devices;
        }
    }
}
