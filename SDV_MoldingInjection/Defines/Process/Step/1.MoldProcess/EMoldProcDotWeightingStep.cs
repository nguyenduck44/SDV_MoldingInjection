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

        XAxis_DotWeightingPos_Move,
        XAxis_DotWeightingPos_Move_Wait,

        ZAxis_DotWeightingPos_Move,
        ZAxis_DotWeightingPos_Move_Wait,
        DotWeighting_Request,
        ClearFlag_DotWeighting_Request,

        ZAxis_SafetyPos_Move_End,
        ZAxis_SafetyPos_Move_End_Wait,

        End,
    }
}
