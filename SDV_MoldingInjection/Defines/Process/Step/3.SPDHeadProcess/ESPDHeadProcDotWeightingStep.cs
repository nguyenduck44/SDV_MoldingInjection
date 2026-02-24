namespace SDV_MoldingInjection.Defines
{
    public enum ESPDHeadProcDotWeightingStep
    {
        Start,

        // ----- CHARGING PHASE -----
        Request_XYAxis_DotWeightingPos_Move,

        Charge_PosVel_Calculte,

        Gate_Close,
        Gate_CloseWait,

        PAxis_ChargePos_Move,
        PAxis_ChargePos_MoveWait,

        // ----- INJECT PHASE -----
        Gate_Open,
        Gate_OpenWait,
                
        Wait_DotWeightingPos_Move,

        Balance_Zero,
        Balance_Zero_Check,

        PAxis_InjectPos_Move,
        PAxis_InjectPos_MoveWait,

        Calibrate_Weight,

        ClearFlag_Request_XYAxis_DotWeightingPos,

        End,
    }
}
