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

        CheckInputType,

        Wait_InOutHandler_Working_RequestStart,

        GetCellId1,
        Write_JigID_1,
        Wait_Write_JigIID_1,
        Validation_Reply_Jig1,
        Wait_Cell1SendBitOff,
        Wait_Cell1SendBitOff_TimeOut,

        GetCellId2,
        Write_JigID_2,
        Wait_Write_JigIID_2,
        Validation_Reply_Jig2,
        Wait_Cell2SendBitOff,
        Wait_Cell2SendBitOff_TimeOut,

        CIM_TrackOut_LeftJig,
        CIM_TrackOut_RightJig,

        Unloading_Manual,

        Unloading_WriteData,

        Load_Jig_Check,

        Chamber_CoverClose,
        Chamber_CoverCloseWait,

        End,
    }
}
