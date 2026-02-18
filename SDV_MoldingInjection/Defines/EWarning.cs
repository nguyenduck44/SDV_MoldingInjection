namespace SDV_MoldingInjection.Defines
{
    public enum EWarning
    {
        //--------- 500 -> 999 : Global Warning ---------
        Root = 500,
        DoorOpen,
        DoorNotSafetyLock,

        //--------- 1500 -> 1999 : Mold Process Warning ---------
        Mold_Chamber_OpenWarning = 1500,
        XAxis_Origin_TimeOut,
        YAxis_Origin_TimeOut,
        Z1Axis_Origin_TimeOut,
        Z2Axis_Origin_TimeOut,
        Z3Axis_Origin_TimeOut,
        Z4Axis_Origin_TimeOut,
        Z1Axis_SafetyPos_MoveTimeOut,
        Z2Axis_SafetyPos_MoveTimeOut,
        Z3Axis_SafetyPos_MoveTimeOut,
        Z4Axis_SafetyPos_MoveTimeOut,
        XAxis_ReadyPos_MoveTimeOut,
        YAxis_ReadyPos_MoveTimeOut,
        Mold_Chamber_VacuumDetectWarning,
        BellowUpDown_DownFail,


        //--------- 2500 -> 2999 : Dry Pump Process Warning ---------
        AngleValve_CloseFail,
        AngleValve_OpenFail,

        //--------- 3500 -> 3999 : SPD Head #1 Process Warning ---------
        P1Axis_Origin_TimeOut = 3500,
        G1Axis_Origin_TimeOut,
        H1_PistonCyl_UpFail,

        //--------- 4500 -> 4999 : SPD Head #2 Process Warning ---------
        P2Axis_Origin_TimeOut = 4500,
        G2Axis_Origin_TimeOut,
        H2_PistonCyl_UpFail,

        //--------- 5500 -> 5999 : SPD Head #3 Process Warning ---------
        P3Axis_Origin_TimeOut = 5500,
        G3Axis_Origin_TimeOut,
        H3_PistonCyl_UpFail,

        //--------- 6500 -> 6999 : SPD Head #3 Process Warning ---------
        P4Axis_Origin_TimeOut = 6500,
        G4Axis_Origin_TimeOut,
        H4_PistonCyl_UpFail,
    }
}
