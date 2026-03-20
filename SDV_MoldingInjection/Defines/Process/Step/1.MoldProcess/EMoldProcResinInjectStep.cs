namespace SDV_MoldingInjection.Defines
{
    public enum EMoldProcResinInjectStep
    {
        Start,

        XYAxis_InjectPos_Move,
        XYAxis_InjectPos_MoveWait,

        ZAxisBellowCyl_InjectPos_Move,
        ZAxisBellowCyl_InjectPos_MoveWait,

        DryPump_Vacuum_Request,
        DryPump_Vacuum_DoneWait,

        SDPHead_Work_Request,
        SDPHead_Work_DoneWait,

        InjectAddTail_Check,

        Wait_SPDHeadReady_InjectAddTail,
        SPDHead_InjectAddTail_Request,
        ZAxis_UpDistance_Move,
        ZAxis_UpDistance_Wait,

        SDPHead_AddTail_DoneWait,

        Update_BothJigStatus,

        ZAxis_SafetyPos_Move,
        ZAxis_SafetyPos_MoveWait,

        End,
    }
}
