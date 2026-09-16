namespace SDV_MoldingInjection.Defines
{
    public enum EMonitoringProcessMoveMultiPointStep
    {
        Start,

        Close_Chamber,
        Close_Chamber_Wait,

        Init_QueuePosition,

        QueueEmptyCheck,

        PointMove,
        PointMove_Wait,

        End
    }
}
