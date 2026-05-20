namespace SDV_MoldingInjection.Defines
{
    public enum EMoldProcDotWeightingStep
    {
        Start,

        Chamber_Close,
        Chamber_Close_Wait,

        SPDHead_WorkCheck,

        SPDHead_DotWeighting_H13_Check,
        SPDHead_DotWeighting_H24_Check,

        XYAxis_DotWeightingReadyPos_Move,
        XYAxis_DotWeightingReadyPos_Wait,

        ZAxis_DotWeightingPos_Move,
        ZAxis_DotWeightingPos_Move_Wait,

        DotWeighting_Request,
        ClearFlag_DotWeighting_Request,

        ZAxis_SafetyPos_Move,
        ZAxis_SafetyPos__Wait,

        End,
    }
}
