namespace SDV_MoldingInjection.Defines
{
    public enum EMonitoringProcessMoveMultiPointStep
    {
        Start,
        Init_QueuePosition,

        QueueEmptyCheck,

        PointMove,
        PointMove_Wait,

        End
    }
}
