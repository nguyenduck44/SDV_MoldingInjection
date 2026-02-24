namespace SDV_MoldingInjection.Defines
{
    public enum EMoldProcDotWeightingStep
    {
        Start,

        YAxis_ReadyPos_Move,
        YAxis_ReadyPos_Wait,

        ZAxis_SafetyPos_Move,
        ZAxis_SafetyPos_Wait,
        WaitSPDHeadRequest,

        XAxis_H13DotWeightingPos_Move,
        XAxis_H13DotWeightingPos_MoveWait,
        ZAxis_H13DotWeightingPos_Move,
        ZAxis_H13DotWeightingPos_MoveWait,

        Head13_DotWeighting_Request,
        ClearFlag_H13DotWeighting_Request,

        XAxis_H24DotWeightingPos_Move,
        XAxis_H24DotWeightingPos_MoveWait,
        ZAxis_H24DotWeightingPos_Move,
        ZAxis_H24DotWeightingPos_MoveWait,

        Head24_DotWeighting_Request,
        ClearFlag_H24DotWeighting_Request,

        End,
    }
}
