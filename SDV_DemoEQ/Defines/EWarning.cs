namespace SDV_DemoEQ.Defines
{
    public enum EWarning
    {
        Root = 0,
        DoorOpen,
        DoorNotSafetyLock,
        InitializeTimeout,

        LeftStage_VacuumStatus_Fail = 1500,

        RightStage_VacuumStatus_Fail = 1700,

        Dispenser_ZAxis_MoveReadyFail = 3500,

        Transfer_Hand1Cyl_BackwardFail = 7500,
        Transfer_Hand1Cyl_ForwardFail,
        Transfer_Hand2Cyl_BackwardFail,
        Transfer_Hand2Cyl_ForwardFail,


    }
}
