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
        //DoorOpen = 3,
        LightCurtainLeftDetected = 4,
        LightCurtainRightDetected = 5,
        EmergencyStopActivated = 6,
        PowerMCOff = 7,
        Motion_Driver_Off,
        Motion_Limit_Detected,
        Motion_Alarm_Detected,

        //--------- 1500 -> 1499 : Mold Process Alarm ---------

        //--------- 2000 -> 2499 : Dry Pump Process Alarm ---------

        //--------- 3000 -> 3499 : SPD Head #1 Process Alarm ---------

        //--------- 4000 -> 4499 : SPD Head #2 Process Alarm ---------

        //--------- 5000 -> 5499 : SPD Head #3 Process Alarm ---------

        //--------- 6000 -> 6499 : SPD Head #3 Process Alarm ---------
    }
}
