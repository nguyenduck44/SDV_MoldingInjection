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

        DryPump_VacuumDone_Send,

        DryPump_Purge_RequestWait,
        DryPump_PurgeAndWait,

        DryPump_PurgeDone_Send,

        WaitToClear_ProcOutput,

        End,
    }
}
