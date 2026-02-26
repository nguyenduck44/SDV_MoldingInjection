namespace SDV_MoldingInjection.Defines
{
    public enum EMoldProcNeedleCleaningStep
    {
        Start,

        CleanCount_Check,

        XY_Axis_NeedleCleanPos_Move,
        XY_Axis_NeedleCleanPos_MoveWait,

        NzlCleanCyl_UnGrip,
        NzlCleanCyl_UnGripWait,

        Z_Axis_NeedleCleanPos_Move,
        Z_Axis_NeedleCleanPos_MoveWait,

        NzlCleanCyl_Grip,
        NzlCleanCyl_GripWait,

        Z_Axis_Up_AfterClean,
        Z_Axis_Up_AfterCleanWait,

        NzlCleanCyl_UnGrip_AfterClean,
        NzlCleanCyl_UnGrip_AfterClean_Wait,

        End,
    }
}
