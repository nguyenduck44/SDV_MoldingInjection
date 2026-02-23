namespace SDV_MoldingInjection.Defines
{
    public enum EMoldProcDotWeightingStep
    {
        Start,

        ZAxis_Up,
        ZAxis_UpWait,

        YAxis_ReadyPos_Move,
        YAxis_ReadyPos_MoveWait,

        XAxis_H13DotWeightingPos_Move,
        XAxis_H13DotWeightingPos_MoveWait,

        Head13_DotWeighting_Request,
        Head13_DotWeighting_DoneWait,

        XAxis_H24DotWeightingPos_Move,
        XAxis_H24DotWeightingPos_MoveWait,

        Head24_DotWeighting_Request,
        Head24_DotWeighting_DoneWait,

        End,
    }
}
