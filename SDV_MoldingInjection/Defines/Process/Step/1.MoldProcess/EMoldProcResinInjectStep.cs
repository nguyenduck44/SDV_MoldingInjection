namespace SDV_MoldingInjection.Defines
{
    public enum EMoldProcResinInjectStep
    {
        Start,

        InitQueue,
        StepQueue_EmptyCheck,

        XYAxis_InjectPos_Move,
        XYAxis_InjectPos_MoveWait,

        ZAxisBellowCyl_InjectPos_Move,
        ZAxisBellowCyl_InjectPos_MoveWait,


        SDPHead_Work_Request,

        DelayAfter_AngleValve_Open,

        DryPump_Vacuum_Request,
        DryPump_Vacuum_DoneWait,
        
        SDPHead_Work_DoneWait,

        Update_BothJigStatus,

        Delay_AfterInjectFinish,
        Request_DryPump_Purge,
        Wait_DryPump_PurgeEnd,

        BellowCylDown,
        BellowCylDown_Wait,

        AddTail_Check,

        ZAxis_SafetyPos_Move,
        ZAxis_SafetyPos_MoveWait,

        ClearFlag,

        End,
    }
}
