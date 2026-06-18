namespace SDV_MoldingInjection.Defines
{
    public enum EMoldProcLoadingUnloadingStep
    {
        Start,

        YAxis_ReadyPos_Move,
        YAxis_ReadyPos_Wait,

        Chamber_CoverOpen,
        Chamber_CoverOpenWait,

        Transfer_LoadUnload_SendRequest,

        CheckIf_LoadOrUnload,

        Wait_InOutHandlerStart_Request,

        GetCellId1,
        Set_Cell1ReplyBitOn,
        Wait_Cell1SendBitOff,
        Wait_Cell1SendBitOff_TimeOut,

        GetCellId2,
        Set_Cell2ReplyBitOn,
        Wait_Cell2SendBitOff,
        Wait_Cell2SendBitOff_TimeOut,

        Load_Jig_Check,
        Update_Jig_Status,

        Chamber_CoverClose,
        Chamber_CoverCloseWait,

        CIM_TrackOut,
        Unload_Wait,

        End,
    }
}
