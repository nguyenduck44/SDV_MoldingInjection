namespace SDV_MoldingInjection.Defines
{
    public enum EMoldProcResinInjectAddTailStep
    {
        Start,

        InjectAddTail_Check,

        Wait_SPDHeadReady_InjectAddTail,
        SPDHead_InjectAddTail_Request,
        ZAxis_UpDistance_Move,
        ZAxis_UpDistance_Wait,

        SDPHead_AddTail_DoneWait,

        Update_BothJigStatus,

        ZAxis_SafetyPos_Move,
        ZAxis_SafetyPos_MoveWait,

        ClearFlag,

        End,
    }
}
