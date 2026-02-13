using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDV_DemoEQ.Defines
{
    public enum ETransferProcessOriginStep
    {
        Start,
        ZAxis_Origin,
        ZAxis_Origin_Wait,
        XAxis_Origin,
        XAxis_Origin_Wait,
        End
    }
}
