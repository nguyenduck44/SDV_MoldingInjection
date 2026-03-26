namespace SDV_MoldingInjection.Defines
{
    public enum EDryPumpProcToRunStep
    {
        Start,

        AngleValve_Close,
        AngleValve_CloseWait,

        ChamperPurge_Off,

        DryPump_Run,

        End,
    }
}
