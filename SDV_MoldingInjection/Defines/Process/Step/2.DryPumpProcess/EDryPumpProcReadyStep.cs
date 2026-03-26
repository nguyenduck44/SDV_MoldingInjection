namespace SDV_MoldingInjection.Defines
{
    public enum EDryPumpProcReadyStep
    {
        Start,
        YAxis_Position_Check,
        AngleValve_Close,
        AngleValve_Close_Wait,
        PurgeCheck,
        PurgeWait,
        BellowCylDown,
        BellowCylDown_Wait,
        ZAxis_ReadyPos_Move,
        ZAxis_ReadyPos_Wait,
        XYAxis_ReadyPos_Move,
        XYAxis_ReadyPos_Wait,
        End,
    }
}
