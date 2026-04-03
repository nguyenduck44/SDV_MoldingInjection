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
        ET_MAIN_STG_ALARM_MARK_NG = 10,

        [Description("IN14")]
        UT_MAIN_CP_PWR_MAIN_SERVO_OFF = 20,
        [Description("IN15")]
        VC_MAIN_STG_AIR_LOW_OPEN,
        [Description("IN16,IN18,IN69,IN71")]
        DO_MAIN_DOOR_INTERLOCK_OPEN,
        [Description("IN17,IN19,IN70,IN72")]
        DO_MAIN_DOOR_LOCK_SWITCH_ON,
        [Description("IN03")]
        EM_MAIN_CP_ESTOP_SERVO_OFF,
        [Description("IN13")]
        UT_MAIN_MCR_COIL_SERVO_OFF,
        MO_MAIN_STG_SRV_ALARM_ERROR,
        MO_MAIN_STG_SRV_RESET_ERROR,
        MO_MAIN_STG_SRV_PWR_SERVO_OFF,
        MO_MAIN_STG_AXIS_LIM_OPEN,
        [Description("IN09")]
        UT_MAIN_CP_SMOKE_OPEN,
        [Description("IN10")]
        TE_MAIN_CP_SENSOR_TEMP_OVER,

        //--------- 500 -> 999 : Mold ---------


        //--------- 1000 -> 1499 : Dry ---------


        //--------- 1500 -> 1999 : Head 1 ---------
        MO_SUB_H01_IDX_G_OPEN_TIMEOUT = 1500,
        MO_SUB_H01_IDX_G_CLOSE_TIMEOUT,
        MO_SUB_H01_IDX_P_CHARGE_TIMEOUT,
        MO_SUB_H01_IDX_P_INJECT_TIMEOUT,
        MO_SUB_H01_IDX_P_ASSY_TIMEOUT,
        MO_SUB_H01_IDX_P_BASE_TIMEOUT,

        MO_SUB_H02_IDX_G_OPEN_TIMEOUT = 2000,
        MO_SUB_H02_IDX_G_CLOSE_TIMEOUT,
        MO_SUB_H02_IDX_P_CHARGE_TIMEOUT,
        MO_SUB_H02_IDX_P_INJECT_TIMEOUT,
        MO_SUB_H02_IDX_P_ASSY_TIMEOUT,
        MO_SUB_H02_IDX_P_BASE_TIMEOUT,

        MO_SUB_H03_IDX_G_OPEN_TIMEOUT = 2500,
        MO_SUB_H03_IDX_G_CLOSE_TIMEOUT,
        MO_SUB_H03_IDX_P_CHARGE_TIMEOUT,
        MO_SUB_H03_IDX_P_INJECT_TIMEOUT,
        MO_SUB_H03_IDX_P_ASSY_TIMEOUT,
        MO_SUB_H03_IDX_P_BASE_TIMEOUT,

        MO_SUB_H04_IDX_G_OPEN_TIMEOUT = 3000,
        MO_SUB_H04_IDX_G_CLOSE_TIMEOUT,
        MO_SUB_H04_IDX_P_CHARGE_TIMEOUT,
        MO_SUB_H04_IDX_P_INJECT_TIMEOUT,
        MO_SUB_H04_IDX_P_ASSY_TIMEOUT,
        MO_SUB_H04_IDX_P_BASE_TIMEOUT,
    }
}
