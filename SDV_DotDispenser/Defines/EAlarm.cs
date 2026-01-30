namespace SDV_DotDispenser.Defines
{
    public enum EAlarm
    {
        None = -1,

        // System Level Alarms
        MainAirNotSupplied = 0,
        MainPowerDown = 1,
        MotionAlarmDetected = 2,
        DoorOpen = 3,
        LightCurtainLeftDetected = 4,
        LightCurtainRightDetected = 5,
        EmergencyStopActivated = 6,
        PowerMCOff = 7,
        Motion_Driver_Off,
        Motion_Limit_Detected,
        Motion_Alarm_Detected,

        TransferZAxis_OriginTimeout = 100,
        TransferXAxis_OriginTimeout,
        TransferYAxis_OriginTimeout,


        ShuttleXAxis_OriginTimeout = 500,
    }
}
