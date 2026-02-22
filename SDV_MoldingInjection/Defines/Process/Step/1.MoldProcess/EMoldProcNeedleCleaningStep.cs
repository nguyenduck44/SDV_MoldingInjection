namespace SDV_MoldingInjection.Defines
{
    public enum EMoldProcNeedleCleaningStep
    {
        Start,

        CleanCount_Check,

        Z_Axis_Up,
        Z_Axis_UpWait,

        XY_Axis_NeedleCleanPos_Move,
        XY_Axis_NeedleCleanPos_MoveWait,

        NzlCleanCyl_UnGrip,
        NzlCleanCyl_UnGripWait,

        Z_Axis_NeedleCleanPos_Move,
        Z_Axis_NeedleCleanPos_MoveWait,

        NzlCleanCyl_Grip,
        NzlCleanCyl_GripWait,

        Y_Axis_Cleaning_Move,
        Y_Axis_Cleaning_MoveWait,
        Y_Axis_Cleaning_MoveBack,
        Y_Axis_Cleaning_MoveBackWait,

        Z_Axis_Up_AfterClean,
        Z_Axis_Up_AfterCleanWait,

        End,
    }
}
