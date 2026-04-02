namespace SDV_MoldingInjection.Defines
{
    public enum EMoldProcResinInjectAddTailStep
    {
        Start,

        ZAxis_UpDistance_Move,
        ZAxis_UpDistance_Wait,

        InjectAddTail_Request,
        Wait_SDPHead_AddTail_Done,

        Update_BothJigStatus,

        ZAxis_SafetyPos_Move,
        ZAxis_SafetyPos_MoveWait,

        End,
    }
}
