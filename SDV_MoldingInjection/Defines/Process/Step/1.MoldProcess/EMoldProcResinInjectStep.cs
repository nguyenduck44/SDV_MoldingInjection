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

        Update_BothJigStatus,

        Delay_AfterInjectFinish,
        Request_DryPump_Purge,
        Wait_DryPump_PurgeEnd,

        BellowCylDown,
        BellowCylDown_Wait,

        ZAxis_SafetyPos_Move,
        ZAxis_SafetyPos_MoveWait,

        ClearFlag,

        End,
    }
}
