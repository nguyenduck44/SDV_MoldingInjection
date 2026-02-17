using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDV_MoldingInjection.Defines
{
    public enum EMoldProcOriginStep
    {
        Start,

        CheckIfChamberOpen,

        ZAxis_Origin,
        ZAxis_OriginWait,

        XYAxis_Origin,
        XYAxis_OriginWait,

        End,
    }
}
