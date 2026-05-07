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

        Wait_Cell1SendBitOn,
        GetCellId1,
        Set_Cell1ReplyBitOn,
        Wait_Cell1SendBitOff,

        Wait_Cell2SendBitOn,
        GetCellId2,
        Set_Cell2ReplyBitOn,
        Wait_Cell2SendBitOff,

        Wait_InOutHandlerStart_Request,

        Jig_Check,

        Update_Jig_Status,


        MCR_Read,

        Chamber_CoverClose,
        Chamber_CoverCloseWait,

        End,
    }
}
