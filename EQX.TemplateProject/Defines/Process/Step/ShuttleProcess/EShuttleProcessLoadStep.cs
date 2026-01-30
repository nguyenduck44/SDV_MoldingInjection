using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQX.TemplateProject.Defines
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
