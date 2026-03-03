namespace SDV_MoldingInjection.Defines
{
    public enum EDryPumpProcResinInjectStep
    {
        Start,
        DryPump_Run,
        DryPump_RunWait,

        DryPump_Vacuum_RequestWait,

        AngleValve_Open,
        AngleValve_OpenWait,

        VacuumGauge_SpecIn_Wait,

        AngleValve_Close,
        AngleValve_CloseWait,

        Delay_BeforeInject,

        DryPump_VacuumDone_Send,

        DryPump_WaitVentTime,
        DryPump_PurgeAndWait,

        WaitToClear_ProcOutput,

        End,
    }
}
