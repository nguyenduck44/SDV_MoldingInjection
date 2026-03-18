using System.ComponentModel;

namespace SDV_MoldingInjection.Defines
{
    public enum EWarning
    {
        //--------- 20 -> 499 : Global Warning ---------
        Root = 20,
        [Description("IN16~IN19 - IN69~IN72")]
        DoorOpen,
        [Description("OUT16")]
        DoorNotSafetyLock,
        MoveTargetPosition_Fail,
        [Description("IN11")]
        Warning_OverTemperature_Detected,
        [Description("IN04")]
        OPSwitchKey_Not_In_AutoMode,

        //--------- 500 -> 999 : Mold Process Warning ---------
        Mold_Chamber_OpenWarning = 500,
        Mold_Chamber_OpenFail,
        Mold_Chamber_CloseWarning,
        Mold_Chamber_CloseFail,

        XAxis_Origin_TimeOut = 550,
        XAxis_ReadyPos_MoveTimeOut,
        XAxis_InjectPos_MoveTimeOut,
        XAxis_DummyPos_MoveTimeOut,
        XAxis_DotWeightingPos_MoveTimeOut,
        XAxis_MoveNeedleCleanPos_Timeout,

        YAxis_Origin_TimeOut = 600,
        YAxis_ReadyPos_MoveTimeOut,
        YAxis_InjectPos_MoveTimeOut,
        YAxis_DummyPos_MoveTimeOut,
        YAxis_MoveNeedleCleanPos_Timeout,

        Z1Axis_Origin_TimeOut = 650,
        Z1Axis_SafetyPos_MoveTimeOut,
        Z1Axis_InjectPos_MoveTimeOut,
        Z1Axis_DummyPos_MoveTimeOut,
        Z1Axis_MoveNeedleCleanPos_Timeout,
        Z1Axis_MoveDotWeightingPos_Timeout,
        Z1Axis_MoveBubbleRemovePos_Timeout,
        Z1Axis_MoveAssembleDisassemblePos_Timeout,
        Z1Axis_Up_AddDetailPos_MoveTimeOut,

        Z2Axis_Origin_TimeOut = 700,
        Z2Axis_SafetyPos_MoveTimeOut,
        Z2Axis_InjectPos_MoveTimeOut,
        Z2Axis_DummyPos_MoveTimeOut,
        Z2Axis_MoveNeedleCleanPos_Timeout,
        Z2Axis_MoveDotWeightingPos_Timeout,
        Z2Axis_MoveBubbleRemovePos_Timeout,
        Z2Axis_MoveAssembleDisassemblePos_Timeout,
        Z2Axis_Up_AddDetailPos_MoveTimeOut,

        Z3Axis_Origin_TimeOut = 750,
        Z3Axis_SafetyPos_MoveTimeOut,
        Z3Axis_InjectPos_MoveTimeOut,
        Z3Axis_DummyPos_MoveTimeOut,
        Z3Axis_MoveNeedleCleanPos_Timeout,
        Z3Axis_MoveDotWeightingPos_Timeout,
        Z3Axis_MoveBubbleRemovePos_Timeout,
        Z3Axis_MoveAssembleDisassemblePos_Timeout,
        Z3Axis_Up_AddDetailPos_MoveTimeOut,

        Z4Axis_Origin_TimeOut = 800,
        Z4Axis_SafetyPos_MoveTimeOut,
        Z4Axis_InjectPos_MoveTimeOut,
        Z4Axis_DummyPos_MoveTimeOut,
        Z4Axis_MoveNeedleCleanPos_Timeout,
        Z4Axis_MoveDotWeightingPos_Timeout,
        Z4Axis_MoveBubbleRemovePos_Timeout,
        Z4Axis_MoveAssembleDisassemblePos_Timeout,
        Z4Axis_Up_AddDetailPos_MoveTimeOut,

        BellowCyl_DownFail = 900,
        BellowCyl_UpFail,
        Chamber_VacuumDetectWarning,
        Chamber_LeftJig_TiltDetect,
        Chamber_RightJig_TiltDetect,
        Chamber_LeftJig_InjectNotFinished,
        Chamber_RightJig_InjectNotFinished,

        Nozzle_CleanCyl_GripFail,
        Nozzle_CleanCyl_UnGripFail,

