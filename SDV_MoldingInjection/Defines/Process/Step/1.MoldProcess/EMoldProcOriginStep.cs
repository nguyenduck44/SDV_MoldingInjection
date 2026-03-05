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

        ZAxis_DummyPos_Move,
        ZAxis_DummyPos_Wait,

        SetFlag_MoveDummyPosDone,
        ClearFlag_MoveDummyPosDone,

        ZAxis_SafetyPos_Move,
        ZAxis_SafetyPos_Wait,

        End,
    }
}
