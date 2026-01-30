namespace EQX.TemplateProject.Defines
{
    public enum ETransferProcessLoadStep
    {
        Start,
        XY_Axis_MoveLoadPosition,
        XY_Axis_MoveLoadPosition_Wait,
        Wait_ShuttleRequestLoad,
        ZAxis_MoveLoadPosition,
        ZAxis_MoveLoadPosition_Wait,

        SetFlag_TransferLoadDone,
        Wait_FlagShuttleRequestLoad_Reset,
        End
    }
}
