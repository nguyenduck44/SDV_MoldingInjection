using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDV_MoldingInjection.Defines.Process.Step._3.SPDHeadProcess
{
    public enum ESPDHeadProcessOriginStep
    {
        Start,

        GAxis_Origin,
        GAxis_OriginWait,

        GAxis_ClosePosition_Move,
        GAxis_ClosePosition_MoveWait,

        PistonCyl_Up,
        PistonCyl_UpWait,

        PAxis_Origin,
        PAxis_OriginWait,

        End,
    }
}
