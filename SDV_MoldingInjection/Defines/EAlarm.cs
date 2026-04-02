using System.ComponentModel;

namespace SDV_MoldingInjection.Defines
{
    public enum EAlarm
    {
        //--------- 10 -> 499 : Global Alarm ---------
        None = 10,

        // System Level Alarms
        [Description("IN14")]
        MainPowerDown = 20,
        [Description("IN15")]
        MainAirNotSupplied,
        [Description("IN16,IN18,IN69,IN71")]
        DoorOpen,
        [Description("IN17,IN19,IN70,IN72")]
        DoorNotSafetyLock,
        [Description("IN03")]
        EmergencyStopActivated,
        [Description("IN13")]
        PowerMC_Off,
        Motion_Alarm_Detected,
        Motion_Alarm_ResetFail,
        Motion_Driver_Off,
        Motion_Limit_Detected,
        [Description("IN09")]
        Panel_Smoke_Detected,
        [Description("IN10")]
        Alarm_OverTemperature_Detected,

        //--------- 500 -> 999 : Mold Process Alarm ---------


        //--------- 1000 -> 1499 : Dry Pump Process Alarm ---------

        //--------- 1500 -> 1999 : SPD Head #1 Process Alarm ---------
        H1GAxis_MoveOpenPos_Timeout = 1500,
        H1GAxis_MoveClosePos_Timeout,
        H1PAxis_MoveChargePos_Timeout,
        H1PAxis_MoveInjectPos_Timeout,
        H1PAxis_MoveAssemblePos_Timeout,
        H1PAxis_MoveBasePos_Timeout,

        //--------- 2000 -> 2499 : SPD Head #2 Process Alarm ---------
        H2GAxis_MoveOpenPos_Timeout = 2000,
        H2GAxis_MoveClosePos_Timeout,
        H2PAxis_MoveChargePos_Timeout,
        H2PAxis_MoveInjectPos_Timeout,
        H2PAxis_MoveAssemblePos_Timeout,
        H2PAxis_MoveBasePos_Timeout,

        //--------- 2500 -> 2999 : SPD Head #3 Process Alarm ---------
        H3GAxis_MoveOpenPos_Timeout = 2500,
        H3GAxis_MoveClosePos_Timeout,
        H3PAxis_MoveChargePos_Timeout,
        H3PAxis_MoveInjectPos_Timeout,
        H3PAxis_MoveAssemblePos_Timeout,
        H3PAxis_MoveBasePos_Timeout,

        //--------- 3000 -> 3499 : SPD Head #3 Process Alarm ---------
        H4GAxis_MoveOpenPos_Timeout = 3000,
        H4GAxis_MoveClosePos_Timeout,
        H4PAxis_MoveChargePos_Timeout,
        H4PAxis_MoveInjectPos_Timeout,
        H4PAxis_MoveAssemblePos_Timeout,
        H4PAxis_MoveBasePos_Timeout,
    }
}
