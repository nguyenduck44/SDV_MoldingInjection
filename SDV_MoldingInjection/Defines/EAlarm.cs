using System.ComponentModel;

namespace SDV_MoldingInjection.Defines
{
    /// <summary>
    /// Đặt Tên theo quy tắc: CATEGORY_POSITION_DEVICE_DESCRIPTION1_DESCRIPTION2_ERRORMODE.(samsung yêu cầu)
    /// CATEGORY_POSITION_DEVICE_DESCRIPTION1_DESCRIPTION2_ERRORMODE.
    /// Category (2 ký tự): VC VA MO IN VI CY SE CO DO TE EM UT OP ET EF.
    /// ErrorMode (bảng): ERROR (lỗi hoàn thành/kiểm tra), MARK_NG, UP_NG, OPEN, SWITCH_ON, SERVO_OFF, TEMP_OVER, TIMEOUT.
    /// Position: MAIN SUB LOADER UNLOADER PRESS INSPECTION BUFFER CONVEYOR.
    /// </summary>
    public enum EAlarm
    {
        ET_NONE = 10,
        [Description("IN14")]
        UT_MAIN_CP_PWR_MAIN_SERVO_OFF,
        [Description("IN15")]
        VC_MAIN_AIR_NOT_SUPPLIED,
        [Description("IN16,IN18,IN69,IN71")]
        DO_MAIN_DOOR_OPEN,
        [Description("IN17,IN19,IN70,IN72")]
        DO_MAIN_DOOR_INTERLOCK_ON,
        [Description("IN03")]
        EM_MAIN_CP_EMS_SERVO_OFF,
        [Description("IN13")]
        UT_MAIN_MC_OFF,
        MO_MAIN_SERVO_ALARM_DETECT,
        MO_MAIN_SERVO_RESET_FAIL,
        MO_MAIN_SERVO_PWR_OFF,
        MO_MAIN_AXIS_LIMIT_DETECT,
        [Description("IN09")]
        UT_MAIN_SMOKE_DETECT,
        [Description("IN10")]
        TE_MAIN_SENSOR_TEMP_OVER,

        //--------- 100 -> 399 : Mold ---------


        //--------- 400 -> 599 : Dry ---------


        //--------- 600 -> 699 : Head 1 ---------
        MO_SPD_H01_G1_AXIS_OPEN_POS_TIMEOUT = 600,
        MO_SPD_H01_G1_AXIS_CLOSE_POS_TIMEOUT,
        MO_SPD_H01_P1_AXIS_CHARGE_POS_TIMEOUT,
        MO_SPD_H01_P1_AXIS_INJECT_POS_TIMEOUT,
        MO_SPD_H01_P1_AXIS_ASSEMBLE_POS_TIMEOUT,
        MO_SPD_H01_P1_AXIS_BASE_POS_TIMEOUT,

        //--------- 700 -> 799 : Head 1 ---------
        MO_SPD_H02_G2_AXIS_OPEN_POS_TIMEOUT = 700,
        MO_SPD_H02_G2_AXIS_CLOSE_POS_TIMEOUT,
        MO_SPD_H02_P2_AXIS_CHARGE_POS_TIMEOUT,
        MO_SPD_H02_P2_AXIS_INJECT_POS_TIMEOUT,
        MO_SPD_H02_P2_AXIS_ASSEMBLE_POS_TIMEOUT,
        MO_SPD_H02_P2_AXIS_BASE_POS_TIMEOUT,

        //--------- 800 -> 899 : Head 1 ---------
        MO_SPD_H03_G3_AXIS_OPEN_POS_TIMEOUT = 800,
        MO_SPD_H03_G3_AXIS_CLOSE_POS_TIMEOUT,
        MO_SPD_H03_P3_AXIS_CHARGE_POS_TIMEOUT,
        MO_SPD_H03_P3_AXIS_INJECT_POS_TIMEOUT,
        MO_SPD_H03_P3_AXIS_ASSEMBLE_POS_TIMEOUT,
        MO_SPD_H03_P3_AXIS_BASE_POS_TIMEOUT,

        //--------- 900 -> 999 : Head 1 ---------
        MO_SPD_H04_G4_AXIS_OPEN_POS_TIMEOUT = 900,
        MO_SPD_H04_G4_AXIS_CLOSE_POS_TIMEOUT,
        MO_SPD_H04_P4_AXIS_CHARGE_POS_TIMEOUT,
        MO_SPD_H04_P4_AXIS_INJECT_POS_TIMEOUT,
        MO_SPD_H04_P4_AXIS_ASSSEMBLE_POS_TIMEOUT,
        MO_SPD_H04_P4_AXIS_BASE_POS_TIMEOUT,
    }
}
