namespace SDV_MoldingInjection.Defines
{
    public enum EMoldProcToRunStep
    {
        Start,

        CheckIfChamberOpen,

        Bellow_Down,
        Bellow_DownWait,

        ZAxis_SafetyPos_Move,
        ZAxis_SafetyPos_MoveWait,

        End,
    }
}
