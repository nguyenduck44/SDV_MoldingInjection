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

        GetCellId1,
        Set_Cell1ReplyBitOn,
        Wait_Cell1SendBitOff,

        GetCellId2,
        Set_Cell2ReplyBitOn,
        Wait_Cell2SendBitOff,

        CheckIf_LoadOrUnload,

        Load_Jig_Check,

        CIM_Validation,

        Unload_Jig_Check,

        CIM_TrackOut,
        Unload_Wait,

        Update_Jig_Status,

        Chamber_CoverClose,
        Chamber_CoverCloseWait,

        End,
    }
}
