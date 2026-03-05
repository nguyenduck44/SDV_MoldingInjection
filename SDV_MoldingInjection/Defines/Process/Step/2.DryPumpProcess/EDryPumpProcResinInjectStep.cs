namespace SDV_MoldingInjection.Defines
{
    public enum EDryPumpProcResinInjectStep
    {
        Start,
        DryPump_Run,
        DryPump_RunWait,

        DryPump_Vacuum_RequestWait,

        AngleValve_Open_1st,
        AngleValve_OpenWait_1st,

        VacuumGauge_SpecIn_Wait_1st,

        AngleValve_Close_1st,
        AngleValve_CloseWait_1st,

        DryPump_Purge_1st,
        DryPump_Purge_Wait_1st,
        //
        AngleValve_Open_2st,
        AngleValve_OpenWait_2st,

        VacuumGauge_SpecIn_Wait_2st,

        AngleValve_Close_2st,
        AngleValve_CloseWait_2st,

        Delay_BeforeInject,

        DryPump_VacuumDone_Send,

        DryPump_WaitVentTime,
        DryPump_PurgeAndWait,

        WaitToClear_ProcOutput,

        End,
    }
}
