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

        Bellow_Down,
        Bellow_DownWait,

        XYAxis_Origin,
        XYAxis_OriginWait,

        XYAxis_MoveDummyPos,
        XYAxis_MoveDummyPosWait,

        SetFlag_MoveDummyPosDone,
        ClearFlag_MoveDummyPosDone,

        End,
    }
}
