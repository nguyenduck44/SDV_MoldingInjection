namespace SDV_MoldingInjection.Defines
{
    public enum EMoldProcLoadingUnloadingStep
    {
        Start,

        YAxis_ReadyPos_Move,
        YAxis_ReadyPos_Wait,

        Chamber_CoverOpen,
        Chamber_CoverOpenWait,

        Transfer_Load_SendRequest,
        Transfer_Load_Wait,

        MCR_Read,

        Chamber_CoverClose,
        Chamber_CoverCloseWait,

        End,
    }
}
