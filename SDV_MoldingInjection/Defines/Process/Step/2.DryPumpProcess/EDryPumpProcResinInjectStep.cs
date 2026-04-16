namespace SDV_MoldingInjection.Defines
{
    public enum EDryPumpProcResinInjectStep
    {
        Start,

        DryPump_Run,
        DryPump_RunWait,

        InitQueue,
        StepQueue_EmptyCheck,

        DryPump_Vacuum_RequestWait,

        AngleValve_Open,
        AngleValve_OpenWait,

        VacuumGauge_SpecIn_Wait_1torr,

        AngleValve_Close,
        AngleValve_CloseWait,

        DryPump_Purge,
        DryPump_Purge_Wait,

        VacuumGauge_Target_Wait,

        Delay_BeforeInject,

        DryPump_VacuumDone_Send,

        DryPump_WaitVentTime,
        SetStartVent,
        DryPump_PurgeAndWait,

        Inject_PurgeRequest_Wait,
        Wait_PurgeEnd,
        SetFlag_PurgeFinish,

        WaitToClear_ProcOutput,

        End,
    }
}
