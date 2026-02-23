namespace SDV_MoldingInjection.Defines
{
    public enum ESPDHeadProcAssembleDisAssembleStep
    {
        Start,

        Gate_Close,
        Gate_CloseWait,

        PAxis_AssemblePos_Move,
        PAxis_AssemblePos_Move_Wait,

        AssembleCheck,
        AssembleCheckWait,

        PistonCyl_Up,
        PistonCyl_UpWait,

        PistonCyl_Down,
        PistonCyl_DownWait,

        GAxis_OpenPosition_Move,
        GAxis_OpenPosition_MoveWait,

        PAxis_BasePosition_Move,
        PAxis_BasePosition_Wait,

        End,
    }
}
