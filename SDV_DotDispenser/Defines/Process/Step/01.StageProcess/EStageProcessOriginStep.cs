using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDV_DotDispenser.Defines
{
    public enum EStageProcessOriginStep
    {
        Start,

        Wait_ZAxis_HomeDone,

        YAxis_Origin,
        YAxis_Origin_Wait,
        End
    }
}
