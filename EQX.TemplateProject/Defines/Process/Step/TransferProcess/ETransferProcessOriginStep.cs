using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQX.TemplateProject.Defines
{
    public enum ETransferProcessOriginStep
    {
        Start,
        ZAxis_Origin,
        ZAxis_Origin_Wait,
        XY_Axis_Origin,
        XY_Axis_Origin_Wait,
        End
    }
}
