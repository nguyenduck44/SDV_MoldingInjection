namespace SDV_MoldingInjection.Defines
{
    public enum ESPDHeadProcToRunStep
    {
        Start,

        SyringeAmountCheck,
        SyringeAirCheck,

        GAxis_ClosePosition_Move,
        GAxis_ClosePosition_MoveWait,

        PistonCyl_Up,
        PistonCyl_UpWait,

        End,
    }
}
