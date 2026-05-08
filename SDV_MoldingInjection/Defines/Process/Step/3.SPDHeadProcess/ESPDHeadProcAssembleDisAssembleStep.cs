namespace SDV_MoldingInjection.Defines
{
    public enum ESPDHeadProcAssembleDisAssembleStep
    {
        Start,

        Gate_Close,
        Gate_CloseWait,

        WorkRequest_Wait,

        PAxis_AssemblePos_Move,
        PAxis_AssemblePos_Move_Wait,

        PistonCyl_Down,
        PistonCyl_DownWait,

        AssembleSensorStatus_Confirm,

        PistonCyl_Up,
        PistonCyl_UpWait,

        GAxis_OpenPosition_Move,
        GAxis_OpenPosition_MoveWait,

        PAxis_BasePosition_Move,
        PAxis_BasePosition_Wait,

        WorkDone_Send,
        WorkDone_Clear,

        End,
    }
}
