using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDV_BIM_Line_DotDispenser.Defines
{
    public enum EShuttleProcessLoadStep
    {
        Start,
        XAxis_MoveLoadPosition,
        XAxis_MoveLoadPosition_Wait,
        Set_FlagRequestLoad,
        Wait_TransferLoadDone,
        End
    }
}
