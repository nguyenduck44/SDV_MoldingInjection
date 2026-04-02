namespace SDV_MoldingInjection.Defines
{
    public enum EMoldProcToRunStep
    {
        Start,

        Machine_Calibration_Check,

        Wait_DryPump_Request_Run,
        SetFlag_ChamberReadyOut,
        ChamberClose,
        ChamberClose_Wait,

        Bellow_Down,
        Bellow_DownWait,

        NozzleClean_Cyl_UnGrip,
        NozzleClean_Cyl_UnGrip_Wait,

        ZAxis_SafetyPos_Move,
        ZAxis_SafetyPos_MoveWait,

        ClearFlag_ChamberReadyOut,

        End,
    }
}
