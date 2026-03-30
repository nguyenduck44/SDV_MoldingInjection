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
        Wait_InOutHandlerStart_Request,
        Update_Jig_Status,

        Jig_Check,

        MCR_Read,

        Chamber_CoverClose,
        Chamber_CoverCloseWait,

        End,
    }
}
