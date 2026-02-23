namespace SDV_MoldingInjection.Defines
{
    public enum EMoldProcDotWeightingStep
    {
        Start,

        ZAxis_Up,
        ZAxis_UpWait,

        XAxis_DotWeightingPos_Move,
        XAxis_DotWeightingPos_MoveWait,

        YAxis_H13DotWeightingPos_Move,
        YAxis_H13DotWeightingPos_MoveWait,

        Head13_DotWeighting_Request,
        Head13_DotWeighting_DoneWait,

        YAxis_H24DotWeightingPos_Move,
        YAxis_H24DotWeightingPos_MoveWait,

        Head24_DotWeighting_Request,
        Head24_DotWeighting_DoneWait,

        End,
    }
}
