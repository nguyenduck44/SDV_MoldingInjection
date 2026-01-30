namespace SDV_BIM_Line_DotDispenser.Defines
{
    public enum ETransferProcessUnloadStep
    {
        Start,
        XY_Axis_MoveUnloadPosition,
        XY_Axis_MoveUnloadPosition_Wait,
        ZAxis_MoveUnloadPosition,
        ZAxis_MoveUnloadPosition_Wait,
        End
    }
}
