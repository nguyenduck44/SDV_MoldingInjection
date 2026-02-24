﻿namespace SDV_MoldingInjection.Defines
{
    public enum EWarning
    {
        //--------- 500 -> 999 : Global Warning ---------
        Root = 500,
        DoorOpen,
        DoorNotSafetyLock,

        //--------- 1500 -> 1999 : Mold Process Warning ---------
        Mold_Chamber_OpenWarning = 1500,
        Mold_Chamber_OpenFail,
        Mold_Chamber_CloseWarning,
        Mold_Chamber_CloseFail,

        XAxis_Origin_TimeOut = 1550,
        XAxis_ReadyPos_MoveTimeOut,
        XAxis_InjectPos_MoveTimeOut,
        XAxis_DummyPos_MoveTimeOut,

        YAxis_Origin_TimeOut = 1600,
        YAxis_ReadyPos_MoveTimeOut,
        YAxis_InjectPos_MoveTimeOut,
        YAxis_DummyPos_MoveTimeOut,
        XAxis_DotWeightingPos_MoveTimeOut,

        Z1Axis_Origin_TimeOut = 1650,
        Z1Axis_SafetyPos_MoveTimeOut,
        Z1Axis_InjectPos_MoveTimeOut,
        Z1Axis_DummyPos_MoveTimeOut,

        Z2Axis_Origin_TimeOut = 1700,
        Z2Axis_SafetyPos_MoveTimeOut,
        Z2Axis_InjectPos_MoveTimeOut,
        Z2Axis_DummyPos_MoveTimeOut,

        Z3Axis_Origin_TimeOut = 1750,
        Z3Axis_SafetyPos_MoveTimeOut,
        Z3Axis_InjectPos_MoveTimeOut,
        Z3Axis_DummyPos_MoveTimeOut,

        Z4Axis_Origin_TimeOut = 1800,
        Z4Axis_SafetyPos_MoveTimeOut,
        Z4Axis_InjectPos_MoveTimeOut,
        Z4Axis_DummyPos_MoveTimeOut,

        BellowCyl_DownFail = 1900,
        BellowCyl_UpFail,
        Chamber_VacuumDetectWarning,
        Chamber_LeftJig_TiltDetect,
        Chamber_RightJig_TiltDetect,
        Chamber_LeftJig_InjectNotFinished,
        Chamber_RightJig_InjectNotFinished,

        Nozzle_CleanCyl_GripFail,
        Nozzle_CleanCyl_UnGripFail,

        //--------- 2500 -> 2999 : Dry Pump Process Warning ---------
        AngleValve_CloseFail = 2500,
        AngleValve_OpenFail,
        DryPump_Run_Timeout,
        Machine_Need_Calibration,

        //--------- 3500 -> 3999 : SPD Head #1 Process Warning ---------
        P1Axis_Origin_Timeout = 3500,
        G1Axis_Origin_Timeout,
        H1_PistonCyl_UpFail,
        H1_PistonCyl_DownFail,
        H1_Assemble_CheckFail,
        H1_Balance_RequestWeight_Fail,
        H1_Balance_Zero_Fail,
        /// <summary>
        /// Balance data fail or syringe empty
        /// </summary>
        H1_Balance_ZeroWeighting_Fail,
        H1_PAxis_BubbleRemove_Timeout,
        H1_GAxis_BubbleRemove_Timeout,
        H1_BubbleRemove_InjectPosOverBasePos,

        //--------- 4500 -> 4999 : SPD Head #2 Process Warning ---------
        P2Axis_Origin_Timeout = 4500,
        G2Axis_Origin_Timeout,
        H2_PistonCyl_UpFail,
        H2_PistonCyl_DownFail,
        H2_Assemble_CheckFail,
        H2_Balance_RequestWeight_Fail,
        H2_Balance_Zero_Fail,
        /// <summary>
        /// Balance data fail or syringe empty
        /// </summary>
        H2_Balance_ZeroWeighting_Fail,
        H2_PAxis_BubbleRemove_Timeout,
        H2_GAxis_BubbleRemove_Timeout,
        H2_BubbleRemove_InjectPosOverBasePos,


        //--------- 5500 -> 5999 : SPD Head #3 Process Warning ---------
        P3Axis_Origin_Timeout = 5500,
        G3Axis_Origin_Timeout,
        H3_PistonCyl_UpFail,
        H3_PistonCyl_DownFail,
        H3_Assemble_CheckFail,
        H3_Balance_RequestWeight_Fail,
        H3_Balance_Zero_Fail,
        /// <summary>
        /// Balance data fail or syringe empty
        /// </summary>
        H3_Balance_ZeroWeighting_Fail,
        H3_PAxis_BubbleRemove_Timeout,
        H3_GAxis_BubbleRemove_Timeout,
        H3_BubbleRemove_InjectPosOverBasePos,

        //--------- 6500 -> 6999 : SPD Head #3 Process Warning ---------
        P4Axis_Origin_Timeout = 6500,
        G4Axis_Origin_Timeout,
        H4_PistonCyl_UpFail,
        H4_PistonCyl_DownFail,
        H4_Assemble_CheckFail,
        H4_Balance_RequestWeight_Fail,
        H4_Balance_Zero_Fail,
        /// <summary>
        /// Balance data fail or syringe empty
        /// </summary>
        H4_Balance_ZeroWeighting_Fail,
        H4_PAxis_BubbleRemove_Timeout,
        H4_GAxis_BubbleRemove_Timeout,
        H4_BubbleRemove_InjectPosOverBasePos,
    }
}
