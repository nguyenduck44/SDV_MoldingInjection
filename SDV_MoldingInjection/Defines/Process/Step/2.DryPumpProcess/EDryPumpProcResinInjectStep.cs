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
        AngleValve_Open_2nd,
        AngleValve_OpenWait_2nd,

        VacuumGauge_SpecIn_Wait_2nd,

        AngleValve_Close_2nd,
        AngleValve_CloseWait_2st,

        Delay_BeforeInject,

        DryPump_VacuumDone_Send,

        DryPump_WaitVentTime,

        AngleValve_CloseCheck,
        AngleValve_CloseWait,

        SetStartVent,

        DryPump_PurgeAndWait,

        WaitToClear_ProcOutput,

        End,
    }
}
