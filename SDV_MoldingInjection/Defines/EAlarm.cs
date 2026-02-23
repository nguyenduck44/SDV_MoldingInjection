namespace SDV_MoldingInjection.Defines
{
    public enum EAlarm
    {
        //--------- -1 -> 499 : Global Alarm ---------
        None = -1,

        // System Level Alarms
        MainAirNotSupplied = 0,
        MainPowerDown = 1,
        MotionAlarmDetected = 2,
        DoorOpen = 3,
        EmergencyStopActivated = 6,
        PowerMCOff = 7,
        Motion_Driver_Off,
        Motion_Limit_Detected,
        Motion_Alarm_Detected,

        //--------- 1500 -> 1499 : Mold Process Alarm ---------
        Z1Axis_MoveNeedleCleanPos_Timeout = 1500,
        Z2Axis_MoveNeedleCleanPos_Timeout,
        Z3Axis_MoveNeedleCleanPos_Timeout,
        Z4Axis_MoveNeedleCleanPos_Timeout,

        XAxis_MoveNeedleCleanPos_Timeout,
        YAxis_MoveNeedleCleanPos_Timeout,
        YAxis_MoveCleaning_Timeout,
        YAxis_MoveBackCleaning_Timeout,

        //--------- 2000 -> 2499 : Dry Pump Process Alarm ---------

        //--------- 3000 -> 3499 : SPD Head #1 Process Alarm ---------
        H1GAxis_MoveOpenPos_Timeout = 3000,
        H1GAxis_MoveClosePos_Timeout,
        H1PAxis_MoveChargePos_Timeout,
        H1PAxis_MoveInjectPos_Timeout,
        H1PAxis_MoveAssemblePos_Timeout,
        H1PAxis_MoveBasePos_Timeout,

        //--------- 4000 -> 4499 : SPD Head #2 Process Alarm ---------
        H2GAxis_MoveOpenPos_Timeout = 4000,
        H2GAxis_MoveClosePos_Timeout,
        H2PAxis_MoveChargePos_Timeout,
        H2PAxis_MoveInjectPos_Timeout,
        H2PAxis_MoveAssemblePos_Timeout,
        H2PAxis_MoveBasePos_Timeout,

        //--------- 5000 -> 5499 : SPD Head #3 Process Alarm ---------
        H3GAxis_MoveOpenPos_Timeout = 5000,
        H3GAxis_MoveClosePos_Timeout,
        H3PAxis_MoveChargePos_Timeout,
        H3PAxis_MoveInjectPos_Timeout,
        H3PAxis_MoveAssemblePos_Timeout,
        H3PAxis_MoveBasePos_Timeout,

        //--------- 6000 -> 6499 : SPD Head #3 Process Alarm ---------
        H4GAxis_MoveOpenPos_Timeout = 6000,
        H4GAxis_MoveClosePos_Timeout,
        H4PAxis_MoveChargePos_Timeout,
        H4PAxis_MoveInjectPos_Timeout,
        H4PAxis_MoveAssemblePos_Timeout,
        H4PAxis_MoveBasePos_Timeout,
    }
}
