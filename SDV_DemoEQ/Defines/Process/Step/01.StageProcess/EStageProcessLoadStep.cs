using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDV_DemoEQ.Defines
{
    public enum EStageProcessLoadStep
    {
        Start,
        YAxis_MoveLoadPosition,
        YAxis_MoveLoadPosition_Wait,
        MutingLightCurtain,
        Wait_SWStart,
        EnableLightCurtain,
        VacuumStatus_Check,
        End
    }
}
