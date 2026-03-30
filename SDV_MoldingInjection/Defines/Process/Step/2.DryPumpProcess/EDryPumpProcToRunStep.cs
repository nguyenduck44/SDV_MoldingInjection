namespace SDV_MoldingInjection.Defines
{
    public enum EDryPumpProcToRunStep
    {
        Start,

        AngleValve_Close,
        AngleValve_CloseWait,

        ChamberTorrCheck,
        ChamperPurge_On,
        ChamperPurge_Off,
        SetFlag_DryPump_PurgeDone,

        DryPump_Run,

        ClearFlag_DryPump_PurgeDone,

        End,
    }
}
