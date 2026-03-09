namespace SDV_MoldingInjection.Defines
{
    public enum EMoldProcToRunStep
    {
        Start,

        ChamberClose,
        ChamberClose_Wait,

        Bellow_Down,
        Bellow_DownWait,

        ZAxis_SafetyPos_Move,
        ZAxis_SafetyPos_MoveWait,

        End,
    }
}
