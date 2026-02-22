using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDV_MoldingInjection.Defines
{
    public enum ESPDHeadProcOriginStep
    {
        Start,

        GAxis_Origin,
        GAxis_OriginWait,

        GAxis_ClosePosition_Move,
        GAxis_ClosePosition_MoveWait,

        WaitXYAxisMoveDummyPos,

        PistonCyl_Up,
        PistonCyl_UpWait,

        PAxis_Origin,
        PAxis_OriginWait,
        
        SetFlag_SPDHeadOriginDone,
        ClearFlag_SPDHeadOriginDone,

        End,
    }
}