        //--------- 1000 -> 1499 : Dry Pump Process Warning ---------
        AngleValve_CloseFail = 1000,
        AngleValve_OpenFail,
        DryPump_Run_Timeout,
        Machine_Need_Calibration,
        Jig_Detected_Unload_Fail,
        Left_Jig_Tilt_State,
        Left_Jig_Not_Detect,
        Right_Jig_Tilt_State,
        Right_Jig_Not_Detect,

        //--------- 1500 -> 1999 : SPD Head #1 Process Warning ---------
        P1Axis_Origin_Timeout = 1500,
        G1Axis_Origin_Timeout,
        [Description("HEAD1_IN00")]
        H1_PistonCyl_UpFail,
        [Description("HEAD1_IN01")]
        H1_PistonCyl_DownFail,
        [Description("HEAD1_IN04")]
        H1_Assemble_Check_Timeout,
        [Description("HEAD1_IN04")]
        H1_Disassemble_Check_Timeout,
        H1_Balance_NotStable,
        H1_Balance_RequestWeight_Fail,
        H1_Balance_Zero_Fail,
        /// <summary>
        /// Balance data fail or syringe empty
        /// </summary>
        H1_Balance_ZeroWeighting_Fail,
        H1_PAxis_BubbleRemove_Timeout,
        H1_GAxis_BubbleRemove_Timeout,
        H1_BubbleRemove_InjectPosOverBasePos,
        [Description("HEAD1_IN05")]
        H1_Syringe_Not_Detected,

        //--------- 2000 -> 2499 : SPD Head #2 Process Warning ---------
        P2Axis_Origin_Timeout = 2000,
        G2Axis_Origin_Timeout,
        [Description("HEAD2_IN00")]
        H2_PistonCyl_UpFail,
        [Description("HEAD2_IN01")]
        H2_PistonCyl_DownFail,
        [Description("HEAD2_IN04")]
        H2_Assemble_Check_Timeout,
        [Description("HEAD2_IN04")]
        H2_Disassemble_Check_Timeout,
        H2_Balance_NotStable,
        H2_Balance_RequestWeight_Fail,
        H2_Balance_Zero_Fail,
        /// <summary>
        /// Balance data fail or syringe empty
        /// </summary>
        H2_Balance_ZeroWeighting_Fail,
        H2_PAxis_BubbleRemove_Timeout,
        H2_GAxis_BubbleRemove_Timeout,
        H2_BubbleRemove_InjectPosOverBasePos,
        [Description("HEAD2_IN05")]
        H2_Syringe_Not_Detected,


        //--------- 2500 -> 3000 : SPD Head #3 Process Warning ---------
        P3Axis_Origin_Timeout = 2500,
        G3Axis_Origin_Timeout,
        [Description("HEAD3_IN00")]
        H3_PistonCyl_UpFail,
        [Description("HEAD3_IN01")]
        H3_PistonCyl_DownFail,
        [Description("HEAD3_IN04")]
        H3_Assemble_Check_Timeout,
        [Description("HEAD3_IN04")]
        H3_Disassemble_Check_Timeout,
        H3_Balance_NotStable,
        H3_Balance_RequestWeight_Fail,
        H3_Balance_Zero_Fail,
        /// <summary>
        /// Balance data fail or syringe empty
        /// </summary>
        H3_Balance_ZeroWeighting_Fail,
        H3_PAxis_BubbleRemove_Timeout,
        H3_GAxis_BubbleRemove_Timeout,
        H3_BubbleRemove_InjectPosOverBasePos,
        [Description("HEAD3_IN05")]
        H3_Syringe_Not_Detected,

        //--------- 3000 -> 3499 : SPD Head #3 Process Warning ---------
        P4Axis_Origin_Timeout = 3000,
        G4Axis_Origin_Timeout,
        [Description("HEAD4_IN00")]
        H4_PistonCyl_UpFail,
        [Description("HEAD4_IN01")]
        H4_PistonCyl_DownFail,
        [Description("HEAD4_IN04")]
        H4_Assemble_Check_Timeout,
        [Description("HEAD4_IN04")]
        H4_Disassemble_Check_Timeout,
        H4_Balance_NotStable,
        H4_Balance_RequestWeight_Fail,
        H4_Balance_Zero_Fail,
        /// <summary>
        /// Balance data fail or syringe empty
        /// </summary>
        H4_Balance_ZeroWeighting_Fail,
        H4_PAxis_BubbleRemove_Timeout,
        H4_GAxis_BubbleRemove_Timeout,
        H4_BubbleRemove_InjectPosOverBasePos,
        [Description("HEAD4_IN05")]
        H4_Syringe_Not_Detected,
    }
}
