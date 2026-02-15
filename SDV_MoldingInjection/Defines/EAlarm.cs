namespace SDV_MoldingInjection.Defines
{
    public enum ESPDHead
    {
        SPDHead1 = 1,
        SPDHead2 = 2,
        SPDHead3 = 3,
        SPDHead4 = 4,
    }

    public enum EAlarm
    {
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

        LeftStage_YAxis_OriginFail = 1000,
        LeftStage_YAxis_MoveLoadPositionFail,

        RightStage_YAxis_OriginFail = 2000,
        RightStage_YAxis_MoveLoadPositionFail,

        DispenserHead_XAxis_OriginFail = 3000,
        DispenserHead_ZAxis_OriginFail,

        VisionInspection_XAxis_OriginFail = 4000,

        Transfer_XAxis_OriginFail = 7000,
        Transfer_ZAxis_OriginFail,
    }
}
