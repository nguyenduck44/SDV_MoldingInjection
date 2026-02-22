namespace SDV_MoldingInjection.Defines
{
    public enum ESPDHeadProcAssembleDisAssembleStep
    {
        Start,

        Gate_Close,
        Gate_CloseWait,

        PAxis_AssemblePos_Move,
        PAxis_AssemblePos_Move_Wait,

        PistonCyl_Up,
        PistonCyl_UpWait,

        PistonCyl_Down,
        PistonCyl_DownWait,

        End,
    }
}
