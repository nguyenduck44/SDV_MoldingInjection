namespace SDV_MoldingInjection.Defines
{
    public enum ERootProcessMoveMultiPointStep
    {
        Start,
        Init_QueuePosition,

        QueueEmptyCheck,

        PointMove,
        PointMove_Wait,

        End
    }
}
